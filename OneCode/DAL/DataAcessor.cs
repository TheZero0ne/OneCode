using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using OneCode;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        /// <summary>
        /// Checks the Syntax of an Document and adds all Locals, Fields, Parameters and Properties of the Document to the VariableCollection
        /// </summary>
        /// <param name="haystackDoc">A Document of Type EnvDTE</param>
        public void FindVariablesInDoc(EnvDTE.TextDocument haystackDoc) {
            var objEditPt = haystackDoc.StartPoint.CreateEditPoint();
            var tree = CSharpSyntaxTree.ParseText(objEditPt.GetText(haystackDoc.EndPoint) + "//Test");
            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation", syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            var model = compilation.GetSemanticModel(tree);
            var variables = tree.GetRoot().DescendantNodes().Where(v => v is FieldDeclarationSyntax || v is LocalDeclarationStatementSyntax || v is PropertyDeclarationSyntax || v is ParameterSyntax);

            string pathHaystackDoc = haystackDoc.DTE.ActiveDocument.FullName;
            Microsoft.CodeAnalysis.Document activeCodeDoc = workspace.CurrentSolution.Projects.First().Documents.Where(d => d.FilePath == pathHaystackDoc).First();
            var solution = workspace.CurrentSolution.RemoveDocument(activeCodeDoc.Id);
            solution = solution.AddDocument(activeCodeDoc.Id, activeCodeDoc.Name, tree.GetRoot());

            workspace.TryApplyChanges(solution);
        }
    }
}
