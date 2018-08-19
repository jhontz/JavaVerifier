namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class KeywordModifier : Modifier {
    public KeywordModifierType ModifierType { get; }

    public KeywordModifier(KeywordModifierType modifierType) {
      ModifierType = modifierType;
    }
  }

}
