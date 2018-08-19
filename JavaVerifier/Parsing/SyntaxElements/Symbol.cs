namespace JavaVerifier.Parsing.SyntaxElements {

  // Represents keyword, operator, or separator
  internal class Symbol : Token {
    public string Value { get; }

    public Symbol(string value) {
      Value = value;
    }

    public override string ToString() {
      return $"Symbol \"{Value}\"";
    }
  }

}