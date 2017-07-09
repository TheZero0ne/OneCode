using DAL;
using EnvDTE;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Microsoft.VisualStudio.Shell;
using System;
using System.Windows;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.ComponentModel;
using System.Windows.Data;

namespace OneCode {
    /// <summary>
    /// The ViewModel of the VariableCollection
    /// </summary>
    class VariableCollectionViewModel : ObservableCollection<VariableViewModel> {
        private RelayCommand findVariablesInDoc;
        private RelayCommand applyChangesToDoc;
        private bool syncDisabled = false;
        private SelectionType currentSelectionType;
        private bool includeMethodNamesEnabled = false;
        private bool includeMethodNamesChecked = false;

        #region getters & setters

        public ICollectionView GroupedVariables {
            get {
                ListCollectionView groupedVariables = new ListCollectionView(this);
                groupedVariables.GroupDescriptions.Add(new PropertyGroupDescription("DocumentName"));
                return groupedVariables;
            }
        }

        // Current Document as default SelectionType
        public SelectionType SelectionType {
            get { return currentSelectionType; }
            set {
                this.currentSelectionType = value;
                this.IncludeMethodNamesEnabled = currentSelectionType == SelectionType.CurrentDocument;
            }
        }

        public bool IncludeMethodNamesEnabled {
            get { return includeMethodNamesEnabled; }
            set {
                this.includeMethodNamesEnabled = value;
                // if disabled, uncheck the CheckBox
                if (!includeMethodNamesEnabled) {
                    this.IncludeMethodNamesChecked = false; 
                }
            }
        }

        public bool IncludeMethodNamesChecked {
            get { return this.includeMethodNamesChecked; }
            set {
                this.includeMethodNamesChecked = value;
            }
        }

        #endregion

        public VariableCollectionViewModel() {
            findVariablesInDoc = new RelayCommand(this.FindVariablesInDoc, this.SyncDisabled);
            applyChangesToDoc = new RelayCommand(this.ApplyChangesToDoc, this.SyncDisabled);

            this.CollectionChanged += ViewModelCollectionChanged;
            DataAccessor.getInstance().varCollection.CollectionChanged += ModelCollectionChanged;
            this.SelectionType = SelectionType.CurrentDocument;
        }

        /// <summary>
        /// Fetches the data from DataAccessor
        /// </summary>
        private void FetchFromModels() {
            // While this boolean is true, no other sync-methods will be executed
            this.syncDisabled = true;
            this.Clear();

            foreach (Variable v in DataAccessor.getInstance().varCollection)
                this.Add(new VariableViewModel(v));

            this.syncDisabled = false;
        }

        /// <summary>
        /// Updates the Collection held by DataAccessor
        /// </summary>
        private void WriteToModels() {
            // While this boolean is true, no other sync-methods will be executed
            this.syncDisabled = true;

            var newCol = new VariableCollection();
            foreach (VariableViewModel vvm in this) {
                newCol.Add(vvm.Var);
            }
            DataAccessor.getInstance().varCollection = newCol;

            this.syncDisabled = false;
        }

        private bool SyncDisabled() {
            return !syncDisabled;
        }

        #region Events

        /// <summary>
        /// Update the DataAccessor when there are changes at the ViewModel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ViewModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (!syncDisabled) {
                syncDisabled = true;
                switch (e.Action) {
                    case NotifyCollectionChangedAction.Add:
                        foreach (Variable v in e.NewItems.OfType<VariableViewModel>().Select(x => x.Var).OfType<Variable>())
                            DataAccessor.getInstance().varCollection.Add(v);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (Variable v in e.OldItems.OfType<VariableViewModel>().Select(x => x.Var).OfType<Variable>())
                            DataAccessor.getInstance().varCollection.Remove(v);
                        break;
                }
                syncDisabled = false;
            }
        }

        /// <summary>
        /// Updates the ViewModel if the collection held by DataAcessor was changed
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Additional arguments</param>
        public void ModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (!syncDisabled) {
                FetchFromModels();
            }
        }

        #endregion

        #region Commands

        public ICommand findVariablesInDocClick { get { return findVariablesInDoc; } }

        public ICommand applyChangesToDocClick { get { return applyChangesToDoc; } }

        /// <summary>
        /// Finds all Variables in the project and creates a new VariableCollection which holds the found data. The Depth is defined in the settings
        /// </summary>
        public async void FindVariablesInDoc() {
            bool includeMethodNames = IncludeMethodNamesEnabled && IncludeMethodNamesChecked;

            switch (SelectionType) {
                case SelectionType.CurrentDocument:
                    try {
                        EnvDTE.TextDocument activeDoc = (Package.GetGlobalService(typeof(DTE)) as DTE).ActiveDocument.Object() as EnvDTE.TextDocument;
                        List<EnvDTE.TextDocument> list = new List<EnvDTE.TextDocument>();
                        list.Add(activeDoc);
                        await DataAccessor.getInstance().FindVariablesInDocs(list);
                        FetchFromModels();
                    } catch (NullReferenceException) {
                        MessageBox.Show("Kein geöffnetes Dokument gefunden.");
                    }
                    break;
                case SelectionType.OpenDocuments:
                    var docs = (Package.GetGlobalService(typeof(DTE)) as DTE).Documents;
                    List<EnvDTE.TextDocument> docList = new List<EnvDTE.TextDocument>();
                    
                    foreach(EnvDTE.Document d in docs) {
                        docList.Add(ConvertFromComObjectToTextDocument(d));
                    }

                    await DataAccessor.getInstance().FindVariablesInDocs(docList);
                    FetchFromModels();

                    break;
                case SelectionType.Project:
                    Workspace workspace = DataAccessor.getInstance().Workspace;
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
                                if (!pi.IsOpen) {
                                    pi.Open();
                                }
                                edoclist.Add(pi.Document.Object() as EnvDTE.TextDocument);
                            } catch {

                            }
                        }
                    }

                    if (edoclist.Count == 0) {
                        MessageBox.Show("Es wurde keine Solution gefunden. Lädt das Projekt noch?");
                    } else {
                        await DataAccessor.getInstance().FindVariablesInDocs(edoclist);
                        FetchFromModels();
                    }

                    break;
            }
        }

        /// <summary>
        /// Casts a ComObject to EnvDTE.TextDocument
        /// </summary>
        /// <param name="comObject">Dynamic ComObject</param>
        /// <returns>If Cast was successful the EnvDTE.TextDocument or null if it wasn't successful</returns>
        private EnvDTE.TextDocument ConvertFromComObjectToTextDocument(dynamic comObject) {
            try {
                return comObject.Object;
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Applies the translation to the Documents. The Depth is defined in the Settings.
        /// </summary>
        public void ApplyChangesToDoc() {
            if (this.Count == 0) {
                MessageBox.Show("Keine Variablen gefunden, bitte suchen Sie erst nach Variablen.", "Fehler bei Aufruf");
                return;
            }

            try {
                WriteToModels();

                switch (SelectionType) {
                    case SelectionType.CurrentDocument:
                        EnvDTE.TextDocument activeDoc = (Package.GetGlobalService(typeof(DTE)) as DTE).ActiveDocument.Object() as EnvDTE.TextDocument;
                        DataAccessor.getInstance().TryApplyChangesToWorkspace(activeDoc);
                        break;
                    case SelectionType.OpenDocuments:
                        var docs = (Package.GetGlobalService(typeof(DTE)) as DTE).Documents;

                        foreach (EnvDTE.Document d in docs) {
                           d.Activate();
                            EnvDTE.TextDocument activeDoc2 = (Package.GetGlobalService(typeof(DTE)) as DTE).ActiveDocument.Object() as EnvDTE.TextDocument;
                            DataAccessor.getInstance().TryApplyChangesToWorkspace(activeDoc2);
                        }
                        
                        break;
                    case SelectionType.Project:
                        Workspace workspace = DataAccessor.getInstance().Workspace;
                        List<ProjectItem> pis = new List<ProjectItem>();

                        var ids = workspace.CurrentSolution.ProjectIds.GetEnumerator();

                        while (ids.MoveNext()) {
                            var roslynDocs = workspace.CurrentSolution.GetProject(ids.Current).Documents;

                            foreach (Microsoft.CodeAnalysis.Document doc in roslynDocs) {
                                pis.Add((Package.GetGlobalService(typeof(DTE)) as DTE).Solution.FindProjectItem(doc.FilePath));
                            }

                            foreach (ProjectItem pi in pis) {
                                try {
                                    if (!pi.IsOpen) {
                                        pi.Open();
                                    }
                                    EnvDTE.TextDocument doc= pi.Document.Object() as EnvDTE.TextDocument;
                                    pi.Document.Activate();
                                    DataAccessor.getInstance().TryApplyChangesToWorkspace(doc);
                                } catch {

                                }
                            }
                        }
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
    }
}
