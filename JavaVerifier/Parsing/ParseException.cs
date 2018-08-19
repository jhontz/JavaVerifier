using Exception = System.Exception;

namespace JavaVerifier.Parsing {

  internal class ParseException : Exception {

    public ParseException() : base() { }
    public ParseException(string message) : base(message) { }
    public ParseException(string message, Exception inner) : base(message, inner) { }

  }

}
