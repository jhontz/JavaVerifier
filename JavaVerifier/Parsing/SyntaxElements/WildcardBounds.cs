namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class WildcardBounds {
    public Type Bound { get; }
    public WildcardBoundsType BoundsType { get; }

    public WildcardBounds(Type bound, WildcardBoundsType boundsType) {
      Bound = bound;
      BoundsType = boundsType;
    }
  }

}