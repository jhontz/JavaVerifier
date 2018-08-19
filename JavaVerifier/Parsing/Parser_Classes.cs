using System;
using System.Collections.Generic;

using JavaVerifier.Parsing.SyntaxElements;

namespace JavaVerifier.Parsing {

  internal partial class Parser {

    private NormalClassDeclaration ReadNormalClassDeclaration() {
      List<Modifier> modifiers = new List<Modifier>();
      Identifier iden;
      IList<TypeParameter> typeParams = new List<TypeParameter>();
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
      throw new NotImplementedException();
    }

    private EnumDeclaration ReadEnumDeclaration() {
      throw new NotImplementedException();
    }

  }

}
