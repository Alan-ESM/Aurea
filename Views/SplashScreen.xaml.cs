using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Aurea.Views
{
    public partial class SplashScreen : Window
    {
        private DispatcherTimer progressTimer;
        private double currentProgress = 0;
        private Random random = new Random();

        public SplashScreen()
        {
            InitializeComponent();
            InitializeStars();
            StartLoading();
        }

        private void InitializeStars()
        {
            // Créer des étoiles scintillantes
            for (int i = 0; i < 100; i++)
            {
                Ellipse star = new Ellipse
                {
                    Width = random.Next(1, 3),
                    Height = random.Next(1, 3),
                    Fill = new SolidColorBrush(Color.FromArgb(
                        (byte)random.Next(150, 255),
                        255, 255, 255))
                };

                Canvas.SetLeft(star, random.Next(0, 900));
                Canvas.SetTop(star, random.Next(0, 600));
                StarsCanvas.Children.Add(star);

                // Animation de scintillement
                DoubleAnimation twinkle = new DoubleAnimation
                {
                    From = 0.2,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(random.Next(2, 5)),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                star.BeginAnimation(OpacityProperty, twinkle);
            }
        }

        private async void StartLoading()
        {
            progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };
            progressTimer.Tick += ProgressTimer_Tick;
            progressTimer.Start();

            // Simuler les étapes de chargement
            await Task.Run(async () =>
            {
                await SimulateLoading("Chargement des modules IA...", 25);
                await SimulateLoading("Initialisation de la base de données...", 50);
                await SimulateLoading("Configuration de la reconnaissance vocale...", 75);
                await SimulateLoading("Préparation de la caméra...", 90);
                await SimulateLoading("Finalisation...", 100);
            });

            await Task.Delay(500);

            // Ouvrir la fenêtre principale
            Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            });
        }

        private async Task SimulateLoading(string message, double targetProgress)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                LoadingText.Text = message;
            });

            while (currentProgress < targetProgress)
            {
                await Task.Delay(50);
            }
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (currentProgress < 100)
            {
                currentProgress += 0.5;
                ProgressBar.Width = (currentProgress / 100) * 400;
            }
            else
            {
                progressTimer.Stop();
            }
        }
    }
}