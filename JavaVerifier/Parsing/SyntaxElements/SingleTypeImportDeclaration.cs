namespace JavaVerifier.Parsing.SyntaxElements {

  internal class SingleTypeImportDeclaration : ImportDeclaration {
    public Name Name { get; }

    public SingleTypeImportDeclaration(Name name) {
      Name = name;
    }
  }

}