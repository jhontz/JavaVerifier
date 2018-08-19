using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class WildcardTypeArgument : TypeArgument {
    public IReadOnlyList<Annotation> Annotations { get; }
    public WildcardBounds Bounds { get; }

    public WildcardTypeArgument(IReadOnlyList<Annotation> annotations, WildcardBounds bounds) {
      Annotations = annotations;
      Bounds = bounds;
    }
  }

}