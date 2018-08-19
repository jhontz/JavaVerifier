using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class PrimitiveType : Type {
    IReadOnlyList<Annotation> Annotations { get; }
    PrimitiveTypeType Type { get; }

    public PrimitiveType(IReadOnlyList<Annotation> annotations, PrimitiveTypeType type) {
      Annotations = annotations;
      Type = type;
    }
  }

}