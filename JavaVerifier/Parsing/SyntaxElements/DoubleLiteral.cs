namespace JavaVerifier.Parsing.SyntaxElements {

  internal class DoubleLiteral : Token {
    public double Value { get; }

    public DoubleLiteral(double value) {
      Value = value;
    }

    public override string ToString() {
      return $"Double literal \"{Value}\"";
    }
  }

}
