using System;
using System.Collections.Generic;
using System.IO;

using JavaVerifier.Parsing.SyntaxElements;

namespace JavaVerifier.Parsing {

  internal partial class Parser {

    private UnicodeTranslator _input { get; }

    private bool _isOperandOfUnaryMinus { get; set; }
    private bool _splitRightAngledBrackets { get; set; }

    public Parser(TextReader reader) {
      _input = new UnicodeTranslator(reader);
      _isOperandOfUnaryMinus = false;
      _splitRightAngledBrackets = false;
    }

    public void LookAhead(Action action) {
      bool oldIsOperandOfUnaryMinus = _isOperandOfUnaryMinus;
      bool oldIsInTypeContext = _splitRightAngledBrackets;
      _input.LookAhead(action);
      _splitRightAngledBrackets = oldIsInTypeContext;
      _isOperandOfUnaryMinus = oldIsOperandOfUnaryMinus;
    }

    private Annotation ReadAnnotation() {
      // TODO: Implement
      throw new NotImplementedException();
    }

    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //%% Other useful parsing functions %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

    // Read dotted identifier sequence
    private List<Identifier> ReadIdentifierSequence() {
      List<Identifier> identifiers = new List<Identifier>();
      bool readIdens = true;
      while (readIdens) {
        identifiers.Add(ReadIdentifier());
        if (HasMoreTokens() && IsSymbol(PeekToken(), ".")) {
          ReadSymbol(".");
        }
        else {
          readIdens = false;
        }
      }
      return identifiers;
    }

    // Read any identifier

    private Identifier ReadIdentifier() {
      return VerifyIdentifier(ReadToken());
    }

    private Identifier VerifyIdentifier(Token tok) {
      if (!(tok is Identifier iden)) {
        throw new ParseException("expected identifier");
      }
      return iden;
    }

    // Read identifier with specific value

    private Identifier ReadSpecificIdentifier(string value) {
      return VerifySpecificIdentifier(ReadToken(), value);
    }

    private Identifier VerifySpecificIdentifier(Token tok, string value) {
      if (!IsSpecificIdentifier(tok, value)) {
        throw new ParseException("expected \"{value}\"");
      }
      return (Identifier)tok;
    }

    private bool IsSpecificIdentifier(Token tok, string value) {
      if (!(tok is Identifier iden)) {
        return false;
      }
      return (iden.Value == value);
    }

    // Read symbol with specific value

    private Symbol ReadSymbol(string value) {
      return VerifySymbol(ReadToken(), value);
    }

    private Symbol VerifySymbol(Token tok, string value) {
      if (!IsSymbol(tok, value)) {
        throw new ParseException($"expected \"{value}\"");
      }
      return (Symbol)tok;
    }

    private bool IsSymbol(Token tok, string value) {
      if (!(tok is Symbol sym) || sym.Value != value) {
        return false;
      }
      return true;
    }

  }

}
