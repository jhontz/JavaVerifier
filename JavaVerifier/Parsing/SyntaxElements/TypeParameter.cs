using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class TypeParameter {
    public IReadOnlyList<Annotation> Annotations { get; }
    public Identifier Identifier { get; }
    public IReadOnlyList<Type> TypeBounds { get; }

    public TypeParameter(IReadOnlyList<Annotation> annotations,
      Identifier identifier,
      IReadOnlyList<Type> typeBounds) {

      Annotations = annotations;
      Identifier = identifier;
      TypeBounds = typeBounds;
    }

  }

}
