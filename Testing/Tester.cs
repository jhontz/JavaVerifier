using System;
using System.IO;
using JavaVerifier;
using JavaVerifier.Parsing;
using JavaVerifier.Parsing.SyntaxElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Text;

using Type = JavaVerifier.Parsing.SyntaxElements.Type;

namespace Testing
{

  internal class Tester
  {

    static void Main(string[] args)
    {
      string path = "../../test_files/test.txt";
      using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
        StreamReader sr = new StreamReader(fs);

        Parser parser = new Parser(sr);
        PrivateObject prv = new PrivateObject(parser);
        Type type = (Type)(prv.Invoke("ReadType", new object[0]));

        CompilationUnit unit = parser.ReadCompilationUnit();
      }

      Console.WriteLine("Done.");
      Console.ReadLine();
    }

  }
}
