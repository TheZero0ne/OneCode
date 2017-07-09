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
                    EnvDTE.TextDocument activeDoc = (Package.GetGlobalService(typeof(DTE)) as DTE).ActiveDocument.Object() as EnvDTE.TextDocument;
                    List<EnvDTE.TextDocument> list = new List<EnvDTE.TextDocument>();
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
                    List<EnvDTE.TextDocument> edoclist = new List<EnvDTE.TextDocument>();
                    List<ProjectItem> pis = new List<ProjectItem>();

                    var ids = workspace.CurrentSolution.ProjectIds.GetEnumerator();

                    while (ids.MoveNext()) {
                        var roslynDocs = workspace.CurrentSolution.GetProject(ids.Current).Documents;

                        foreach (Microsoft.CodeAnalysis.Document doc in roslynDocs) {
                            pis.Add((Package.GetGlobalService(typeof(DTE)) as DTE).Solution.FindProjectItem(doc.FilePath));
                        }

                        foreach (ProjectItem pi in pis) {
                            try {
                                if (pi.IsOpen) {
                                    edoclist.Add(pi.Document.Object() as EnvDTE.TextDocument);
                                } else {
                                    pi.Open();
                                    edoclist.Add(pi.Document.Object() as EnvDTE.TextDocument);
                                }
                            } catch {

                            }
                        }
                    }

                    if (edoclist.Count == 0) {
                        MessageBox.Show("Es wurde keine Solution gefunden. Lädt das Projekt noch?");
                    } else {
                        await DataAcessor.getInstance().FindVariablesInDocs(edoclist);
                        FetchFromModels();
                    }

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
                        EnvDTE.TextDocument activeDoc = (Package.GetGlobalService(typeof(DTE)) as DTE).ActiveDocument.Object() as EnvDTE.TextDocument;
                        List<EnvDTE.TextDocument> singleList = new List<EnvDTE.TextDocument>();
                        singleList.Add(activeDoc);
                        DataAcessor.getInstance().TryApplyChangesToWorkspace(singleList);
                        break;
                    case SelectionType.OpenDocuments:
                        var docs = (Package.GetGlobalService(typeof(DTE)) as DTE).Documents;
                        List<EnvDTE.TextDocument> docList = new List<EnvDTE.TextDocument>();

                        foreach (EnvDTE.Document d in docs)
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
