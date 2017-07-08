using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCode {
    class VariableViewModel
    {
        public Variable Var { get; }

        public VariableViewModel() {
            Var = new Variable();
        }

        public VariableViewModel(Variable _var) {
            Var = _var;
        }

        public string Text {
            get { return Var.Text; }
            set { Var.Text = value; }
        }

        public string Type {
            get { return Var.Type; }
            set { Var.Type = value; }
        }

        public string Kind {
            get { return Var.Kind; }
            set { Var.Kind = value; }
        }

        public string DocumentName {
            get { return Var.DocumentName; }
            set { Var.DocumentName = value; }
        }

        public string Name {
            get {
                return Var.Name.GetContentWithPrefix();
            }
            set {
                Var.Name.Content = value;
            }
        }

        public string Translation {
            get {
                return Var.Translation.GetContentWithPrefix();
            }
            set {
                Var.Translation.Content = value;
            }
        }

    }
}
