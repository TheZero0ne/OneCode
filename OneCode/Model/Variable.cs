using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace OneCode {
    [Serializable]
    class Variable {
        public String Name { get; set; }
        public TextDocument Document { get; set; }
        public CodeRange TextRange { get; set; }
        public String Text { get; set; }
        public Type Type { get; set; }
        public int LineNumber { get; set; }

        public Variable() {

        }

        public Variable(String _name, String _txt, int _lineNumber, TextDocument _txtDoc, CodeRange _txtRng, Type _type) {
            Name = _name;
            Text = _txt;
            Document = _txtDoc;
            TextRange = _txtRng;
            Type = _type;
            LineNumber = _lineNumber;
        }

        public Variable(String _name, String _txt, TextDocument _txtDoc, Type _type) {
            Name = _name;
            Text = _txt;
            Document = _txtDoc;
            Type = _type;
            LineNumber = 0;
        }
    }
}
