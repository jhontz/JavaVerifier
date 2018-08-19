using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class OpensDirective : ModuleDirective {
    public Name PackageName { get; }
    public IReadOnlyList<Name> ModuleNames { get; }

    public OpensDirective(Name packageName, IReadOnlyList<Name> moduleNames) {
      PackageName = packageName;
      ModuleNames = moduleNames;
    }
  }

}