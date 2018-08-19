namespace JavaVerifier.Parsing {

  internal static class DigitConverter {

    public static bool IsOctalDigit(char digit) {
      return TryGetOctalDigitValue(digit, out int _);
    }
    
    public static bool TryGetOctalDigitValue(char digit, out int value) {
      if ('0' <= digit && digit <= '7') {
        value = digit - 48;
        return true;
      }
      else {
        value = 0;
        return false;
      }
    }

    public static bool IsDecimalDigit(char digit) {
      return TryGetDecimalDigitValue(digit, out int _);
    }

    public static bool TryGetDecimalDigitValue(char digit, out int value) {
      if ('0' <= digit && digit <= '9') {
        value = digit - 48;
        return true;
      }
      else {
        value = 0;
        return false;
      }
    }

    public static bool IsHexDigit(char digit) {
      return TryGetHexDigitValue(digit, out int _);
    }

    public static bool TryGetHexDigitValue(char digit, out int value) {
      if ('0' <= digit && digit <= '9') {
        value = digit - 48;
        return true;
      }
      else if ('a' <= digit && digit <= 'f') {
        value = digit - 87;
        return true;
      }
      else if ('A' <= digit && digit <= 'F') {
        value = digit - 55;
        return true;
      }
      else {
        value = 0;
        return false;
      }
    }

    public static bool IsBinaryDigit(char digit) {
      return TryGetBinaryDigitValue(digit, out int _);
    }

    public static bool TryGetBinaryDigitValue(char digit, out int value) {
      if ('0' <= digit && digit <= '1') {
        value = digit - 48;
        return true;
      }
      else {
        value = 0;
        return false;
      }
    }

  }

}
