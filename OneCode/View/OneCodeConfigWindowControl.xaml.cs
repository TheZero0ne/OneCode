//------------------------------------------------------------------------------
// <copyright file="OneCodeWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace OneCode.View {
    using Properties;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for OneCodeWindowControl.
    /// </summary>
    public partial class OneCodeConfigWindowControl : UserControl {
        /// <summary>
        /// Initializes a new instance of the <see cref="OneCodeConfigWindowControl"/> class.
        /// </summary>
        public OneCodeConfigWindowControl() {
            this.InitializeComponent();
        }

        private void settingSelectionChanged(object sender, SelectionChangedEventArgs e) {
            // Save Settings on every Settings-Change
            Settings.Default.Save();
        }

    }
}