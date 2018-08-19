using System.Collections.ObjectModel;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class RequiresDirective : ModuleDirective {
    public ReadOnlyCollection<RequiresModifier> Modifiers { get; }
    public Name ModuleName { get; }

    public RequiresDirective(ReadOnlyCollection<RequiresModifier> modifiers, Name moduleName) {
      ModuleName = moduleName;
    }
  }

}