//------------------------------------------------------------------------------
// <copyright file="OneCodeWindowPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.CodeAnalysis;
using DAL;
using System.Diagnostics;

namespace OneCode.View {
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(OneCodeWindow))]
    [ProvideToolWindow(typeof(OneCodeConfigWindow))]
    [Guid(OneCodeWindowPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class OneCodeWindowPackage : Package {
        /// <summary>
        /// OneCodeWindowPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "3294be3f-4de1-4135-a302-2542ff48d271";

        /// <summary>
        /// Initializes a new instance of the <see cref="OneCodeWindowPackage"/> class.
        /// </summary>
        public OneCodeWindowPackage() {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize() {
            var componentModel = (IComponentModel)this.GetService(typeof(SComponentModel));
            var workspace = componentModel.GetService<VisualStudioWorkspace>();

            DataAcessor.getInstance().Workspace = workspace;

            workspace.DocumentOpened += Workspace_DocumentOpened;
            workspace.WorkspaceChanged += Workspace_WorkspaceChanged;

            OneCodeWindowCommand.Initialize(this);
            OneCodeConfigWindowCommand.Initialize(this);
            OneCodeTranslateSelectionCommand.Initialize(this);
            base.Initialize();
        }

        private void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e) {
            Debug.WriteLine("Changed");
        }

        private void Workspace_DocumentOpened(object sender, DocumentEventArgs e) {
            DataAcessor.getInstance().ActualDocument = DataAcessor.getInstance().Workspace.CurrentSolution.GetDocument(e.Document.Id);
            Debug.WriteLine("Opened");
        }
    }
}
