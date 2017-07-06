//------------------------------------------------------------------------------
// <copyright file="OneCodeWindowCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows;
using EnvDTE;
using OneCode;

namespace OneCode.View
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OneCodeTranslateSelectionCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0300;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("A1221E65-866E-4F9F-B041-0AF3541C1771");
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        private DTE _dte = null;
        private DTE GetDTE()
        {
            if (_dte == null)
            {
                _dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            }
            return _dte;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneCodeTranslateSelectionCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private OneCodeTranslateSelectionCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.TranslateSelection, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OneCodeTranslateSelectionCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new OneCodeTranslateSelectionCommand(package);
        }

        private void BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand pasteCommand = (OleMenuCommand)sender;

            // disabled by default
            pasteCommand.Enabled = false;

            Document activeDoc = GetDTE().ActiveDocument;
            if (activeDoc != null && activeDoc.ProjectItem != null && activeDoc.ProjectItem.ContainingProject != null)
            {
                // enable command, if there is text selected
                var selection = (TextSelection)activeDoc.Selection;
                pasteCommand.Enabled = selection != null && selection.Text.Length > 0;
            }
        }


        /// <summary>
        /// Translates the currentSelected
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        public async void TranslateSelection(object sender, EventArgs e)
        {
            
            DTE dte = GetDTE();
            try {
                dte.UndoContext.Open("Übersetze Auswahl");

                var selection = (TextSelection)dte.ActiveDocument.Selection;
                if (selection != null)
                {
                    string selectedText = selection.Text;
                    string splitted = VariableFormatter.SplitString(selectedText);
                    string translation = await Translator.TranslateString(splitted);

                    selection.ReplacePattern(selectedText, translation);
                    dte.ActiveDocument.Activate();
                    dte.ExecuteCommand("Edit.FormatDocument");
                }
            }
            finally
            {
                dte.UndoContext.Close();
            }
        }
    }
}
