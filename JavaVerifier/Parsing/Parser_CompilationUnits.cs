using System;
using System.Collections.Generic;

using JavaVerifier.Parsing.SyntaxElements;

namespace JavaVerifier.Parsing {

  internal partial class Parser {

    public CompilationUnit ReadCompilationUnit() {
      Func<CompilationUnit> translation = ReadOrdinaryCompilationUnit;
      LookAhead(() => {
        // Discard any import statements appearing at start of file
        while (HasMoreTokens() && IsSymbol(PeekToken(), "import")) {
          ReadImportDeclaration();
        }
        // Discard any annotations preceding package/module/type declaration
        DiscardAnnotationsCompilationUnit();
        if (HasMoreTokens()) {
          Token peek = PeekToken();
          if (IsSpecificIdentifier(peek, "open") || IsSpecificIdentifier(peek, "module")) {
            translation = ReadModularCompilationUnit;
          }
        }
      });
      return translation();
    }

    private OrdinaryCompilationUnit ReadOrdinaryCompilationUnit() {
      PackageDeclaration packageDecl = null;
      List<ImportDeclaration> importDecls = new List<ImportDeclaration>();
      List<TypeDeclaration> typeDecls = new List<TypeDeclaration>();
      // Check for package declaration
      if (HasMoreTokens()) {
        Action action = () => { }; // Do nothing by default
        LookAhead(() => {
          DiscardAnnotationsCompilationUnit();
          if (IsSymbol(PeekToken(), "package")) {
            action = () => {
              packageDecl = ReadPackageDeclaration();
            };
          }
        });
        action();
      }
      // Check for import declarations
      while (HasMoreTokens()) {
        if (IsSymbol(PeekToken(), "import")) {
          importDecls.Add(ReadImportDeclaration());
        }
      }
      // Check for type declarations
      while (HasMoreTokens()) {
        typeDecls.Add(ReadTypeDeclaration());
      }
      return new OrdinaryCompilationUnit(packageDecl, importDecls.AsReadOnly(), typeDecls.AsReadOnly());
    }

    private ModularCompilationUnit ReadModularCompilationUnit() {
      List<ImportDeclaration> importDecls = new List<ImportDeclaration>();
      ModuleDeclaration moduleDecl = null;
      // Read any import declarations
      // Cannot be end of stream
      while (IsSymbol(PeekToken(), "import")) {
        importDecls.Add(ReadImportDeclaration());
      }
      // Now read module declaration
      moduleDecl = ReadModuleDeclaration();
      return new ModularCompilationUnit(importDecls.AsReadOnly(), moduleDecl);
      // TODO: implement
      throw new NotImplementedException();
    }

    /* Called when parsing compilation unit to discard any annotations that
       precede package, module, or type declarations */
    private void DiscardAnnotationsCompilationUnit() {
      Token peek;
      while (IsSymbol(peek = PeekToken(), "@")) {
        // Need to keep in mind that token "@" may denote an annotation type declaration, not an annotation
        Action action = () => { }; // Do nothing by default
        LookAhead(() => {
          ReadSymbol("@");
          if (!IsSymbol(peek = PeekToken(), "interface")) {
            action = () => { ReadAnnotation(); };
          }
        });
        action();
      }
    }

    private PackageDeclaration ReadPackageDeclaration() {
      List<Annotation> modifiers = new List<Annotation>();
      List<Identifier> identifiers = null;
      Token peek;
      while (IsSymbol(peek = PeekToken(), "@")) {
        modifiers.Add(ReadAnnotation());
      }
      ReadSymbol("package");
      identifiers = ReadIdentifierSequence();
      ReadSymbol(";");
      return new PackageDeclaration(modifiers.AsReadOnly(), identifiers.AsReadOnly());
    }

    private ImportDeclaration ReadImportDeclaration() {
      bool isStatic = false;
      bool isStar = false;
      List<Identifier> identifiers = new List<Identifier>();
      ReadSymbol("import");
      if (IsSymbol(PeekToken(), "static")) {
        isStatic = true;
        ReadSymbol("static");
      }
      // Read identifiers and separators
      bool readIdentifiers = true;
      while (readIdentifiers) {
        Token tok = ReadToken(); // Identifier or '*'
        if (IsSymbol(tok, "*")) {
          isStar = true;
        }
        else {
          identifiers.Add(VerifyIdentifier(tok));
        }
        // Now read separator
        tok = ReadToken(); // Separator
        if (isStar) {
          VerifySymbol(tok, ";");
          readIdentifiers = false;
        }
        else if (isStatic && !isStar && identifiers.Count == 1) {
          VerifySymbol(tok, ".");
        }
        else {
          if (IsSymbol(tok, ".")) {
            // Keep reading identifiers
          }
          else {
            VerifySymbol(tok, ";");
            readIdentifiers = false;
          }
        }
      }
      // Now determine which type of import statement we have
      if (isStatic) {
        if (isStar) {
          Name name = Name.GetNameFromIdentifiers(identifiers, NameType.Type);
          return new StaticImportOnDemandDeclaration(name);
        }
        else {
          List<Identifier> nameIdens = identifiers.GetRange(0, identifiers.Count - 1);
          Name name = Name.GetNameFromIdentifiers(nameIdens, NameType.Type);
          return new SingleStaticImportDeclaration(name, identifiers[identifiers.Count - 1]);
        }
      }
      else {
        if (isStar) {
          Name name = Name.GetNameFromIdentifiers(identifiers, NameType.PackageOrType);
          return new TypeImportOnDemandDeclaration(name);
        }
        else {
          Name name = Name.GetNameFromIdentifiers(identifiers, NameType.Type);
          return new SingleTypeImportDeclaration(name);
        }
      }
    }

    private TypeDeclaration ReadTypeDeclaration() {
      Token tok;
      Func<TypeDeclaration> func = null;
      LookAhead(() => {
        if (IsSymbol(PeekToken(), ";")) {
          func = () => {
            ReadSymbol(";");
            return new BlankTypeDeclaration();
          };
        }
        else {
          DiscardModifiers();
          tok = PeekToken();
          if (IsSymbol(tok, "class")) {
            func = ReadNormalClassDeclaration;
          }
          else if (IsSymbol(tok, "enum")) {
            func = ReadEnumDeclaration;
          }
          else if (IsSymbol(tok, "interface")) {
            func = ReadNormalInterfaceDeclaration;
          }
          else if (IsSymbol(tok, "@")) {
            func = ReadAnnotationTypeDeclaration;
          }
          else {
            throw new ParseException("illegal start of type declaration");
          }
        }
      });
      return func();
    }

    private void DiscardModifiers() {
      while (true) {
        bool moreModifiers = false;
        if ((PeekToken()) is Symbol sym) {
          switch (sym.Value) {
            case "abstract":
            case "default":
            case "final":
            case "native":
            case "private":
            case "protected":
            case "public":
            case "static":
            case "strictfp":
            case "synchronized":
            case "volatile":
              moreModifiers = true;
              break;
            case "@": {
              bool isAnnotation = true;
              LookAhead(() => {
                ReadSymbol("@");
                if (IsSymbol(PeekToken(), "interface")) {
                  isAnnotation = false;
                }
              });
              moreModifiers = isAnnotation;
              break;
            }
            default:
              break;
          }
        }
        if (!moreModifiers) { break; }
        ReadModifier();
      }
    }

    private Modifier ReadModifier() {
      if (ReadToken() is Symbol sym) {
        switch (sym.Value) {
          case "abstract":
            return new KeywordModifier(KeywordModifierType.Abstract);
          case "default":
            return new KeywordModifier(KeywordModifierType.Default);
          case "final":
            return new KeywordModifier(KeywordModifierType.Final);
          case "native":
            return new KeywordModifier(KeywordModifierType.Native);
          case "private":
            return new KeywordModifier(KeywordModifierType.Private);
          case "protected":
            return new KeywordModifier(KeywordModifierType.Protected);
          case "public":
            return new KeywordModifier(KeywordModifierType.Public);
          case "static":
            return new KeywordModifier(KeywordModifierType.Static);
          case "strictfp":
            return new KeywordModifier(KeywordModifierType.StrictFP);
          case "synchronized":
            return new KeywordModifier(KeywordModifierType.Synchronized);
          case "volatile":
            return new KeywordModifier(KeywordModifierType.Volatile);
          case "@":
            return new AnnotationModifier(ReadAnnotation());
        }
      }
      throw new ParseException("illegal modifier");
    }

    private ModuleDeclaration ReadModuleDeclaration() {
      List<Annotation> annotations = new List<Annotation>();
      bool isOpen = false;
      List<Identifier> identifiers = null;
      List<ModuleDirective> moduleDirectives = new List<ModuleDirective>();
      Token tok;
      // Begin by reading annotations
      while (IsSymbol(PeekToken(), "@")) {
        annotations.Add(ReadAnnotation());
      }
      // Read other header stuff
      tok = ReadToken();
      if (IsSymbol(tok, "open")) {
        isOpen = true;
        ReadSymbol("module");
      }
      else {
        VerifySymbol(tok, "module");
      }
      identifiers = ReadIdentifierSequence();
      ReadSymbol("{");
      // Now read module directives
      bool readingDirectives = true;
      while (readingDirectives) {
        tok = PeekToken();
        if (IsSpecificIdentifier(tok, "requires")) {
          moduleDirectives.Add(ReadRequiresDirective());
        }
        else if (IsSpecificIdentifier(tok, "exports")) {
          moduleDirectives.Add(ReadExportsDirectve());
        }
        else if (IsSpecificIdentifier(tok, "opens")) {
          moduleDirectives.Add(ReadOpensDirectve());
        }
        else if (IsSpecificIdentifier(tok, "uses")) {
          moduleDirectives.Add(ReadUsesDirective());
        }
        else if (IsSpecificIdentifier(tok, "provides")) {
          moduleDirectives.Add(ReadProvidesDirective());
        }
        else {
          readingDirectives = false;
        }
      }
      ReadSymbol("}");
      return new ModuleDeclaration(
        annotations.AsReadOnly(),
        isOpen,
        identifiers.AsReadOnly(),
        moduleDirectives.AsReadOnly()
      );
    }

    private RequiresDirective ReadRequiresDirective() {
      List<RequiresModifier> modifiers = new List<RequiresModifier>();
      Name moduleName;
      ReadSpecificIdentifier("requires");
      // Read modifiers
      bool readingMods = true;
      while (readingMods) {
        Action action = () => { }; // Do nothing by default
        LookAhead(() => {
          Token tok = ReadToken();
          if (IsSymbol(tok, "static")) {
            action = () => {
              modifiers.Add(new RequiresModifier(RequiresModifierType.Static));
              ReadToken();
            };
          }
          else if (IsSpecificIdentifier(tok, "transitive")) {
            Token tok2 = PeekToken();
            if (!IsSymbol(tok2, ".") && !IsSymbol(tok2, ";")) {
              action = () => {
                modifiers.Add(new RequiresModifier(RequiresModifierType.Transitive));
                ReadToken();
              };
            }
          }
          else {
            action = () => { readingMods = false; };
          }
        }); // end look ahead
        action();
      } // end while
      moduleName = Name.GetNameFromIdentifiers(ReadIdentifierSequence(), NameType.Module);
      ReadSymbol(";");
      return new RequiresDirective(modifiers.AsReadOnly(), moduleName);
    }

    private ExportsDirective ReadExportsDirectve() {
      Name packageName;
      List<Name> moduleNames = new List<Name>();
      Token tok;
      ReadSpecificIdentifier("exports");
      packageName = Name.GetNameFromIdentifiers(ReadIdentifierSequence(), NameType.Package);
      tok = PeekToken();
      if (IsSpecificIdentifier(tok, "to")) {
        bool readingNames = true;
        while (readingNames) {
          moduleNames.Add(Name.GetNameFromIdentifiers(ReadIdentifierSequence(), NameType.Module));
          if (IsSymbol(PeekToken(), ",")) {
            ReadSymbol(",");
          }
          else {
            readingNames = false;
          }
        }
      }
      ReadSymbol(";");
      return new ExportsDirective(packageName, moduleNames.AsReadOnly());
    }

    private OpensDirective ReadOpensDirectve() {
      Name packageName;
      List<Name> moduleNames = new List<Name>();
      Token tok;
      ReadSpecificIdentifier("opens");
      packageName = Name.GetNameFromIdentifiers(ReadIdentifierSequence(), NameType.Package);
      tok = PeekToken();
      if (IsSpecificIdentifier(tok, "to")) {
        bool readingNames = true;
        while (readingNames) {
          moduleNames.Add(Name.GetNameFromIdentifiers(ReadIdentifierSequence(), NameType.Module));
          if (IsSymbol(PeekToken(), ",")) {
            ReadSymbol(",");
          }
          else {
            readingNames = false;
          }
        }
      }
      ReadSymbol(";");
      return new OpensDirective(packageName, moduleNames.AsReadOnly());
    }

    private UsesDirective ReadUsesDirective() {
      Name typeName;
      ReadSpecificIdentifier("uses");
      typeName = Name.GetNameFromIdentifiers(ReadIdentifierSequence(), NameType.Type);
      ReadSymbol(";");
      return new UsesDirective(typeName);
    }

    private ProvidesDirective ReadProvidesDirective() {
      Name serviceName;
      List<Name> serviceProviderNames = new List<Name>();
      ReadSpecificIdentifier("provides");
      serviceName = Name.GetNameFromIdentifiers(ReadIdentifierSequence(), NameType.Type);
      ReadSpecificIdentifier("with");
      // Read service provider names
      bool readingNames = true;
      while (readingNames) {
        serviceProviderNames.Add(Name.GetNameFromIdentifiers(ReadIdentifierSequence(), NameType.Type));
        if (IsSymbol(PeekToken(), ",")) {
          ReadSymbol(",");
        }
        else {
          readingNames = false;
        }
      }
      ReadSymbol(";");
      return new ProvidesDirective(serviceName, serviceProviderNames.AsReadOnly());
    }

  }

}
