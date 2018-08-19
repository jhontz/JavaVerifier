namespace JavaVerifier {

  internal struct StreamPosition {

    public StreamPosition(int index, int lineNumber, int columnNumber, bool isEndOfStream) {
      Index = index;
      LineNumber = lineNumber;
      ColumnNumber = columnNumber;
      IsEndOfStream = isEndOfStream;
    }

    public int Index { get; }
    public int LineNumber { get; }
    public int ColumnNumber { get; }
    public bool IsEndOfStream { get; }

  }

}
