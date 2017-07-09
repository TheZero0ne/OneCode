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
                    List<TextDocument> docList = new List<TextDocument>();
                    
                    foreach(Document d in docs) {
                        docList.Add(ConvertFromComObjectToTextDocument(d));
                    }

                    await DataAcessor.getInstance().FindVariablesInDocs(docList);
                    FetchFromModels();

                    break;
                case SelectionType.Project:
                    Projects projects = (Package.GetGlobalService(typeof(DTE)) as DTE).Solution.Projects;

                    foreach(Project proj in projects) {
                        foreach(ProjectItem pi in proj.ProjectItems) {
                            foreach (var p in pi.Collection) {
                                TextDocument tDoc = ConvertFromComObjectToTextDocument(p);
                            }
                        }
                    }

                    break;
            }
        }

        private TextDocument ConvertFromComObjectToTextDocument(dynamic comObject) {
            return comObject.Object;
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
                        DataAcessor.getInstance().TryApplyChangesToWorkspace(activeDoc);
                        break;
                    case SelectionType.OpenDocuments:
                        var docs = (Package.GetGlobalService(typeof(DTE)) as DTE).Documents;

                        foreach (Document d in docs)
                        {
                           d.Activate();
                            TextDocument activeDoc2 = (Package.GetGlobalService(typeof(DTE)) as DTE).ActiveDocument.Object() as TextDocument;
                            DataAcessor.getInstance().TryApplyChangesToWorkspace(activeDoc2);
                        }
                        
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
