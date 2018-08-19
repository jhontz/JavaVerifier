using System;
using System.Collections.Generic;
using System.Numerics;
using System.Globalization;
using System.Text;

using JavaVerifier.Parsing.SyntaxElements;

namespace JavaVerifier.Parsing {

  internal partial class Parser {

    public bool HasMoreTokens() {
      bool result = false;
      _input.LookAhead(() => {
        while (HasMoreInputElements()) {
          if (ReadInputElement() is Token) {
            result = true;
            break;
          }
        }
      });
      return result;
    }

    public Token ReadToken() {
      while (true) {
        if (ReadInputElement() is Token token) {
          return token;
        }
      }
    }

    public Token PeekToken() {
      Token token = null;
      LookAhead(() => {
        token = ReadToken();
      });
      return token;
    }

    public bool HasMoreInputElements() {
      if (!_input.HasMoreData()) {
        return false;
      }
      bool result = true;
      _input.LookAhead(() => {
        if (_input.Read() == '\x1A' && !_input.HasMoreData()) {
          result = false;
        }
      });
      return result;
    }

    private InputElement ReadInputElement() {
      Func<InputElement> translation = null;
      _input.LookAhead(() => {
        int c = ReadCodePoint();
        switch (c) {
          // White space
          case '\t':
          case '\f':
          case ' ':
            translation = () => {
              _input.Read();
              return new WhiteSpace();
            };
            break;
          // Line terminator
          case '\r':
          case '\n':
            translation = ReadLineTerminator;
            break;
          // Character and string literals
          case '\'':
            translation = ReadCharacterLiteral;
            break;
          case '"':
            translation = ReadStringLiteral;
            break;
          // Numbers
          case '0':
            translation = ReadNumberStartsWithZero;
            break;
          case '1':
          case '2':
          case '3':
          case '4':
          case '5':
          case '6':
          case '7':
          case '8':
          case '9':
            translation = ReadNumberStartsWithNonZeroDigit;
            break;
          // Operators and separators - simple cases
          case '(':
          case ')':
          case '{':
          case '}':
          case '[':
          case ']':
          case ';':
          case ',':
          case '@':
          case '~':
          case '?':
            translation = () => {
              _input.Read();
              return new Symbol(((char)c).ToString());
            };
            break;
          // Operators and separators - complex cases
          case '.':
            translation = ReadStartsWithPeriod;
            break;
          case ':':
            translation = ReadStartsWithColon;
            break;
          case '=':
            translation = ReadStartsWithEquals;
            break;
          case '!':
            translation = ReadStartsWithNot;
            break;
          case '*':
            translation = ReadStartsWithStar;
            break;
          case '/':
            translation = ReadStartsWithSlash;
            break;
          case '^':
            translation = ReadStartsWithXor;
            break;
          case '%':
            translation = ReadStartsWithPercent;
            break;
          case '&':
            translation = ReadStartsWithAnd;
            break;
          case '|':
            translation = ReadStartsWithOr;
            break;
          case '+':
            translation = ReadStartsWithPlus;
            break;
          case '-':
            translation = ReadStartsWithMinus;
            break;
          case '<':
            translation = ReadStartsWithLess;
            break;
          case '>':
            translation = ReadStartsWithGreater;
            break;
          // Either identifier, keyword, or crap
          default:
            if (IsJavaIdentifierStart(c)) {
              translation = ReadIdentifierCharSequence;
            }
            else {
              throw new ParseException("illegal start of input element");
            }
            break;
        } // End switch
      }); // End look ahead
      return translation();
    }

    private InputElement PeekInputElement() {
      InputElement elem = null;
      LookAhead(() => {
        elem = ReadInputElement();
      });
      return elem;
    }

    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //%% Reading comments and line terminators %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

    private Comment ReadTraditionalComment() {
      StringBuilder content = new StringBuilder();
      ReadChar('/');
      ReadChar('*');
      bool starMode = false;
      while (true) {
        char c = _input.Read();
        if (c == '*') {
          starMode = true;
        }
        else if (c == '/') {
          if (starMode) {
            break;
          }
        }
        else {
          if (starMode) {
            content.Append('*');
          }
          starMode = false;
        }
        if (!starMode) {
          content.Append(c);
        }
      }
      return new Comment(content.ToString());
    }

    private Comment ReadEndOfLineComment() {
      StringBuilder content = new StringBuilder();
      ReadChar('/');
      ReadChar('/');
      while (true) {
        if (!_input.HasMoreData()) {
          break;
        }
        char c = _input.Peek();
        if (c == '\n' || c == '\r') {
          break;
        }
        content.Append(_input.Read());
      }
      return new Comment(content.ToString());
    }

    private WhiteSpace ReadLineTerminator() {
      char c = _input.Read();
      if (c == '\n') { /* Do nothing */ }
      else if (c == '\r') {
        if (_input.HasMoreData() && _input.Peek() == '\n') {
          _input.Read();
        }
      }
      else {
        throw new ParseException("illegal start of line terminator");
      }
      return new WhiteSpace();
    }

    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //%% Reading character and string literals %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

    private CharacterLiteral ReadCharacterLiteral() {
      ReadChar('\'');
      char value;
      char peek = _input.Peek();
      if (peek == '\\') {
        value = ReadEscapeSequence();
      }
      else if (peek == '\'') {
        throw new ParseException("empty character literal");
      }
      else if (peek == '\r' || peek == '\n') {
        throw new ParseException("illegal line terminator in character literal");
      }
      else {
        value = peek;
        _input.Read();
      }
      ReadChar('\'');
      return new CharacterLiteral(value);
    }

    private StringLiteral ReadStringLiteral() {
      ReadChar('"');
      StringBuilder content = new StringBuilder();
      bool stillReading = true;
      while (stillReading) {
        char peek = _input.Peek();
        if (peek == '\\') {
          content.Append(ReadEscapeSequence());
        }
        else if (peek == '"') {
          stillReading = false;
        }
        else if (peek == '\r' || peek == '\n') {
          throw new ParseException("illegal line terminator in string literal");
        }
        else {
          content.Append(peek);
          _input.Read();
        }
      }
      ReadChar('"');
      return new StringLiteral(content.ToString());
    }

    private char ReadEscapeSequence() {
      ReadChar('\\');
      char c = _input.Read();
      switch (c) {
        case 'b':
          return '\x08';
        case 't':
          return '\x09';
        case 'n':
          return '\x0a';
        case 'f':
          return '\x0c';
        case 'r':
          return '\x0d';
        case '"':
        case '\'':
        case '\\':
          return c;
        default:
          if (DigitConverter.TryGetOctalDigitValue(c, out int v1)) {
            int value = v1;
            if (_input.HasMoreData() && DigitConverter.TryGetOctalDigitValue(_input.Peek(), out int v2)) {
              _input.Read();
              value = 8 * value + v2;
              if (v1 <= 3 && _input.HasMoreData() &&
                DigitConverter.TryGetOctalDigitValue(_input.Peek(), out int v3)) {
                _input.Read();
                value = 8 * value + v3;
              }
            }
            return (char)value;
          }
          else {
            throw new ParseException("illegal escape sequence");
          }
      }
    }

    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //%% Reading integer and float literals %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

    private Token ReadNumberStartsWithNonZeroDigit() {
      Func<Token> translation = null;
      _input.LookAhead(() => {
        foreach (int digitValue in ReadDigitValues(DigitBase.Decimal)) { } // Discard digits
        if (_input.HasMoreData()) {
          char peek = _input.Peek();
          switch (peek) {
            case '.':
            case 'e':
            case 'E':
            case 'f':
            case 'F':
            case 'd':
            case 'D':
              translation = ReadDecimalFloatLiteral;
              break;
            default:
              translation = ReadDecimalIntegerLiteral;
              break;
          }
        }
        else {
          translation = ReadDecimalIntegerLiteral;
        }
      });
      return translation();
    }

    private Token ReadNumberStartsWithZero() {
      Func<Token> translation = null;
      _input.LookAhead(() => {
        ReadChar('0');
        if (_input.HasMoreData()) {
          char peek = _input.Peek();
          switch (peek) {
            case 'x':
            case 'X':
              translation = ReadHexIntegerOrFloatLiteral;
              break;
            case 'b':
            case 'B':
              translation = ReadBinaryIntegerLiteral;
              break;
            case '_':
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
              translation = ReadOctalIntegerOrDecimalFloatLiteral;
              break;
            case '8':
            case '9':
            case '.':
            case 'e':
            case 'E':
            case 'd':
            case 'D':
            case 'f':
            case 'F':
              translation = ReadDecimalFloatLiteral;
              break;
            default:
              translation = ReadDecimalIntegerLiteral;
              break;
          }
        }
        else {
          translation = ReadDecimalIntegerLiteral;
        }
      }); // End look ahead
      return translation();
    }

    private Token ReadHexIntegerOrFloatLiteral() {
      Func<Token> translation = null;
      _input.LookAhead(() => {
        ReadChar('0');
        ReadChar((c) => (c == 'x' || c == 'X'), "expected 'x' or 'X'");
        if (_input.Peek() == '.') {
          translation = ReadHexFloatLiteral;
        }
        else {
          foreach (int digitValue in ReadDigitValues(DigitBase.Hex)) { } // Discard digits
          if (_input.HasMoreData()) {
            char peek = _input.Peek();
            if (peek == '.' || peek == 'p' || peek == 'P') {
              translation = ReadHexFloatLiteral;
            }
            else {
              translation = ReadHexIntegerLiteral;
            }
          }
          else {
            translation = ReadHexIntegerLiteral;
          }
        }
      });
      return translation();
    }

    private Token ReadOctalIntegerOrDecimalFloatLiteral() {
      // We assume input begins with '0'
      Func<Token> translation = null;
      _input.LookAhead(() => {
        foreach (int digitValue in ReadDigitValues(DigitBase.Decimal)) {
          if (digitValue == 8 || digitValue == 9) {
            translation = ReadDecimalFloatLiteral;
            break;
          }
        }
        if (translation == null) {
          // Only octal digits have been read so far
          if (_input.HasMoreData()) {
            char peek = _input.Peek();
            switch (peek) {
              case '.':
              case 'e':
              case 'E':
              case 'd':
              case 'D':
              case 'f':
              case 'F':
                translation = ReadDecimalFloatLiteral;
                break;
              default:
                translation = ReadOctalIntegerLiteral;
                break;
            }
          }
          else {
            translation = ReadOctalIntegerLiteral;
          }
        }
      }); // End look ahead
      return translation();
    }

    private Token ReadDecimalIntegerLiteral() {
      StringBuilder digits = new StringBuilder();
      bool firstDigit = true;
      long literalValue = 0;
      bool intOverflow = false;
      bool longOverflow = false;
      foreach (int digitValue in ReadDigitValues(DigitBase.Decimal)) {
        long newValue = 10 * literalValue + digitValue;
        if (newValue > int.MaxValue) {
          intOverflow = true;
        }
        if (literalValue > (long.MaxValue / 10) || newValue < 0) {
          longOverflow = true;
        }
        literalValue = newValue;
        digits.Append(digitValue);
        if (firstDigit && digitValue == 0) {
          break;
        }
        firstDigit = false;
      }
      char peek;
      if (_input.HasMoreData() && ((peek = _input.Peek()) == 'l' || peek == 'L')) {
        _input.Read();
        if (!longOverflow || (_isOperandOfUnaryMinus && digits.ToString() == "9223372036854775808")) {
          return new LongLiteral(literalValue);
        }
      }
      else if (!intOverflow || (_isOperandOfUnaryMinus && digits.ToString() == "2147483648")) {
        return new IntLiteral((int)literalValue);
      }
      throw new ParseException("integer literal value too large");
    }

    private Token ReadHexIntegerLiteral() {
      ReadChar('0');
      ReadChar((c) => (c == 'x' || c == 'X'), "expected 'x' or 'X'");
      bool readingLeadingZeros = true;
      int digitsRead = 0;
      long literalValue = 0;
      foreach (int digitValue in ReadDigitValues(DigitBase.Hex)) {
        if (digitValue > 0 || !readingLeadingZeros) {
          readingLeadingZeros = false;
          literalValue = 16 * literalValue + digitValue;
          digitsRead++;
        }
      }
      char peek;
      if (_input.HasMoreData() && ((peek = _input.Peek()) == 'l' || peek == 'L')) {
        _input.Read();
        if (digitsRead <= 16) {
          return new LongLiteral(literalValue);
        }
      }
      else if (digitsRead <= 8) {
        return new IntLiteral((int)literalValue);
      }
      throw new ParseException("integer literal value too large");
    }

    private Token ReadOctalIntegerLiteral() {
      ReadChar('0');
      while (_input.HasMoreData() && _input.Peek() == '_') {
        _input.Read();
      }
      int firstNonZeroDigitValue = -1;
      int digitsRead = 0;
      long literalValue = 0;
      foreach (int digitValue in ReadDigitValues(DigitBase.Octal)) {
        if (digitValue > 0 || firstNonZeroDigitValue != -1) {
          if (firstNonZeroDigitValue == -1) {
            firstNonZeroDigitValue = digitValue;
          }
          literalValue = 8 * literalValue + digitValue;
          digitsRead++;
        }
      }
      char peek;
      if (_input.HasMoreData() && ((peek = _input.Peek()) == 'l' || peek == 'L')) {
        _input.Read();
        if (digitsRead < 22 || (digitsRead == 22 && firstNonZeroDigitValue < 2)) {
          return new LongLiteral(literalValue);
        }
      }
      else if (digitsRead < 11 || (digitsRead == 11 && firstNonZeroDigitValue < 4)) {
        return new IntLiteral((int)literalValue);
      }
      throw new ParseException("integer literal value too large");
    }

    private Token ReadBinaryIntegerLiteral() {
      ReadChar('0');
      ReadChar((c) => (c == 'b' || c == 'B'), "expected 'b' or 'B'");
      bool readingLeadingZeros = true;
      int digitsRead = 0;
      long literalValue = 0;
      foreach (int digitValue in ReadDigitValues(DigitBase.Binary)) {
        if (digitValue > 0 || !readingLeadingZeros) {
          readingLeadingZeros = false;
          literalValue = 2 * literalValue + digitValue;
          digitsRead++;
        }
      }
      char peek;
      if (_input.HasMoreData() && ((peek = _input.Peek()) == 'l' || peek == 'L')) {
        _input.Read();
        if (digitsRead <= 64) {
          return new LongLiteral(literalValue);
        }
      }
      else if (digitsRead <= 32) {
        return new IntLiteral((int)literalValue);
      }
      throw new ParseException("integer literal value too large");
    }

    private Token ReadDecimalFloatLiteral() {
      StringBuilder content = new StringBuilder();
      bool zeroDigitsOnly = true;
      bool digitsRead = false;
      bool decimalPointRead = false;
      bool exponentPartRead = false;
      bool floatSuffixRead = false;
      bool isDouble = true;
      char peek;
      if (DigitConverter.IsDecimalDigit(_input.Peek())) {
        digitsRead = true;
        foreach (int digitValue in ReadDigitValues(DigitBase.Decimal)) {
          content.Append(digitValue);
          if (digitValue != 0) {
            zeroDigitsOnly = false;
          }
        }
      }
      if (_input.Peek() == '.') {
        decimalPointRead = true;
        _input.Read();
        content.Append('.');
      }
      if (_input.HasMoreData() && DigitConverter.IsDecimalDigit(_input.Peek())) {
        digitsRead = true;
        foreach (int digitValue in ReadDigitValues(DigitBase.Decimal)) {
          content.Append(digitValue);
          if (digitValue != 0) {
            zeroDigitsOnly = false;
          }
        }
      }
      if (_input.HasMoreData() && ((peek = _input.Peek()) == 'e' || peek == 'E')) {
        exponentPartRead = true;
        content.Append(_input.Read());
        peek = _input.Peek();
        if (peek == '+' || peek == '-') {
          content.Append(_input.Read());
        }
        foreach (int digitValue in ReadDigitValues(DigitBase.Decimal)) {
          content.Append(digitValue);
        }
      }
      if (_input.HasMoreData()) {
        peek = _input.Peek();
        if (peek == 'f' || peek == 'F' || peek == 'd' || peek == 'D') {
          if (peek == 'f' || peek == 'F') {
            isDouble = false;
          }
          _input.Read();
          floatSuffixRead = true;
        }
      }
      if (digitsRead && (decimalPointRead || exponentPartRead || floatSuffixRead)) {
        if (isDouble) {
          if (double.TryParse(content.ToString(), out double d)) {
            if (d == 0 && !zeroDigitsOnly) {
              throw new ParseException("floating point literal underflow");
            }
            return new DoubleLiteral(d);
          }
          else {
            throw new ParseException("floating point literal overflow");
          }
        }
        else {
          if (float.TryParse(content.ToString(), out float f)) {
            if (f == 0 && !zeroDigitsOnly) {
              throw new ParseException("floating point literal underflow");
            }
            return new FloatLiteral(f);
          }
          else {
            throw new ParseException("floating point literal overflow");
          }
        }
      }
      else {
        throw new ParseException("malformed floating point literal");
      }
    }

    private Token ReadHexFloatLiteral() {
      ReadChar('0');
      ReadChar((c) => (c == 'x' || c == 'X'), "expected 'x' or 'X'");
      char peek;
      long significandBits = 0;
      BigInteger currentSignicandBitIndex = 51;
      BigInteger decimalPointExponentShift = 0;
      bool digitsRead = false;
      bool nonZeroBitsRead = false;
      bool isRoundBit1 = false; // Is round bit a 1?
      bool isBitAfterRoundBit1 = false; // Is there a 1 bit somewhere after round bit?
      BigInteger exponent = 0;
      bool isExponentNegative = false;
      bool isDouble = true;
      // Check for digits before decimal point
      if (DigitConverter.IsHexDigit(_input.Peek())) {
        foreach (int bit in ReadSignificandBits()) {
          digitsRead = true;
          if (nonZeroBitsRead) {
            decimalPointExponentShift++;
            if (currentSignicandBitIndex >= 0) {
              significandBits |= ((long)bit << (int)currentSignicandBitIndex);
            }
            else if (currentSignicandBitIndex == -1) {
              isRoundBit1 = (bit == 1);
            }
            else {
              isBitAfterRoundBit1 = isBitAfterRoundBit1 || (bit == 1);
            }
            currentSignicandBitIndex--;
          }
          else if (!nonZeroBitsRead) {
            nonZeroBitsRead = (bit == 1);
          }
        }
      }
      // Check for decimal point
      if (_input.Peek() == '.') {
        _input.Read();
      }
      // Check for digits after decimal point
      if (DigitConverter.IsHexDigit(_input.Peek())) {
        foreach (int bit in ReadSignificandBits()) {
          digitsRead = true;
          if (!nonZeroBitsRead) {
            decimalPointExponentShift--;
            nonZeroBitsRead = (bit == 1);
          }
          else {
            if (currentSignicandBitIndex >= 0) {
              significandBits |= ((long)bit << (int)currentSignicandBitIndex);
            }
            else if (currentSignicandBitIndex == -1) {
              isRoundBit1 = (bit == 1);
            }
            else {
              isBitAfterRoundBit1 = isBitAfterRoundBit1 || (bit == 1);
            }
            currentSignicandBitIndex--;
          }
        }
      }
      // Check to make sure some digits have been read
      if (!digitsRead) {
        throw new ParseException("malformed floating point literal");
      }
      // Read binary exponent 
      ReadChar((c) => (c == 'p' || c == 'P'), "expected 'p' or 'P'");
      peek = _input.Peek();
      if (peek == '+' || peek == '-') {
        _input.Read();
        if (peek == '-') {
          isExponentNegative = true;
        }
      }
      foreach (int digitValue in ReadDigitValues(DigitBase.Decimal)) {
        exponent = 10 * exponent + digitValue;
      }
      exponent = isExponentNegative ? -exponent : exponent;
      exponent = exponent + decimalPointExponentShift;
      // Check for type suffix
      if (_input.HasMoreData()) {
        peek = _input.Peek();
        if (peek == 'f' || peek == 'F' || peek == 'd' || peek == 'D') {
          _input.Read();
          if (peek == 'f' || peek == 'F') {
            isDouble = false;
          }
        }
      }
      // Return early if only zero digits have been read
      if (!nonZeroBitsRead) {
        if (isDouble) {
          return new DoubleLiteral(0.0);
        }
        return new FloatLiteral(0.0f);
      }
      // From this point onward, at least one '1' bit has been read
      // Truncate significand if not double
      if (!isDouble) {
        for (int i = 0; i < 52 - 23; i++) {
          int lostBit = (int)(significandBits & 1L);
          significandBits >>= 1;
          isBitAfterRoundBit1 = isBitAfterRoundBit1 || isRoundBit1;
          isRoundBit1 = (lostBit == 1);
        }
      }
      // Handle denormalized values and check for underflow
      bool isDenormalized = false;
      long minExponent = isDouble ? -1022 : -126;
      int significandLength = isDouble ? 52 : 23;
      while (exponent < minExponent) {
        int lostBit = (int)(significandBits & 1L);
        significandBits >>= 1;
        isBitAfterRoundBit1 = isBitAfterRoundBit1 || isRoundBit1;
        isRoundBit1 = (lostBit == 1);
        exponent++;
        if (!isDenormalized) {
          isDenormalized = true;
          significandBits |= (1L << (significandLength - 1));
        }
        if (significandBits == 0L) {
          throw new ParseException("floating point literal underflow");
        }
      }
      // Perform any necessary rounding
      if (isRoundBit1) {
        int lsb = (int)(significandBits & 1L);
        if (lsb == 1 || isBitAfterRoundBit1) {
          significandBits += 1;
          long mask = isDouble ? 0xFFFFFFFFFFFFFL : 0x7FFFFFL;
          if ((significandBits & mask) == 0L) {
            exponent++;
          }
        }
      }
      // Check for overflow
      if ((isDouble && exponent >= 1024) || (!isDouble && exponent >= 128)) {
        throw new ParseException("floating point literal overflow");
      }
      // Produce final result
      if (isDouble) {
        long exponentBits = isDenormalized ? 0L : (long)exponent + 1023;
        long finalBits = significandBits | (exponentBits << 52);
        byte[] bytes = BitConverter.GetBytes(finalBits);
        return new DoubleLiteral(BitConverter.ToDouble(bytes, 0));
      }
      else {
        int exponentBits = isDenormalized ? 0 : (int)exponent + 127;
        int finalBits = (int)significandBits | (exponentBits << 23);
        byte[] bytes = BitConverter.GetBytes(finalBits);
        return new FloatLiteral(BitConverter.ToSingle(bytes, 0));
      }
    }

    // Helper method for ReadHexFloatLiteral()
    private IEnumerable<int> ReadSignificandBits() {
      foreach (int digitValue in ReadDigitValues(DigitBase.Hex)) {
        int temp = digitValue;
        for (int i = 0; i < 4; i++) {
          if ((temp & 0x8) != 0) {
            yield return 1;
          }
          else {
            yield return 0;
          }
          temp <<= 1;
        }
      }
    }

    private enum DigitBase {
      Decimal,
      Hex,
      Octal,
      Binary
    }

    private delegate bool DigitConversion(char digit, out int value);

    private IEnumerable<int> ReadDigitValues(DigitBase digitBase) {
      bool firstDigitRead = false;
      bool mustReadDigit = true;
      DigitConversion conversion = null;
      switch (digitBase) {
        case DigitBase.Decimal:
          conversion = DigitConverter.TryGetDecimalDigitValue;
          break;
        case DigitBase.Hex:
          conversion = DigitConverter.TryGetHexDigitValue;
          break;
        case DigitBase.Octal:
          conversion = DigitConverter.TryGetOctalDigitValue;
          break;
        case DigitBase.Binary:
          conversion = DigitConverter.TryGetBinaryDigitValue;
          break;
      }
      while (true) {
        if (_input.HasMoreData() && conversion(_input.Peek(), out int digitValue)) {
          _input.Read();
          yield return digitValue;
          firstDigitRead = true;
          mustReadDigit = false;
          while (_input.HasMoreData() && _input.Peek() == '_') {
            mustReadDigit = true;
            _input.Read();
          }
        }
        else {
          if (mustReadDigit) {
            if (firstDigitRead) {
              throw new ParseException("digit sequence cannot end with underscore");
            }
            else {
              throw new ParseException("digit sequence must contain at least one valid digit");
            }
          }
          else {
            break;
          }
        }
      }
    }

    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //%% Reading operators and separators %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

    // Can read separators '.', '...', and float literals
    private InputElement ReadStartsWithPeriod() {
      Func<InputElement> translation = null;
      _input.LookAhead(() => {
        ReadChar('.');
        if (!_input.HasMoreData()) {
          translation = () => {
            ReadChar('.');
            return new Symbol(".");
          };
        }
        char c = _input.Peek();
        switch (c) {
          case '.': {
            translation = () => {
              ReadChar('.');
              ReadChar('.');
              ReadChar('.');
              return new Symbol("...");
            };
            break;
          }
          case '0':
          case '1':
          case '2':
          case '3':
          case '4':
          case '5':
          case '6':
          case '7':
          case '8':
          case '9':
            translation = ReadDecimalFloatLiteral;
            break;
          default:
            translation = () => {
              ReadChar('.');
              return new Symbol(".");
            };
            break;
        } // End switch
      }); // End look ahead
      return translation();
    }

    // Can read separator '::' and operator ':'
    private InputElement ReadStartsWithColon() {
      ReadChar(':');
      if (_input.HasMoreData() && _input.Peek() == ':') {
        _input.Read();
        return new Symbol("::");
      }
      else {
        return new Symbol(":");
      }
    }

    // Can read operators '=' and '=='
    private InputElement ReadStartsWithEquals() {
      ReadChar('=');
      if (_input.HasMoreData() && _input.Peek() == '=') {
        _input.Read();
        return new Symbol("==");
      }
      else {
        return new Symbol("=");
      }
    }

    // Can read operators '!' and '!='
    private InputElement ReadStartsWithNot() {
      ReadChar('!');
      if (_input.HasMoreData() && _input.Peek() == '=') {
        _input.Read();
        return new Symbol("!=");
      }
      else {
        return new Symbol("!");
      }
    }

    // Can read operators '*' and '*='
    private InputElement ReadStartsWithStar() {
      ReadChar('*');
      if (_input.HasMoreData() && _input.Peek() == '=') {
        _input.Read();
        return new Symbol("*=");
      }
      else {
        return new Symbol("*");
      }
    }

    // Can read operators '/' and '/=', and comments
    private InputElement ReadStartsWithSlash() {
      Func<InputElement> translation = null;
      _input.LookAhead(() => {
        ReadChar('/');
        bool hasMoreData = _input.HasMoreData();
        if (hasMoreData && _input.Peek() == '=') {
          translation = () => {
            ReadChar('/');
            ReadChar('=');
            return new Symbol("/=");
          };
        }
        else if (hasMoreData && _input.Peek() == '/') {
          translation = ReadEndOfLineComment;
        }
        else if (hasMoreData && _input.Peek() == '*') {
          translation = ReadTraditionalComment;
        }
        else {
          translation = () => {
            ReadChar('/');
            return new Symbol("/");
          };
        }
      });
      return translation();
    }

    // Can read operators '^' and '^='
    private InputElement ReadStartsWithXor() {
      ReadChar('^');
      if (_input.HasMoreData() && _input.Peek() == '=') {
        _input.Read();
        return new Symbol("^=");
      }
      else {
        return new Symbol("^");
      }
    }

    // Can read operators '%' and '%='
    private InputElement ReadStartsWithPercent() {
      ReadChar('%');
      if (_input.HasMoreData() && _input.Peek() == '=') {
        _input.Read();
        return new Symbol("%=");
      }
      else {
        return new Symbol("%");
      }
    }

    // Can read operators '&', '&&', and '&='
    private InputElement ReadStartsWithAnd() {
      ReadChar('&');
      bool hasMoreData = _input.HasMoreData();
      if (hasMoreData && _input.Peek() == '&') {
        _input.Read();
        return new Symbol("&&");
      }
      else if (hasMoreData && _input.Peek() == '=') {
        _input.Read();
        return new Symbol("&=");
      }
      else {
        return new Symbol("&");
      }
    }

    // Can read operators '|', '||', and '|='
    private InputElement ReadStartsWithOr() {
      ReadChar('|');
      bool hasMoreData = _input.HasMoreData();
      if (hasMoreData && _input.Peek() == '|') {
        _input.Read();
        return new Symbol("||");
      }
      else if (hasMoreData && _input.Peek() == '=') {
        _input.Read();
        return new Symbol("|=");
      }
      else {
        return new Symbol("|");
      }
    }

    // Can read operators '+', '++', and '+='
    private InputElement ReadStartsWithPlus() {
      ReadChar('+');
      bool hasMoreData = _input.HasMoreData();
      if (hasMoreData && _input.Peek() == '+') {
        _input.Read();
        return new Symbol("++");
      }
      else if (hasMoreData && _input.Peek() == '=') {
        _input.Read();
        return new Symbol("+=");
      }
      else {
        return new Symbol("+");
      }
    }

    // Can read operators '-', '--', '-=', and '->'
    private InputElement ReadStartsWithMinus() {
      ReadChar('-');
      bool hasMoreData = _input.HasMoreData();
      if (hasMoreData && _input.Peek() == '-') {
        _input.Read();
        return new Symbol("--");
      }
      else if (hasMoreData && _input.Peek() == '=') {
        _input.Read();
        return new Symbol("-=");
      }
      else if (hasMoreData && _input.Peek() == '>') {
        _input.Read();
        return new Symbol("->");
      }
      else {
        return new Symbol("-");
      }
    }

    // Can read operators '<', '<=', '<<', and '<<='
    private InputElement ReadStartsWithLess() {
      ReadChar('<');
      bool hasMoreData = _input.HasMoreData();
      if (hasMoreData && _input.Peek() == '=') {
        _input.Read();
        return new Symbol("<=");
      }
      else if (hasMoreData && _input.Peek() == '<') {
        _input.Read();
        if (!_input.HasMoreData() && _input.Peek() == '=') {
          _input.Read();
          return new Symbol("<<=");
        }
        else {
          return new Symbol("<<");
        }
      }
      else {
        return new Symbol("<");
      }
    }

    // Can read operators '>', '>=', '>>', '>>=', '>>>', and '>>>='
    private InputElement ReadStartsWithGreater() {
      ReadChar('>');
      bool hasMoreData = _input.HasMoreData();
      if (hasMoreData && _input.Peek() == '=') {
        _input.Read();
        return new Symbol(">=");
      }
      else if (hasMoreData && _input.Peek() == '>' && !_splitRightAngledBrackets) {
        _input.Read();
        bool hasMoreData2 = _input.HasMoreData();
        if (hasMoreData2 && _input.Peek() == '=') {
          _input.Read();
          return new Symbol(">>=");
        }
        else if (hasMoreData2 && _input.Peek() == '>') {
          _input.Read();
          if (_input.HasMoreData() && _input.Peek() == '=') {
            _input.Read();
            return new Symbol(">>>=");
          }
          else {
            return new Symbol(">>>");
          }
        }
        else {
          return new Symbol(">>");
        }
      }
      else {
        return new Symbol(">");
      }
    }

    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //%% Other functions %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

    // Reads identifier, keyword, boolean literal or null literal
    private Token ReadIdentifierCharSequence() {
      StringBuilder builder = new StringBuilder();
      int cp = ReadCodePoint();
      if (!IsJavaIdentifierStart(cp)) {
        throw new ParseException("illegal start of identifier");
      }
      builder.Append(char.ConvertFromUtf32(cp));
      while (_input.HasMoreData()) {
        cp = PeekCodePoint();
        if (!IsJavaIdentifierPart(cp)) {
          break;
        }
        ReadCodePoint();
        if (!IsIdentifierIgnorable(cp)) {
          builder.Append(char.ConvertFromUtf32(cp));
        }
      }
      return TokenizeIdentifierCharSequence(builder.ToString());
    }

    private void ReadChar(char expected) {
      ReadChar((c) => (c == expected), $"expected '{expected}'");
    }

    private void ReadChar(Predicate<char> condition, string message) {
      char c = _input.Read();
      if (!condition(c)) {
        throw new ParseException(message);
      }
    }

    private int ReadCodePoint() {
      char c1 = _input.Read();
      if (char.IsLowSurrogate(c1)) {
        throw new ParseException("illegal start of code point");
      }
      else if (char.IsHighSurrogate(c1)) {
        char c2 = _input.Read();
        if (!char.IsLowSurrogate(c2)) {
          throw new ParseException("expected low surrogate");
        }
        int codePoint = char.ConvertToUtf32(c1, c2);
        return codePoint;
      }
      else {
        return c1;
      }
    }

    private int PeekCodePoint() {
      int cp = new int();
      _input.LookAhead(() => {
        cp = ReadCodePoint();
      });
      return cp;
    }

    private bool IsJavaIdentifierStart(int c) {
      string str = char.ConvertFromUtf32(c);
      var cat = CharUnicodeInfo.GetUnicodeCategory(str, 0);
      switch (cat) {
        case UnicodeCategory.UppercaseLetter:
        case UnicodeCategory.LowercaseLetter:
        case UnicodeCategory.TitlecaseLetter:
        case UnicodeCategory.ModifierLetter:
        case UnicodeCategory.OtherLetter:
        case UnicodeCategory.LetterNumber:
        case UnicodeCategory.CurrencySymbol:
        case UnicodeCategory.ConnectorPunctuation:
          return true;
        default:
          return false;
      }
    }

    private bool IsJavaIdentifierPart(int c) {
      string str = char.ConvertFromUtf32(c);
      UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(str, 0);
      switch (cat) {
        case UnicodeCategory.UppercaseLetter:
        case UnicodeCategory.LowercaseLetter:
        case UnicodeCategory.TitlecaseLetter:
        case UnicodeCategory.ModifierLetter:
        case UnicodeCategory.OtherLetter:
        case UnicodeCategory.LetterNumber:
        case UnicodeCategory.CurrencySymbol:
        case UnicodeCategory.ConnectorPunctuation:
        case UnicodeCategory.DecimalDigitNumber:
        case UnicodeCategory.SpacingCombiningMark:
        case UnicodeCategory.NonSpacingMark:
          return true;
        default:
          return IsIdentifierIgnorable(c);
      }
    }

    // Is character ignorable?
    private bool IsIdentifierIgnorable(int c) {
      if ((0x00 <= c && c <= 0x08) || (0x0E <= c && c <= 0x1B) || (0x7F <= c && c <= 0x9F)) {
        return true;
      }
      string str = char.ConvertFromUtf32(c);
      if (CharUnicodeInfo.GetUnicodeCategory(str, 0) == UnicodeCategory.Format) {
        return true;
      }
      return false;
    }

    private Token TokenizeIdentifierCharSequence(string seq) {
      switch (seq) {
        case "true":
          return new BooleanLiteral(true);
        case "false":
          return new BooleanLiteral(false);
        case "null":
          return new NullLiteral();
        case "abstract":
        case "assert":
        case "boolean":
        case "break":
        case "byte":
        case "case":
        case "catch":
        case "char":
        case "class":
        case "const":
        case "continue":
        case "default":
        case "do":
        case "double":
        case "else":
        case "enum":
        case "extends":
        case "final":
        case "finally":
        case "float":
        case "for":
        case "if":
        case "goto":
        case "implements":
        case "import":
        case "instanceof":
        case "int":
        case "interface":
        case "long":
        case "native":
        case "new":
        case "package":
        case "private":
        case "protected":
        case "public":
        case "return":
        case "short":
        case "static":
        case "strictfp":
        case "super":
        case "switch":
        case "synchronized":
        case "this":
        case "throw":
        case "throws":
        case "transient":
        case "try":
        case "void":
        case "volatile":
        case "while":
        case "_":
          return new Symbol(seq);
        // Must be an identifier or restricted keyword
        default:
          return new Identifier(seq);
      }
    }

  }

}
