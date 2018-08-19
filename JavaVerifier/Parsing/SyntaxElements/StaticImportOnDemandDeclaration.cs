namespace JavaVerifier.Parsing.SyntaxElements {

  internal class StaticImportOnDemandDeclaration : ImportDeclaration {
    public Name Name { get; }

    public StaticImportOnDemandDeclaration(Name name) {
      Name = name;
    }
  }

}
