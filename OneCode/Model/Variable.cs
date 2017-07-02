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

        public Variable() {

        }

    }
}
