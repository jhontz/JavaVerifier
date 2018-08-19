using System;
using System.IO;

namespace JavaVerifier.Parsing {

  internal class UnicodeTranslator {

    private RawInputStream _input { get; }
    private int _consecutiveBackslashCount { get; set; }

    public UnicodeTranslator(TextReader reader) {
      _input = new RawInputStream(reader);
      _consecutiveBackslashCount = 0;
    }

    public bool HasMoreData() {
      return !_input.IsEndOfStream();
    }

    public char Read() {
      Func<char> translation = null;
      _input.LookAhead(() => {
        int c = _input.Read();
        if (c == '\\') {
          if (_consecutiveBackslashCount % 2 == 0 && !_input.IsEndOfStream() && _input.Read() == 'u') {
            translation = ReadUnicodeEscape;
          }
          else {
            translation = () => {
              _consecutiveBackslashCount++;
              return _input.Read();
            };
          }
        }
        else {
          translation = () => {
            _consecutiveBackslashCount = 0;
            return _input.Read();
          };
        }
      });
      return translation();
    }

    public char Peek() {
      char result = new char();
      LookAhead(() => {
        result = Read();
      });
      return result;
    }

    public void LookAhead(Action action) {
      int oldCount = _consecutiveBackslashCount;
      _input.LookAhead(action);
      _consecutiveBackslashCount = oldCount;
    }

    private char ReadUnicodeEscape() {
      ReadChar('\\');
      ReadChar('u');
      while (_input.Peek() == 'u') {
        _input.Read();
      }
      char escapeCode = (char)0;
      for (int i = 0; i < 4; i++) {
        char c = _input.Read();
        if (!DigitConverter.TryGetHexDigitValue(c, out int value)) {
          throw new ParseException("expected hexidecimal digit");
        }
        escapeCode = (char)(16 * escapeCode + value);
      }
      _consecutiveBackslashCount = 0;
      return escapeCode;
    }

    private void ReadChar(char expected) {
      char c = _input.Read();
      if (c != expected) {
        throw new ParseException($"expected '{expected}'");
      }
    }

  }

}
