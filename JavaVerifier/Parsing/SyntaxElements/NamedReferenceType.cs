using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class NamedReferenceType : Type {
    public NamedReferenceType Parent { get; }
    public IReadOnlyList<Annotation> Annotations { get; }
    public Identifier Identifier { get; }
    public IReadOnlyList<TypeArgument> TypeArguments { get; }

    public NamedReferenceType(NamedReferenceType parent,
      IReadOnlyList<Annotation> annotations,
      Identifier iden,
      IReadOnlyList<TypeArgument> args) {

      Parent = parent;
      Annotations = annotations;
      Identifier = Identifier;
      TypeArguments = args;
    }
  }

}