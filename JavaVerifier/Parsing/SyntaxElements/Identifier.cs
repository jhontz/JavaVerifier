namespace JavaVerifier.Parsing.SyntaxElements {

  // Includes restricted keywords
  internal class Identifier : Token {
    public string Value { get; }

    public Identifier(string value) {
      Value = value;
    }

    public override string ToString() {
      return $"Identifier \"{Value}\"";
    }
  }

}