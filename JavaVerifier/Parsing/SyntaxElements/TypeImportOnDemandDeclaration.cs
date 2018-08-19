namespace JavaVerifier.Parsing.SyntaxElements {

  internal class TypeImportOnDemandDeclaration : ImportDeclaration {
    public Name Name { get; }

    public TypeImportOnDemandDeclaration(Name name) {
      Name = name;
    }
  }

}
