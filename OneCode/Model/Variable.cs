using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using System.Globalization;

namespace OneCode {
    [Serializable]
    class Variable {

        public CultureInfo FromLanguage { get; set; }
        public CultureInfo ToLanguage { get; set; }
        public String Type { get; set; }
        public String Name { get; set; }
        public String Text { get; set; }
        public String Translation { get; set; }
        public String Kind { get; set; }
        public int SpanStart { get; set; }

        public Variable() {

        }

        public Variable(string _type, string _name, string _kind, int _spanStart) {
            this.Type = _type;
            this.Name = _name;
            this.Text = _type + " " + _name;
            this.Kind = _kind;
            this.SpanStart = _spanStart;
        }

    }
}
