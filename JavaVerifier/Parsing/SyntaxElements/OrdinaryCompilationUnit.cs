using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal class OrdinaryCompilationUnit : CompilationUnit {
    public PackageDeclaration PackageDeclaration { get; }
    public IReadOnlyList<TypeDeclaration> TypeDeclarations { get; }

    public OrdinaryCompilationUnit(PackageDeclaration packageDecl,
      IReadOnlyList<ImportDeclaration> importDecls,
      IReadOnlyList<TypeDeclaration> typeDecls)
      : base(importDecls) {

      PackageDeclaration = packageDecl;
      TypeDeclarations = typeDecls;
    }
  }

}