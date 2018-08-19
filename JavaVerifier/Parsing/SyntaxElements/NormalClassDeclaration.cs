using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class NormalClassDeclaration : TypeDeclaration {
    public IReadOnlyList<Modifier> Modifiers { get; }
    public Identifier Identifier { get; }
    public IReadOnlyList<TypeParameter> TypeParameters { get; }
    // May be null
    public Type Superclass { get; }
    public IReadOnlyList<Type> Superinterfaces { get; }
    public ClassBody ClassBody { get; }

    public NormalClassDeclaration(IReadOnlyList<Modifier> modifiers,
      Identifier identifier,
      IReadOnlyList<TypeParameter> typeParameters,
      Type superclass,
      IReadOnlyList<Type> superinterfaces,
      ClassBody classBody) {

      Modifiers = modifiers;
      Identifier = identifier;
      TypeParameters = typeParameters;
      Superclass = superclass;
      Superinterfaces = superinterfaces;
      ClassBody = classBody;
    }

  }

}
