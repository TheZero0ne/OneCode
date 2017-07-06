using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCode {
    class VariableViewModel {
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

        public VariableNameInfo Name {
            get {
                return Var.Name;
            }
            set {
                Var.Name = value;
            }
        }

        public VariableNameInfo Translation {
            get {
                return Var.Translation;
            }
            set {
                Var.Translation = value;
            }
        }
    }
}
