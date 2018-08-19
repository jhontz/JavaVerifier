namespace JavaVerifier.Parsing.SyntaxElements {

  internal class IntLiteral : Token {
    public int Value { get; }

    public IntLiteral(int value) {
      Value = value;
    }

    public override string ToString() {
      return $"Int literal \"{Value}\"";
    }
  }

}
