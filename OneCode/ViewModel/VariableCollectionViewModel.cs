using DAL;
using EnvDTE;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Microsoft.VisualStudio.Shell;
using System.Windows.Data;
using OneCode.View;
using System;
using System.Windows;
using EnvDTE80;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace OneCode {
    class VariableCollectionViewModel : ObservableCollection<VariableViewModel> {
        private RelayCommand findVariablesInDoc;
        private RelayCommand applyChangesToDoc;
        private bool syncDisabled = false;
        private SelectionType currentSelectionType;
        private bool includeMethodNamesEnabled = false;
        private bool includeMethodNamesChecked = false;

        // Current Document as default SelectionType
        public SelectionType SelectionType {

            get{return currentSelectionType;}
            set
            {
                this.currentSelectionType = value;
                this.IncludeMethodNamesEnabled = currentSelectionType == SelectionType.CurrentDocument;
            }
        }

        public bool IncludeMethodNamesEnabled {

            get {return includeMethodNamesEnabled;}
            set
            {
                this.includeMethodNamesEnabled = value;
                // if disabled, uncheck the CheckBox
                if (!includeMethodNamesEnabled)
                {
                    this.IncludeMethodNamesChecked = false; 
                }
            }
        }
        public bool IncludeMethodNamesChecked {

            get{return this.includeMethodNamesChecked;}
            set
            {
                this.includeMethodNamesChecked = value;
            }
        }

        public VariableCollectionViewModel() {
            findVariablesInDoc = new RelayCommand(this.FindVariablesInDoc, this.SyncDisabled);
            applyChangesToDoc = new RelayCommand(this.ApplyChangesToDoc, this.SyncDisabled);

            this.CollectionChanged += ViewModelCollectionChanged;
            DataAcessor.getInstance().varCollection.CollectionChanged += ModelCollectionChanged;
            this.SelectionType = SelectionType.CurrentDocument;
        }

        private void FetchFromModels() {
            // While this boolean is true, no other sync-methods will be executed
            this.syncDisabled = true;
            this.Clear();

            foreach (Variable v in DataAcessor.getInstance().varCollection)
                this.Add(new VariableViewModel(v));

            this.syncDisabled = false;
        }

        private void WriteToModels()
        {
            // While this boolean is true, no other sync-methods will be executed
            this.syncDisabled = true;

            var newCol = new VariableCollection();
            foreach (VariableViewModel vvm in this)
            {
                newCol.Add(vvm.Var);
            }
            DataAcessor.getInstance().varCollection = newCol;

            this.syncDisabled = false;
        }

        public void ViewModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (!syncDisabled) {
                syncDisabled = true;
                switch (e.Action) {
                    case NotifyCollectionChangedAction.Add:
                        foreach (Variable v in e.NewItems.OfType<VariableViewModel>().Select(x => x.Var).OfType<Variable>())
                            DataAcessor.getInstance().varCollection.Add(v);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (Variable v in e.OldItems.OfType<VariableViewModel>().Select(x => x.Var).OfType<Variable>())
                            DataAcessor.getInstance().varCollection.Remove(v);
                        break;
                }
                syncDisabled = false;
            }
        }

        public void ModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (!syncDisabled) {
                FetchFromModels();
            }
        }

        private bool SyncDisabled() {
            return !syncDisabled;
        }

        #region Commands

        public ICommand findVariablesInDocClick { get { return findVariablesInDoc; } }

        public ICommand applyChangesToDocClick { get { return applyChangesToDoc; } }

        public async void FindVariablesInDoc() {
            bool includeMethodNames = IncludeMethodNamesEnabled && IncludeMethodNamesChecked;

            switch (SelectionType) {
                case SelectionType.CurrentDocument:
                    TextDocument activeDoc = (Package.GetGlobalService(typeof(DTE)) as DTE).ActiveDocument.Object() as TextDocument;
                    List<TextDocument> list = new List<TextDocument>();
                    list.Add(activeDoc);
                    await DataAcessor.getInstance().FindVariablesInDocs(list);
                    FetchFromModels();
                    break;
                case SelectionType.OpenDocuments:
                    var docs = (Package.GetGlobalService(typeof(DTE)) as DTE).Documents;
                    List<EnvDTE.TextDocument> docList = new List<EnvDTE.TextDocument>();
                    
                    foreach(EnvDTE.Document d in docs) {
                        docList.Add(ConvertFromComObjectToTextDocument(d));
                    }

                    await DataAcessor.getInstance().FindVariablesInDocs(docList);
                    FetchFromModels();

                    break;
                case SelectionType.Project:
                    Workspace workspace = DataAcessor.getInstance().Workspace;

                    var enumer = (Package.GetGlobalService(typeof(DTE)) as DTE).Solution.Projects.GetEnumerator();

                    while (enumer.MoveNext()) {
                        var test = ((EnvDTE.Project)enumer.Current).ProjectItems;

                        foreach(ProjectItem pi in test) {
                            try {
                                var test2 = pi;
                                var test1 = pi.Object;
                                var test3 = pi.FileCodeModel;

                                if (pi.Document != null) {
                                    var test4 = pi.Document.ProjectItem;
                                }
                                EnvDTE.TextDocument tDoc0 = pi.Document.ProjectItem as EnvDTE.TextDocument;
                                EnvDTE.TextDocument tDoc01 = ConvertFromComObjectToTextDocument(pi.Document.ProjectItem as EnvDTE.TextDocument);

                                EnvDTE.TextDocument tDoc = pi.Object as EnvDTE.TextDocument;
                                EnvDTE.TextDocument tDoc2 = ConvertFromComObjectToTextDocument(pi.Object);

                                EnvDTE.TextDocument tDoc3 = pi as EnvDTE.TextDocument;
                                if (tDoc3 != null) {
                                    await DataAcessor.getInstance().FindVariablesInDocs(new List<EnvDTE.TextDocument>() { tDoc3 });
                                }

                                EnvDTE.TextDocument tDoc4 = ConvertFromComObjectToTextDocument(pi);
                                EnvDTE.TextDocument tDoc5 = pi.FileCodeModel as EnvDTE.TextDocument;
                                EnvDTE.TextDocument tDoc6 = ConvertFromComObjectToTextDocument(pi.FileCodeModel);
                            } catch {

                            }
                        }
                    }

                    /*foreach(Microsoft.CodeAnalysis.Project p in workspace.CurrentSolution.Projects) {
                        foreach(Microsoft.CodeAnalysis.Document d in p.Documents) {
                            EnvDTE.Solution sol = (Package.GetGlobalService(typeof(DTE)) as DTE).Solution.Projects.;
                            EnvDTE.TextDocument tDoc = sol.FindProjectItem(d.FilePath) as EnvDTE.TextDocument;
                        }
                    }*/

                    break;
            }
        }

        private EnvDTE.TextDocument ConvertFromComObjectToTextDocument(dynamic comObject) {
            try {
                return comObject.Object;
            } catch {
                return null;
            }
            
        }

        public void ApplyChangesToDoc()
        {
            if (this.Count == 0)
            {
                MessageBox.Show("Keine Variablen gefunden, bitte suchen Sie erst nach Variablen.", "Fehler bei Aufruf");
                return;
            }
            try {
                WriteToModels();

                
                switch (SelectionType) {
                    case SelectionType.CurrentDocument:
                        TextDocument activeDoc = (Package.GetGlobalService(typeof(DTE)) as DTE).ActiveDocument.Object() as TextDocument;
                        List<TextDocument> singleList = new List<TextDocument>();
                        singleList.Add(activeDoc);
                        DataAcessor.getInstance().TryApplyChangesToWorkspace(singleList);
                        break;
                    case SelectionType.OpenDocuments:
                        var docs = (Package.GetGlobalService(typeof(DTE)) as DTE).Documents;
                        List<TextDocument> docList = new List<TextDocument>();

                        foreach (Document d in docs)
                        {
                           docList.Add(ConvertFromComObjectToTextDocument(d));
                        }
                        DataAcessor.getInstance().TryApplyChangesToWorkspace(docList);
                        break;
                    case SelectionType.Project:

                        break;
                }
                

                // When applied successfull clear list
                this.Clear();
            } catch (Exception e) {
                    MessageBox.Show("Bei der Anwendung ist ein Fehler aufgetreten.\n" + e.Message, "Interner Fehler");
            } finally {

            }

        }

        #endregion

        public ListCollectionView CollectionView {
            get {
                ListCollectionView vars = new ListCollectionView(this);
                vars.GroupDescriptions.Add(new PropertyGroupDescription("Variablen"));

                return vars;
            }
        }

        public ListCollectionView collectionView() {
            ListCollectionView vars = new ListCollectionView(this);
            vars.GroupDescriptions.Add(new PropertyGroupDescription("Variablen"));

            return vars;
        }
    }
}
