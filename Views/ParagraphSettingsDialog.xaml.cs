using System;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;

namespace Aurea.Views
{
    public partial class ParagraphSettingsDialog : Window
    {
        public TextAlignment SelectedAlignment { get; private set; }
        public double SelectedLineSpacing { get; private set; }
        public double SelectedLeftIndent { get; private set; }
        public double SelectedRightIndent { get; private set; }

        public ParagraphSettingsDialog()
        {
            InitializeComponent();
            AlignmentComboBox.SelectedIndex = 0;
            LineSpacingTextBox.Text = 1.15.ToString(CultureInfo.CurrentCulture);
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

            if (double.TryParse(LineSpacingTextBox.Text, NumberStyles.Float,
                    CultureInfo.CurrentCulture, out double spacing))
                SelectedLineSpacing = spacing;
            else
                SelectedLineSpacing = 1.15;

            if (double.TryParse(LeftIndentTextBox.Text, NumberStyles.Float,
                    CultureInfo.CurrentCulture, out double leftIndent))
                SelectedLeftIndent = leftIndent;

            if (double.TryParse(RightIndentTextBox.Text, NumberStyles.Float,
                    CultureInfo.CurrentCulture, out double rightIndent))
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
