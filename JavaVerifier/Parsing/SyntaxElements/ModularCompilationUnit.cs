using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal class ModularCompilationUnit : CompilationUnit {
    public ModuleDeclaration ModuleDeclaration { get; }

    public ModularCompilationUnit(IReadOnlyList<ImportDeclaration> importDecls,
      ModuleDeclaration moduleDecl)
      : base(importDecls) {

      ModuleDeclaration = moduleDecl;
    }
  }

}