using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class ClassBody {
    public IReadOnlyList<ClassBodyDeclaration> ClassBodyDeclarations { get; }

    public ClassBody(IReadOnlyList<ClassBodyDeclaration> decls) {
      ClassBodyDeclarations = decls;
    }
  }

}
