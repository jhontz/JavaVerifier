using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal abstract class CompilationUnit {
    public IReadOnlyList<ImportDeclaration> ImportDeclarations { get; }

    public CompilationUnit(IReadOnlyList<ImportDeclaration> importDecls) {
      ImportDeclarations = importDecls;
    }
  }
}