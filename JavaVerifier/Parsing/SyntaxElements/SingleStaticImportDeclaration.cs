namespace JavaVerifier.Parsing.SyntaxElements {

  internal class SingleStaticImportDeclaration : ImportDeclaration {
    public Name Name { get; }
    public Identifier Identifier { get; }

    public SingleStaticImportDeclaration(Name name, Identifier identifier) {
      Name = name;
      Identifier = identifier;
    }
  }

}