using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class ExportsDirective : ModuleDirective {
    public Name PackageName { get; }
    public IReadOnlyList<Name> ModuleNames { get; }

    public ExportsDirective(Name packageName, IReadOnlyList<Name> moduleNames) {
      PackageName = packageName;
      ModuleNames = moduleNames;
    }
  }

}