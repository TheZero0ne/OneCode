//------------------------------------------------------------------------------
// <copyright file="OneCodeWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace OneCode.View {
    using global::DAL;
    using System.ComponentModel;
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
        }

        private void btnOpenConfig_Click(object sender, RoutedEventArgs e) {
            if (OneCodeConfigWindowCommand.Instance != null) {
                OneCodeConfigWindowCommand.Instance.ShowToolWindow(sender, e);
            } else {
                MessageBox.Show("Konfiguration kann nicht geöffnet werden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}