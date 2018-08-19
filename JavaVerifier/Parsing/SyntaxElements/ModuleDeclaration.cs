using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal class ModuleDeclaration {
    private IReadOnlyList<Annotation> Annotations { get; }
    private bool IsOpen { get; }
    private IReadOnlyList<Identifier> Identifiers { get; }
    private IReadOnlyList<ModuleDirective> ModuleDirectives { get; }

    public ModuleDeclaration(IReadOnlyList<Annotation> annotations,
      bool isOpen,
      IReadOnlyList<Identifier> identifiers,
      IReadOnlyList<ModuleDirective> moduleDirectives) {

      Annotations = annotations;
      IsOpen = isOpen;
      Identifiers = identifiers;
      ModuleDirectives = moduleDirectives;
    }
  }

}
