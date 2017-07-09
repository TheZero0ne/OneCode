using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using OneCode;
using OneCode.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DAL {
    class DataAcessor {

        private bool workspaceWasSet = false;
        private static DataAcessor instance;
        private VisualStudioWorkspace workspace;
        private Microsoft.CodeAnalysis.Document doc;
        private MyCSharpSyntaxRewriter Rewriter { get; set; }

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

        public ListCollectionView collectionView() {
            ListCollectionView vars = new ListCollectionView(varCollection);
            vars.GroupDescriptions.Add(new PropertyGroupDescription("Variablen"));

            return vars;
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
        public async Task<VariableCollection> FindVariablesInDoc(EnvDTE.TextDocument haystackDoc) {
            varCollection = new VariableCollection();
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

            varCollection.ApplyTranslationDictionary(translationDic);

            return this.varCollection;
        }

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

                varCollection.ApplyTranslationDictionary(translationDic);
            }

            return this.varCollection;
        }

        public void TryApplyChangesToWorkspace(EnvDTE.TextDocument haystackDoc)
        {
            var objEditPt = haystackDoc.StartPoint.CreateEditPoint();
            var tree = CSharpSyntaxTree.ParseText(objEditPt.GetText(haystackDoc.EndPoint));

            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation", syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            var model = compilation.GetSemanticModel(tree);

            // Write
            Rewriter = new MyCSharpSyntaxRewriter(model, varCollection, MyRewriterModus.WRITE, haystackDoc.Parent.Name);
            var result = Rewriter.Visit(tree.GetRoot());


            string pathHaystackDoc = (Package.GetGlobalService(typeof(DTE)) as DTE).ActiveDocument.Name;
            Microsoft.CodeAnalysis.Document activeCodeDoc = workspace.CurrentSolution.Projects.First().Documents.Where(d => d.Name.ToLower().Equals(pathHaystackDoc.ToLower())).First();
            var solution = workspace.CurrentSolution.RemoveDocument(activeCodeDoc.Id);
            solution = solution.AddDocument(activeCodeDoc.Id, activeCodeDoc.Name, result);

            workspace.TryApplyChanges(solution);
        }

    }

    /*
     * 
     *  TODOs 
     *  -   Anzeige der Variablen nach Dokument sortiert
     *     
     *  -   SelectionType auswerten -> Einschränkung für die Dokumentenauswahl
     *     - Wie Save ich automatisch / programmatisch den ganzen Workspace?
     *     - Kann der Benutzer auswählen, ob direkt gespeichert oder nur geändert wird?!
     *  
     *  -   Dokumentation ^-^
     * 
     * 
     */
}
