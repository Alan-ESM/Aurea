using System;
using System.Collections.Generic;
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
    public partial class CollaboratorDialog : Window
    {
        public string CollaboratorName { get; private set; }
        public string CollaboratorEmail { get; private set; }
        public string CollaboratorRole { get; private set; }

        public CollaboratorDialog()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Veuillez entrer un nom", "Erreur",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Veuillez entrer un email", "Erreur",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CollaboratorName = NameTextBox.Text;
            CollaboratorEmail = EmailTextBox.Text;
            CollaboratorRole = RoleComboBox.Text;
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
