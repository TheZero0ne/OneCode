using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using OneCode;
using OneCode.DAL;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DAL {
    /// <summary>
    /// The DataAccessor is in the DataAcessLayer and is accessible from the whole project. It holds and provides data to transmit between the other layers.
    /// </summary>
    class DataAccessor {
        private bool workspaceWasSet = false;
        private static DataAccessor instance;
        private VisualStudioWorkspace workspace;
        private Microsoft.CodeAnalysis.Document doc;

        #region getters & setters

        public ICollectionView GroupedVariables {
            get {
                ListCollectionView groupedVariables = new ListCollectionView(varCollection);
                groupedVariables.GroupDescriptions.Add(new PropertyGroupDescription("DocumentName"));
                return groupedVariables;
            }
        }

        private MyCSharpSyntaxRewriter Rewriter { get; set; }

        public Microsoft.CodeAnalysis.Document ActualDocument {
            get { return doc; }
            set { doc = value; }
        }

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

        #endregion

        /// <summary>
        /// The Constructor is private because DataAccessor follows the pattern of a Singleton
        /// </summary>
        private DataAccessor() {
            varCollection = new VariableCollection();
        }

        /// <summary>
        /// This Method is the only point to get an instance of DataAccessor. Because the Construcor is private there will only be one Instance of this class at runtime at once.
        /// </summary>
        /// <returns>An instance of DataAccessor</returns>
        public static DataAccessor getInstance() {
            if (instance == null)
                instance = new DataAccessor();

            return instance;
        }

        /// <summary>
        /// Checks the syntax of all EnvDTE.Textdocuments in the list and searches for parameters, properties, fields and variables. The Names of these are translated for later use.
        /// </summary>
        /// <param name="haystackDocs">The List of EnvDTE.Textdocuments where to search in</param>
        /// <returns>the updated VariableCollection</returns>
        public async Task<VariableCollection> FindVariablesInDocs(List<EnvDTE.TextDocument> haystackDocs) {
            varCollection = new VariableCollection();

            foreach (EnvDTE.TextDocument haystackDoc in haystackDocs) {
                var objEditPt = haystackDoc.StartPoint.CreateEditPoint();
                var tree = CSharpSyntaxTree.ParseText(objEditPt.GetText(haystackDoc.EndPoint));
                var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                var compilation = CSharpCompilation.Create("MyCompilation", syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
                var model = compilation.GetSemanticModel(tree);
                Rewriter = new MyCSharpSyntaxRewriter(model, varCollection, MyRewriterModus.SEARCH, haystackDoc.Parent.Name);
                var result = Rewriter.Visit(tree.GetRoot());

                // Translate
                var dictionary = varCollection.GetNamesDictionaryForTranslation();
                Task<Dictionary<int, VariableNameInfo>> translationTask = Translator.TranslateDictionary(dictionary);
                Dictionary<int, VariableNameInfo> translationDic = await translationTask;

                // Apply translation to VariableCollection
                varCollection.ApplyTranslationDictionary(translationDic);
            }

            return this.varCollection;
        }

        /// <summary>
        /// Applies the translation of the VariableCollection to the TextDocuments.
        /// </summary>
        /// <param name="haystackDoc">The EnvDTE.TextDocument to apply the changes to</param>
        public void TryApplyChangesToWorkspace(EnvDTE.TextDocument haystackDoc) {
            var objEditPt = haystackDoc.StartPoint.CreateEditPoint();
            var tree = CSharpSyntaxTree.ParseText(objEditPt.GetText(haystackDoc.EndPoint));
            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation", syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            var model = compilation.GetSemanticModel(tree);

            // Write translations to the given EnvDTE.TextDocument
            Rewriter = new MyCSharpSyntaxRewriter(model, varCollection, MyRewriterModus.WRITE, haystackDoc.Parent.Name);
            var result = Rewriter.Visit(tree.GetRoot());

            // Get the Microsoft.Code.Analysis Document for the given EnvDTE Document to apply the changed SyntaxTree to the workspace
            string pathHaystackDoc = (Package.GetGlobalService(typeof(DTE)) as DTE).ActiveDocument.Name;
            Microsoft.CodeAnalysis.Document activeCodeDoc = workspace.CurrentSolution.Projects.First().Documents.Where(d => d.Name.ToLower().Equals(pathHaystackDoc.ToLower())).First();
            var solution = workspace.CurrentSolution.RemoveDocument(activeCodeDoc.Id);
            solution = solution.AddDocument(activeCodeDoc.Id, activeCodeDoc.Name, result);

            workspace.TryApplyChanges(solution);
        }
    }
}
