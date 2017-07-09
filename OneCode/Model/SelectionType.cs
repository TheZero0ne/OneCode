using System.ComponentModel;

namespace OneCode {
    /// <summary>
    ///  Defines the depth of the search of variables for the translation 
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
