namespace JavaVerifier.Parsing.SyntaxElements {

  internal class LongLiteral : Token {
    public long Value { get; }

    public LongLiteral(long value) {
      Value = value;
    }

    public override string ToString() {
      return $"Long literal \"{Value}\"";
    }
  }

}