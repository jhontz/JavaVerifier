namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class UsesDirective : ModuleDirective {
    public Name TypeName { get; }

    public UsesDirective(Name typeName) {
      TypeName = typeName;
    }

  }

}