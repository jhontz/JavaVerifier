using System;
using System.Collections.Generic;
using System.IO;

namespace JavaVerifier.Parsing {

  internal class RawInputStream {

    private TextReader _reader { get; }
    private Stack<char> _buffer { get; }
    private Stack<char> _markedData { get; }
    private int _lookAheadLevelCount { get; set; }

    public RawInputStream(TextReader reader) {
      _reader = reader;
      _buffer = new Stack<char>();
      _markedData = new Stack<char>();
      _lookAheadLevelCount = 0;
    }

    public bool IsEndOfStream() {
      if (_buffer.Count > 0) {
        return false;
      }
      int c = _reader.Read();
      if (c == -1) {
        return true;
      }
      _buffer.Push((char)c);
      return false;
    }

    public char Read() {
      char output;
      if (_buffer.Count == 0) {
        int rawOutput = _reader.Read();
        if (rawOutput == -1) {
          throw new ParseException("Unexpected end of stream");
        }
        output = (char)rawOutput;
      }
      else {
        output = _buffer.Pop();
      }
      if (_lookAheadLevelCount > 0) {
        _markedData.Push(output);
      }
      return output;
    }

    public char Peek() {
      char result = new char();
      LookAhead(() => {
        result = Read();
      });
      return result;
    }

    public void LookAhead(Action action) {
      int count = _markedData.Count;
      _lookAheadLevelCount++;
      action();
      _lookAheadLevelCount--;
      while (_markedData.Count != count) {
        _buffer.Push(_markedData.Pop());
      }
    }

  }

}
