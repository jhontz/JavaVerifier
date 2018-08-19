using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class ArrayType : Type {
    public Type ElementType { get; }
    public IReadOnlyList<IReadOnlyList<Annotation>> Dims { get; }

    public ArrayType(Type elementType, IReadOnlyList<IReadOnlyList<Annotation>> dims) {
      ElementType = elementType;
      Dims = dims;
    }
  }

}