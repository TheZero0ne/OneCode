using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CSharp;
using OneCode;
using System;
using System.Linq;

namespace DAL {
    class DataAcessor {
        private static DataAcessor instance;
        public VariableCollection varCollection { get; set; }

        private DataAcessor() {
            varCollection = new VariableCollection();
        }

        public static DataAcessor getInstance() {
            if (instance == null)
                instance = new DataAcessor();

            return instance;
        }

        public void FindVariablesInDoc(EnvDTE.TextDocument haystackDoc) {
            EnvDTE.EditPoint objEditPt = haystackDoc.StartPoint.CreateEditPoint();

            var tree = CSharpSyntaxTree.ParseText(objEditPt.GetText(haystackDoc.EndPoint));
            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation", syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            var model = compilation.GetSemanticModel(tree);

            var vars = tree.GetRoot().DescendantNodes().Where(v => v is VariableDeclarationSyntax || v is ParameterSyntax || v is PropertyDeclarationSyntax);

            var property = tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>();
            var parameter = tree.GetRoot().DescendantNodes().OfType<ParameterSyntax>();
            var field = tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>();

            //Get symbols
            foreach (var p in property) {
                var propertySymbol = model.GetDeclaredSymbol(p);
            }

            foreach (var p in parameter) {
                var methodSymbol = model.GetDeclaredSymbol(p);
            }

            foreach (var f in field) {
                foreach (var var in f.Declaration.Variables) {
                    var fieldSymbol = model.GetDeclaredSymbol(var);

                    string name = fieldSymbol.Name;
                    Type type = Type.GetType(fieldSymbol.ContainingType.Name.ToString());
                    string visibleType = f.Declaration.Type.ChildTokens().Where(v => v.IsKeyword()).First().Value.ToString();
                    string text = fieldSymbol.OriginalDefinition.ToString();

                    varCollection.Add(new Variable(name, text, haystackDoc, type));
                }
            }

            Translator.TranslateStringArray(new string[] { "Hello", "my", "name", "is", "Tim" });
        }
    }
}
