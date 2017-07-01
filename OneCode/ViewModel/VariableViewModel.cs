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

        public int LineNumber {
            get { return Var.LineNumber; }
            set { Var.LineNumber = value; }
        }
    }
}
