using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCode {
    /// <summary>
    ///  Definiert, nach welcher Logik die einzelnen Wörter bei Variablen aufgetrennt werden
    /// </summary>
    public enum CodeStyle {
        [Description("Camel Case")]
        CamelCase,

        [Description("Unterlined")]
        UnderLine
    }
}
