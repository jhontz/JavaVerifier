namespace JavaVerifier.Parsing.SyntaxElements {

  internal class CharacterLiteral : Token {
    public char Value { get; }

    public CharacterLiteral(char value) {
      Value = value;
    }

    public override string ToString() {
      return $"Character literal \"{Value}\"";
    }
  }

}
