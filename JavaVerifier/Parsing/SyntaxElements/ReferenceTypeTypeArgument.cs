namespace JavaVerifier.Parsing.SyntaxElements {

  internal class ReferenceTypeTypeArgument : TypeArgument {
    public Type Type { get; }

    public ReferenceTypeTypeArgument(Type type) {
      Type = type;
    }
  }

}
