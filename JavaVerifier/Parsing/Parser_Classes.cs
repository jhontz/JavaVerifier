using System;
using System.Collections.Generic;

using JavaVerifier.Parsing.SyntaxElements;

using Type = JavaVerifier.Parsing.SyntaxElements.Type;

namespace JavaVerifier.Parsing {

  internal partial class Parser {

    private NormalClassDeclaration ReadNormalClassDeclaration() {
      List<Modifier> modifiers = new List<Modifier>();
      Identifier iden;
      List<TypeParameter> typeParams = new List<TypeParameter>();
      Type superclass = null;
      List<Type> superinterfaces = new List<Type>();
      ClassBody body;
      while (!IsSymbol(PeekToken(), "class")) {
        modifiers.Add(ReadModifier());
      }
      ReadSymbol("class");
      iden = ReadIdentifier();
      if (IsSymbol(PeekToken(), "<")) {
        ReadSymbol("<");
        bool readParams = true;
        while (readParams) {
          typeParams.Add(ReadTypeParameter());
          if (IsSymbol(PeekToken(), ",")) {
            ReadSymbol(",");
          }
          else {
            readParams = false;
          }
        }
        ReadSymbol(">");
      }
      if (IsSymbol(PeekToken(), "extends")) {
        ReadSymbol("extends");
        superclass = ReadType();
      }
      if (IsSymbol(PeekToken(), "implements")) {
        ReadSymbol("implements");
        superinterfaces.Add(ReadType());
      }
      body = ReadClassBody();
      return new NormalClassDeclaration(modifiers, iden, typeParams, superclass, superinterfaces, body);
    }

    private ClassBody ReadClassBody() {
      throw new NotImplementedException();
    }

    private EnumDeclaration ReadEnumDeclaration() {
      throw new NotImplementedException();
    }

  }

}
