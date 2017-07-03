//------------------------------------------------------------------------------
// <copyright file="OneCodeWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace OneCode.View {
    using Microsoft.CodeAnalysis;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Properties;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
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

            //Populate CultureInfos
            List<CultureInfo> ciList = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.AllCultures));
            this.languages.DisplayMemberPath = "DisplayName";
            this.languages.ItemsSource = ciList;

            List<CodeStyle> styles = Enum.GetValues(typeof(CodeStyle)).Cast<CodeStyle>().ToList();
            this.codestyles.ItemsSource = styles;

        }

        private void languages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show("Sprache in Settings beim Change " + Settings.Default.CodeLanguage.DisplayName);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            this.Visibility = Visibility.Hidden;
        }
    }
}