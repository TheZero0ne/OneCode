using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.LanguageServices;
using OneCode;
using System.Linq;

namespace DAL {
    class DataAcessor {
        private bool workspaceWasSet = false;
        private static DataAcessor instance;
        private VisualStudioWorkspace workspace;

        public VariableCollection varCollection { get; set; }

        public VisualStudioWorkspace Workspace {
            get { return workspace; }
            set {
                if (!workspaceWasSet) {
                    workspace = value;
                    workspaceWasSet = true;
                }
            }
        }

        private DataAcessor() {
            varCollection = new VariableCollection();
        }

        public static DataAcessor getInstance() {
            if (instance == null)
                instance = new DataAcessor();

            return instance;
        }

        public void FindVariablesInDoc(EnvDTE.TextDocument haystackDoc) {
            VariableCollection helperCollection = new VariableCollection();
            var objEditPt = haystackDoc.StartPoint.CreateEditPoint();
            var tree = CSharpSyntaxTree.ParseText(objEditPt.GetText(haystackDoc.EndPoint));
            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation", syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            var model = compilation.GetSemanticModel(tree);
            var variables = tree.GetRoot().DescendantNodes().Where(v => v is FieldDeclarationSyntax || v is LocalDeclarationStatementSyntax || v is PropertyDeclarationSyntax || v is ParameterSyntax);

            foreach (var v in variables) {
                var para = v as ParameterSyntax;
                var prop = v as PropertyDeclarationSyntax;
                var local = v as LocalDeclarationStatementSyntax;
                var field = v as FieldDeclarationSyntax;

                var symbol = model.GetDeclaredSymbol(v);
                string name = symbol.Name;
                string kind = symbol.Kind.ToString();

                if (para != null) {
                    string visibleType = para.Type.ToString();

                    varCollection.Add(new Variable(visibleType, name, kind, para.SpanStart));
                } else if (prop != null) {
                    //var symbol = model.GetDeclaredSymbol(prop);
                    string visibleType = prop.Type.ToString();
                    //string name = symbol.Name;
                    //string kind = symbol.Kind.ToString();

                    varCollection.Add(new Variable(visibleType, name, kind, prop.SpanStart));
                } else if (local != null) {
                    string visibleType = local.Declaration.Type.ToString();

                    foreach (var var in local.Declaration.Variables) {
                        symbol = model.GetDeclaredSymbol(var);
                        name = symbol.Name;
                        kind = symbol.Kind.ToString();

                        varCollection.Add(new Variable(visibleType, name, kind, local.SpanStart));
                    }
                } else if (field != null) {
                    string visibleType = field.Declaration.Type.ToString();

                    foreach (var var in field.Declaration.Variables) {
                        symbol = model.GetDeclaredSymbol(var);
                        name = symbol.Name;
                        kind = symbol.Kind.ToString();

                        varCollection.Add(new Variable(visibleType, name, kind, field.SpanStart));
                    }
                }
            }
        }
    }
}
