//------------------------------------------------------------------------------
// <copyright file="MyQuickTransalationCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;

namespace OneCode {
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class MyQuickTransalationCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("6bd9e375-9b77-4964-b500-4a74e796e233");

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
        /// Initializes a new instance of the <see cref="MyQuickTransalationCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private MyQuickTransalationCommand(Package package)
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
        public static MyQuickTransalationCommand Instance
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
            Instance = new MyQuickTransalationCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void TranslateSelection(object sender, EventArgs e)
        {

            DTE dte = GetDTE();
            try
            {
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
