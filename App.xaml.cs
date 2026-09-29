using Application = System.Windows.Application;
using System.Configuration;
using System.Data;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace Aurea
{
        public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configuration globale de l'application
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Vous pouvez ajouter d'autres initialisations ici
            InitializeApplication();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Gestion des exceptions non gérées
            MessageBox.Show($"Une erreur inattendue s'est produite: {e.Exception.Message}",
                          "Erreur",
                          MessageBoxButton.OK,
                          MessageBoxImage.Error);
            e.Handled = true;
        }

        private void InitializeApplication()
        {
            // Initialisation des ressources ou services globaux
            // Par exemple: styles, thèmes, services, etc.

            // Configuration des couleurs du thème si nécessaire
            // Application.Current.Resources.MergedDictionaries.Add(...);
        }
    }
}