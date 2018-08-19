using System;
using System.Collections.Generic;

namespace JavaVerifier.Parsing.SyntaxElements {

  internal sealed class Name {

    public Identifier Identifier { get; }
    public Name Parent { get; }
    public NameType NameType { get; set; } // May be changed later if ambiguous

    public Name(Identifier identifier, Name parent, NameType nameType) {
      Identifier = identifier;
      Parent = parent;
      NameType = nameType;
    }

    public static Name GetNameFromIdentifiers(List<Identifier> identifiers, NameType nameType) {
      return GetNameFromIdentifiersHelper(identifiers, nameType, identifiers.Count);
    }

    private static Name GetNameFromIdentifiersHelper(List<Identifier> identifiers, NameType nameType, int count) {
      if (count <= 0) {
        throw new ArgumentOutOfRangeException();
      }
      if (count == 1) {
        return new Name(identifiers[0], null, nameType);
      }
      else {
        NameType newNameType;
        switch (nameType) {
          case NameType.Module:
          case NameType.Package:
          case NameType.PackageOrType:
          case NameType.Ambiguous:
            newNameType = nameType;
            break;
          case NameType.Type:
            newNameType = NameType.PackageOrType;
            break;
          case NameType.Expression:
            newNameType = NameType.Ambiguous;
            break;
          default: // NameType.Method
            throw new InvalidOperationException();
        }
        Name parent = GetNameFromIdentifiersHelper(identifiers, newNameType, count - 1);
        return new Name(identifiers[count - 1], parent, nameType);
      }
    }

  }

}