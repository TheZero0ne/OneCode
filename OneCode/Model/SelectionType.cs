using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCode {
    /// <summary>
    ///  Definiert den Such- und Anwendungsradius für OneCode 
    /// </summary>
    public enum SelectionType {
        [Description("aktuelles (offenes) Dokument")]
        CurrentDocument,

        [Description("Alle offenen Dokumente")]
        OpenDocuments,

        [Description("Gesamtes Projekt")]
        Project
    }
}
