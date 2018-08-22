using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class RequiresDirective : ModuleDirective {
    public IReadOnlyCollection<RequiresModifier> Modifiers { get; }
    public Name ModuleName { get; }

    public RequiresDirective(IReadOnlyCollection<RequiresModifier> modifiers, Name moduleName) {
      ModuleName = moduleName;
    }
  }

}