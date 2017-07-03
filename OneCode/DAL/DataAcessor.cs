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
        private Microsoft.CodeAnalysis.Document doc;

        public Microsoft.CodeAnalysis.Document ActualDocument { get { return doc; } set { doc = value; } }
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

        /// <summary>
        /// Checks the Syntax of an Document and adds all Locals, Fields, Parameters and Properties of the Document to the VariableCollection
        /// </summary>
        /// <param name="haystackDoc">A Document of Type EnvDTE</param>
        public void FindVariablesInDoc(EnvDTE.TextDocument haystackDoc) {
            varCollection = new VariableCollection();
            var objEditPt = haystackDoc.StartPoint.CreateEditPoint();
            var tree = CSharpSyntaxTree.ParseText(objEditPt.GetText(haystackDoc.EndPoint));

            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation", syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            var model = compilation.GetSemanticModel(tree);

            Rewriter = new MyRewriter(model, varCollection, MyRewriterModus.SEARCH);
            var result = Rewriter.Visit(tree.GetRoot());

            string pathHaystackDoc = haystackDoc.DTE.ActiveDocument.FullName;
            Microsoft.CodeAnalysis.Document activeCodeDoc = workspace.CurrentSolution.Projects.First().Documents.Where(d => d.FilePath == pathHaystackDoc).First();
            var solution = workspace.CurrentSolution.RemoveDocument(activeCodeDoc.Id);
            solution = solution.AddDocument(activeCodeDoc.Id, activeCodeDoc.Name, result);

            workspace.TryApplyChanges(solution);
        }
    }
}
