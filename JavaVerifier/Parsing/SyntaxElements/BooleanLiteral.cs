namespace JavaVerifier.Parsing.SyntaxElements {

  internal class BooleanLiteral : Token {
    public bool Value { get; }

    public BooleanLiteral(bool value) {
      Value = value;
    }

    public override string ToString() {
      return $"Boolean literal \"{Value.ToString().ToLower()}\"";
    }
  }

}
