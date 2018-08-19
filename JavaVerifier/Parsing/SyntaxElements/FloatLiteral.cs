namespace JavaVerifier.Parsing.SyntaxElements {

  internal class FloatLiteral : Token {
    public float Value { get; }

    public FloatLiteral(float value) {
      Value = value;
    }

    public override string ToString() {
      return $"Float literal \"{Value}\"";
    }
  }

}
