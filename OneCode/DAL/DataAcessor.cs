using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using OneCode;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace DAL {
    class DataAcessor {
        private bool workspaceWasSet = false;
        private static DataAcessor instance;
        private VisualStudioWorkspace workspace;

        public VariableCollection varCollection { get; set; }

        /// <summary>
        /// 
        /// </summary>
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

        private MyRewriter Rewriter { get; set; }

        public enum MyRewriterModus
        {
            SEARCH,
            WRITE
        }

        public class MyRewriter : CSharpSyntaxRewriter {

            private readonly SemanticModel MySemanticModel;
            private VariableCollection varCol;
            private MyRewriterModus modus;

            public MyRewriter(SemanticModel sm, VariableCollection col, MyRewriterModus modus)
            {
                this.MySemanticModel = sm;
                this.varCol = col;
                this.modus = modus;
            }

            public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
            {
                if (modus == MyRewriterModus.SEARCH)
                {
                    TypeInfo initializerInfo = MySemanticModel.GetTypeInfo(node.Initializer.Value);
                    varCol.Add(new Variable(initializerInfo.Type.Name.ToString(), node.Identifier.Text, node.Kind().ToString(), node.SpanStart));

                    return base.VisitVariableDeclarator(node);
                } else
                {
                    var newVariableName = "_" + node.Identifier.Text;
                    var newIdentifier = SyntaxFactory.Identifier(newVariableName);

                    return node.WithIdentifier(newIdentifier);
                }

            }

            public override SyntaxNode VisitParameter(ParameterSyntax node)
            {
                varCol.Add(new Variable(node.Type.ToString(), node.Identifier.Text, node.Kind().ToString(), node.SpanStart));
                return base.VisitParameter(node);
            }

        }

        public void FindVariablesInDoc(EnvDTE.TextDocument haystackDoc) {
            varCollection = new VariableCollection();
            var objEditPt = haystackDoc.StartPoint.CreateEditPoint();
            var tree = CSharpSyntaxTree.ParseText(objEditPt.GetText(haystackDoc.EndPoint));

            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation", syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            var model = compilation.GetSemanticModel(tree);




            Rewriter = new MyRewriter(model, varCollection, MyRewriterModus.SEARCH);
            var result = Rewriter.Visit(tree.GetRoot());

            var solution = workspace.CurrentSolution;

            var project = solution.GetProject(solution.ProjectIds.First());
           

            var filePath = (Package.GetGlobalService(typeof(DTE)) as DTE).ActiveDocument.FullName;

            // Save to file
            //File.WriteAllText(filePath, result.ToFullString());

            /*
            var objEditPt = haystackDoc.StartPoint.CreateEditPoint();
            var tree = CSharpSyntaxTree.ParseText(objEditPt.GetText(haystackDoc.EndPoint));
            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation", syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            var model = compilation.GetSemanticModel(tree);
            var variables = tree.GetRoot().DescendantNodes().Where(v => v is FieldDeclarationSyntax || v is LocalDeclarationStatementSyntax || v is PropertyDeclarationSyntax || v is ParameterSyntax);
            Microsoft.CodeAnalysis.TextDocument activeDoc = (Package.GetGlobalService(typeof(DTE)) as DTE).ActiveDocument.Object() as Microsoft.CodeAnalysis.TextDocument;

            foreach (var v in variables)
            {
                var para = v as ParameterSyntax;
                var prop = v as PropertyDeclarationSyntax;
                var local = v as LocalDeclarationStatementSyntax;
                var field = v as FieldDeclarationSyntax;

                var symbol = model.GetDeclaredSymbol(v);
                string name = symbol.Name;
                string kind = symbol.Kind.ToString();

                var originalSolution = workspace.CurrentSolution;
                var project = originalSolution.GetProject(originalSolution.ProjectIds.First());


                if (para != null)
                {
                    string visibleType = para.Type.ToString();

                    varCollection.Add(new Variable(visibleType, name, kind, para.SpanStart));
                }
                else if (prop != null)
                {
                    string visibleType = prop.Type.ToString();

                    varCollection.Add(new Variable(visibleType, name, kind, prop.SpanStart));
                }
                else if (local != null)
                {
                    string visibleType = local.Declaration.Type.ToString();

                    foreach (var var in local.Declaration.Variables)
                    {
                        symbol = model.GetDeclaredSymbol(var);
                        name = symbol.Name;
                        kind = symbol.Kind.ToString();

                        varCollection.Add(new Variable(visibleType, name, kind, local.SpanStart));
                    }
                }
                else if (field != null)
                {
                    string visibleType = field.Declaration.Type.ToString();

                    foreach (var var in field.Declaration.Variables)
                    {
                        symbol = model.GetDeclaredSymbol(var);
                        name = symbol.Name;
                        kind = symbol.Kind.ToString();

                        varCollection.Add(new Variable(visibleType, name, kind, field.SpanStart));
                    }
                }
                */
            }
            
    }
}
