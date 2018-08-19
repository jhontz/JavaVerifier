using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal class PackageDeclaration {

    public IReadOnlyList<Annotation> Modifiers { get; }
    public IReadOnlyList<Identifier> Identifiers { get; }

    public PackageDeclaration(IReadOnlyList<Annotation> modifiers, IReadOnlyList<Identifier> identifiers) {

      Modifiers = modifiers;
      Identifiers = identifiers;
    }

  }

}
