using System;
using System.Collections.Generic;

using JavaVerifier.Parsing.SyntaxElements;

using Type = JavaVerifier.Parsing.SyntaxElements.Type;

namespace JavaVerifier.Parsing {

  internal partial class Parser {

    private Type ReadType() {
      Token tok;
      Type elementType = ReadNonArrayType();
      List<List<Annotation>> dims = new List<List<Annotation>>();
      while (IsSymbol(tok = PeekToken(), "@") || IsSymbol(tok, "[")) {
        List<Annotation> annotations = new List<Annotation>();
        while (IsSymbol(PeekToken(), "@")) {
          annotations.Add(ReadAnnotation());
        }
        ReadSymbol("[");
        ReadSymbol("]");
        dims.Add(annotations);
      }
      if (dims.Count == 0) {
        return elementType;
      }
      else {
        return new ArrayType(elementType, dims);
      }
    }

    private Type ReadNonArrayType() {
      Token tok;
      Func<Type> func = null;
      LookAhead(() => {
        // Discard annotations
        while (IsSymbol(PeekToken(), "@")) {
          ReadAnnotation();
        }
        tok = PeekToken();
        if (tok is Identifier) {
          func = ReadNamedReferenceType;
        }
        else if (IsSymbol(tok, "boolean") || IsSymbol(tok, "byte") || IsSymbol(tok, "char")
          || IsSymbol(tok, "double") || IsSymbol(tok, "float") || IsSymbol(tok, "int")
          || IsSymbol(tok, "long") || IsSymbol(tok, "short")) {

          func = ReadPrimitiveType;
        }
        else {
          throw new ParseException("illegal start of type");
        }
      });
      return func();
    }

    private PrimitiveType ReadPrimitiveType() {
      List<Annotation> annotations = new List<Annotation>();
      PrimitiveTypeType type;
      Token tok;
      while (IsSymbol(PeekToken(), "@")) {
        annotations.Add(ReadAnnotation());
      }
      tok = ReadToken();
      if (IsSymbol(tok, "boolean")) {
        type = PrimitiveTypeType.Boolean;
      }
      else if (IsSymbol(tok, "byte")) {
        type = PrimitiveTypeType.Byte;
      }
      else if (IsSymbol(tok, "char")) {
        type = PrimitiveTypeType.Char;
      }
      else if (IsSymbol(tok, "double")) {
        type = PrimitiveTypeType.Double;
      }
      else if (IsSymbol(tok, "float")) {
        type = PrimitiveTypeType.Float;
      }
      else if (IsSymbol(tok, "int")) {
        type = PrimitiveTypeType.Int;
      }
      else if (IsSymbol(tok, "long")) {
        type = PrimitiveTypeType.Long;
      }
      else if (IsSymbol(tok, "short")) {
        type = PrimitiveTypeType.Short;
      }
      else {
        throw new ParseException("illegal primitive type");
      }
      return new PrimitiveType(annotations, type);
    }

    private NamedReferenceType ReadNamedReferenceType() {
      NamedReferenceType current = null;
      bool keepReading = true;
      while (keepReading) {
        List<Annotation> annotations = new List<Annotation>();
        Identifier iden;
        List<TypeArgument> args = new List<TypeArgument>();
        while (IsSymbol(PeekToken(), "@")) {
          annotations.Add(ReadAnnotation());
        }
        iden = ReadIdentifier();
        // Check for type arguments
        if (IsSymbol(PeekToken(), "<")) {
          ReadSymbol("<");
          bool readArgs = true;
          while (readArgs) {
            args.Add(ReadTypeArgument());
          }
          if (IsSymbol(PeekToken(), ",")) {
            ReadSymbol(",");
          }
          else {
            readArgs = false;
          }
          _splitRightAngledBrackets = true;
          ReadSymbol(">");
          _splitRightAngledBrackets = false;
        }
        current = new NamedReferenceType(current, annotations, iden, args);
        // Check whether to keep reading
        if (!IsSymbol(PeekToken(), ".")) {
          keepReading = false;
        }
      }
      return current;
    }

    private TypeParameter ReadTypeParameter() {
      List<Annotation> annotations = new List<Annotation>();
      Identifier iden;
      List<Type> bounds = new List<Type>();
      while (IsSymbol(PeekToken(), "@")) {
        annotations.Add(ReadAnnotation());
      }
      iden = ReadIdentifier();
      if (IsSymbol(PeekToken(), "extends")) {
        ReadSymbol("extends");
        bool readBounds = true;
        while (readBounds) {
          bounds.Add(ReadType());
          if (IsSymbol(PeekToken(), "&")) {
            ReadSymbol("&");
          }
          else {
            readBounds = false;
          }
        }
      }
      return new TypeParameter(annotations, iden, bounds);
    }

    private TypeArgument ReadTypeArgument() {
      if (IsSymbol(PeekToken(), "?")) {
        return ReadWildcardTypeArgument();
      }
      else {
        return new ReferenceTypeTypeArgument(ReadType());
      }
    }

    private WildcardTypeArgument ReadWildcardTypeArgument() {
      Token tok;
      List<Annotation> annotations = new List<Annotation>();
      WildcardBounds bounds;
      while (IsSymbol(PeekToken(), "@")) {
        annotations.Add(ReadAnnotation());
      }
      ReadSymbol("?");
      tok = ReadToken();
      if (IsSymbol(tok, "extends")) {
        bounds = new WildcardBounds(ReadType(), WildcardBoundsType.Extends);
      }
      else if (IsSymbol(tok, "super")) {
        bounds = new WildcardBounds(ReadType(), WildcardBoundsType.Super);
      }
      else {
        bounds = null;
      }
      return new WildcardTypeArgument(annotations, bounds);
    }

  }

}
