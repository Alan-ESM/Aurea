using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Aurea.Views
{
    public partial class ParagraphSettingsDialog : Window
    {
        public TextAlignment SelectedAlignment { get; private set; }
        public double SelectedLineSpacing { get; private set; }
        public double SelectedLeftIndent { get; private set; }
        public double SelectedRightIndent { get; private set; }

        // Ajoutez ces champs privés à la classe ParagraphSettingsDialog pour corriger les erreurs CS0103
        private System.Windows.Controls.ComboBox AlignmentComboBox;
        private System.Windows.Controls.TextBox LineSpacingTextBox;
        private System.Windows.Controls.TextBox LeftIndentTextBox;
        private System.Windows.Controls.TextBox RightIndentTextBox;

        public ParagraphSettingsDialog()
        {
                InitializeComponent();
                AlignmentComboBox.SelectedIndex = 0;
                LineSpacingTextBox.Text = "1.15";
                LeftIndentTextBox.Text = "0";
                RightIndentTextBox.Text = "0";
            }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            switch (AlignmentComboBox.SelectedIndex)
            {
                case 0: SelectedAlignment = TextAlignment.Left; break;
                case 1: SelectedAlignment = TextAlignment.Center; break;
                case 2: SelectedAlignment = TextAlignment.Right; break;
                case 3: SelectedAlignment = TextAlignment.Justify; break;
            }

            if (double.TryParse(LineSpacingTextBox.Text, out double spacing))
                SelectedLineSpacing = spacing;
            else
                SelectedLineSpacing = 1.15;

            if (double.TryParse(LeftIndentTextBox.Text, out double leftIndent))
                SelectedLeftIndent = leftIndent;

            if (double.TryParse(RightIndentTextBox.Text, out double rightIndent))
                SelectedRightIndent = rightIndent;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}