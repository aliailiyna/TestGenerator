using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using TestGeneratorLibrary;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMethodOneClass()
        {
            TestGenerator testGenerator = new TestGenerator();
            string code = new StreamReader("../../../../TestLibrary/Class1.cs").ReadToEnd();

            List<ResultOfTestGeneration> listResult = testGenerator.GenerateTests(code);
            Assert.AreEqual(1, listResult.Count);
        }

        [TestMethod]
        public void TestMethodTwoClass()
        {
            TestGenerator testGenerator = new TestGenerator();
            string code = new StreamReader("../../../../TestLibrary/Class4.cs").ReadToEnd();

            List<ResultOfTestGeneration> listResult = testGenerator.GenerateTests(code);
            Assert.AreEqual(2, listResult.Count);
        }

        [TestMethod]
        public void TestMethodThreeClass()
        {
            TestGenerator testGenerator = new TestGenerator();
            string code = new StreamReader("../../../../TestLibrary/Class5.cs").ReadToEnd();

            List<ResultOfTestGeneration> listResult = testGenerator.GenerateTests(code);
            Assert.AreEqual(3, listResult.Count);
        }

        [TestMethod]
        public void TestMethodNoClass()
        {
            TestGenerator testGenerator = new TestGenerator();
            string code = new StreamReader("../../../../TestLibrary/ClassEmpty.cs").ReadToEnd();

            List<ResultOfTestGeneration> listResult = testGenerator.GenerateTests(code);
            Assert.AreEqual(1, listResult.Count);
        }

        [TestMethod]
        public void TestMethodUsing()
        {
            TestGenerator testGenerator = new TestGenerator();
            string code = new StreamReader("../../../../TestLibrary/Class2.cs").ReadToEnd();

            List<ResultOfTestGeneration> listResult = testGenerator.GenerateTests(code);
            ResultOfTestGeneration classResult = listResult[0];
            CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(classResult.Code).GetCompilationUnitRoot();

            List<UsingDirectiveSyntax> usings = GetUsings(root.DescendantNodes());
            List<string> usingNames = GetUsingNames(usings);

            Assert.IsTrue(usingNames.Contains("Microsoft.VisualStudio.TestTools.UnitTesting"));
        }

        private List<UsingDirectiveSyntax> GetUsings(IEnumerable<SyntaxNode> members)
        {
            return members.OfType<UsingDirectiveSyntax>().ToList();
        }

        private List<string> GetUsingNames(List<UsingDirectiveSyntax> usings)
        {
            return usings.Select(usingDeclaration => usingDeclaration.Name.ToString()).ToList();
        }

        [TestMethod]
        public void TestMethodAssertAndAttributes()
        {
            TestGenerator testGenerator = new TestGenerator();
            string code = new StreamReader("../../../../TestLibrary/Class3.cs").ReadToEnd();

            List<ResultOfTestGeneration> listResult = testGenerator.GenerateTests(code);
            ResultOfTestGeneration classResult = listResult[0];
            CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(classResult.Code).GetCompilationUnitRoot();

            List<MethodDeclarationSyntax> publicMethods = GetPublicMethods(root.DescendantNodes());
            Assert.AreEqual(3, publicMethods.Count);
            List<string> methodNames = GetMethodNames(publicMethods);

            foreach (MethodDeclarationSyntax method in publicMethods)
            {
                List<string> identifireNames = method.DescendantNodes().OfType<IdentifierNameSyntax>().
                    Select(identifire => identifire.Identifier.ToString()).ToList();
                Assert.IsTrue(identifireNames.Contains("Assert"));
                List<string> methodAttributes = method.DescendantNodes().OfType<AttributeSyntax>().
                    Select(attribute => attribute.Name.ToString()).ToList();
                Assert.IsTrue(methodAttributes.Contains("TestMethod"));
            }
        }

        [TestMethod]
        public void TestMethodClass2Methods()
        {
            TestGenerator testGenerator = new TestGenerator();
            string code = new StreamReader("../../../../TestLibrary/Class2.cs").ReadToEnd();

            List<ResultOfTestGeneration> listResult = testGenerator.GenerateTests(code);
            ResultOfTestGeneration classResult = listResult[0];
            CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(classResult.Code).GetCompilationUnitRoot();

            List<MethodDeclarationSyntax> publicMethods = GetPublicMethods(root.DescendantNodes());
            Assert.AreEqual(2, publicMethods.Count);
            List<string> methodNames = GetMethodNames(publicMethods);

            Assert.IsTrue(methodNames.Contains("FirstMethodTest"));
            Assert.IsTrue(methodNames.Contains("SecondMethodTest"));
        }

        private List<MethodDeclarationSyntax> GetPublicMethods(IEnumerable<SyntaxNode> members)
        {
            return members.OfType<MethodDeclarationSyntax>()
                .Where(methodDeclaration => methodDeclaration.Modifiers.Select(modifire => 
                modifire.IsKind(SyntaxKind.PublicKeyword)).Any()).ToList();
        }

        private List<string> GetMethodNames(List<MethodDeclarationSyntax> methods)
        {
            return methods.Select(method => method.Identifier.ToString()).ToList();
        }

        [TestMethod]
        public void TestMethodClass2NameAttributeFile()
        {
            TestGenerator testGenerator = new TestGenerator();
            string code = new StreamReader("../../../../TestLibrary/Class2.cs").ReadToEnd();

            List<ResultOfTestGeneration> listResult = testGenerator.GenerateTests(code);

            var syntaxTree = CSharpSyntaxTree.ParseText(listResult[0].Code);
            var classes = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
            Assert.AreEqual(classes.Count, 1);

            foreach (var classDeclaration in classes)
            {
                Assert.AreEqual("Class2UnitTest", classDeclaration.Identifier.Text);
                List<string> methodAttributes = classDeclaration.DescendantNodes().OfType<AttributeSyntax>().
                    Select(attribute => attribute.Name.ToString()).ToList();
                Assert.IsTrue(methodAttributes.Contains("TestMethod"));
            }

            Assert.AreEqual(listResult[0].Name, "Class2UnitTest");
        }
    }
}
