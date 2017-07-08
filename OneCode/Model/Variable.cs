using System;
using System.Globalization;

namespace OneCode {
    [Serializable]
    class Variable {
        public CultureInfo FromLanguage { get; set; }
        public CultureInfo ToLanguage { get; set; }
        public String Type { get; set; }
        public VariableNameInfo Name { get; set; }
        public String Text { get; set; }
        public VariableNameInfo Translation { get; set; }
        public String Kind { get; set; }
        public int SpanStart { get; set; }
        public string DocumentName { get; set; }

        public Variable() {

        }

        public Variable(string _type, string _name, string _kind, int _spanStart, string _docName) {
            this.Type = _type;
            this.Name = new VariableNameInfo(_name);
            this.Text = _type + " " + Name.Content;
            this.Kind = _kind;
            this.SpanStart = _spanStart;
            this.DocumentName = _docName;
        }
    }
}
