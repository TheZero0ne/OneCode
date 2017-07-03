//------------------------------------------------------------------------------
// <copyright file="OneCodeWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace OneCode.View {
    using EnvDTE;
    using Microsoft.CodeAnalysis;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for OneCodeWindowControl.
    /// </summary>
    public partial class OneCodeWindowControl : UserControl {
        /// <summary>
        /// Initializes a new instance of the <see cref="OneCodeWindowControl"/> class.
        /// </summary>
        public OneCodeWindowControl() {
            this.InitializeComponent();

            List<SelectionType> selectionTypes = Enum.GetValues(typeof(SelectionType)).Cast<SelectionType>().ToList();
            this.selectionTypes.ItemsSource = selectionTypes;
            this.selectionTypes.SelectedItem = selectionTypes.First();
            //this.selectionTypes.DisplayMemberPath = "Description";
        }

        private void btnOpenConfig_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}