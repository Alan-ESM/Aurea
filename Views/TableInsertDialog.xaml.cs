using System.Windows;

namespace Aurea.Views
{
    public partial class TableInsertDialog : Window
    {
        public int Rows { get; private set; }
        public int Columns { get; private set; }

        public TableInsertDialog()
        {
            InitializeComponent();
            RowsTextBox.Text = "3";
            ColumnsTextBox.Text = "3";
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(RowsTextBox.Text, out int rows) &&
                int.TryParse(ColumnsTextBox.Text, out int columns))
            {
                if (rows > 0 && rows <= 50 && columns > 0 && columns <= 20)
                {
                    Rows = rows;
                    Columns = columns;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Valeurs invalides. Lignes: 1-50, Colonnes: 1-20",
                                  "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Veuillez entrer des nombres valides",
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}