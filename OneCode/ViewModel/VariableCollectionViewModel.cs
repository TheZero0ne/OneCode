using DAL;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualStudio.Shell;

namespace OneCode {
    class VariableCollectionViewModel : ObservableCollection<VariableViewModel> {
        private RelayCommand findVariablesInDoc;
        private bool syncDisabled = false;

        public VariableCollectionViewModel() {
            findVariablesInDoc = new RelayCommand(this.FindVariablesInDoc, this.SyncDisabled);
        }

        public void FetchFromModels() {
            // While this boolean is true, no other sync-methods will be executed
            this.syncDisabled = true;
            this.Clear();

            foreach (Variable v in DataAcessor.getInstance().varCollection)
                this.Add(new VariableViewModel(v));

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

        public void FindVariablesInDoc() {
            TextDocument activeDoc = (Package.GetGlobalService(typeof(DTE)) as DTE).ActiveDocument.Object() as TextDocument;

            DataAcessor.getInstance().FindVariablesInDoc(activeDoc);
            FetchFromModels();
        }

        #endregion
    }
}
