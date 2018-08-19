using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class ProvidesDirective : ModuleDirective {
    public Name ServiceName { get; }
    public IReadOnlyList<Name> ServiceProviderNames { get; }

    public ProvidesDirective(Name serviceName, IReadOnlyList<Name> serviceProviderNames) {
      ServiceName = serviceName;
      ServiceProviderNames = serviceProviderNames;
    }
  }

}