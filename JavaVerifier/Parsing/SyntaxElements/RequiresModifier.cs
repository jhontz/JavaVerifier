namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class RequiresModifier {
    public RequiresModifierType ModifierType { get; }

    public RequiresModifier(RequiresModifierType type) {
      ModifierType = type;
    }

  }

}