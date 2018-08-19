namespace JavaVerifier.Parsing.SyntaxElements {

  internal class StringLiteral : Token {
    public string Value { get; }

    public StringLiteral(string value) {
      Value = value;
    }

    public override string ToString() {
      return $"String literal \"{Value}\"";
    }
  }

}
