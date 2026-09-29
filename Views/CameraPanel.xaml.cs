using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OpenCvSharp;

namespace Aurea.Views
{
    public partial class CameraPanel : UserControl
    {
        private DispatcherTimer frameTimer;
        private VideoCapture capture;
        private VideoWriter videoWriter;
        private bool isRunning;
        private bool isRecording = false;
        private DateTime recordingStartTime;
        private string recordingsFolder;

        public CameraPanel()
        {
            InitializeComponent();

            // Créer le dossier d'enregistrement dans Documents
            recordingsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Aurea",
                "Video Records",
                DateTime.Now.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(recordingsFolder);

            InitializeCamera();
        }

        private void InitializeCamera()
        {
            try
            {
                capture = new VideoCapture(0);
                if (!capture.IsOpened())
                {
                    GestureStatus.Text = "Caméra non disponible";
                    RecordButton.IsEnabled = false;
                    return;
                }

                isRunning = true;
                frameTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
                };
                frameTimer.Tick += FrameTimer_Tick;
                frameTimer.Start();

                GestureStatus.Text = "Caméra active - Prêt à enregistrer";
                RecordingStatus.Text = $"Dossier: {recordingsFolder}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur d'initialisation caméra: {ex.Message}", "Erreur");
                RecordButton.IsEnabled = false;
                StopCameraButton.IsEnabled = false;
            }
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRecording)
            {
                StartRecording();
                RecordButton.Content = "⏸ Arrêter l'enregistrement";
                RecordButton.Background = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                StopRecording();
                RecordButton.Content = "🔴 Démarrer l'enregistrement";
                RecordButton.Background = new SolidColorBrush(Colors.Red);
            }
        }

        private void StopCameraButton_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(recordingsFolder))
                {
                    System.Diagnostics.Process.Start("explorer.exe", recordingsFolder);
                    GestureStatus.Text = $"Dossier ouvert: {recordingsFolder}";
                }
                else
                {
                    MessageBox.Show($"Le dossier n'existe pas: {recordingsFolder}", "Erreur");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossible d'ouvrir le dossier: {ex.Message}", "Erreur");
            }
        }

        private void StartRecording()
        {
            if (!isRunning) return;

            try
            {
                // Générer un nom de fichier unique
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string recordingPath = Path.Combine(recordingsFolder, $"recording_{timestamp}.avi");

                // Récupérer les propriétés de la caméra
                int frameWidth = (int)capture.Get(VideoCaptureProperties.FrameWidth);
                int frameHeight = (int)capture.Get(VideoCaptureProperties.FrameHeight);
                double fps = capture.Get(VideoCaptureProperties.Fps);

                if (fps <= 0) fps = 30; // Valeur par défaut
                if (frameWidth <= 0) frameWidth = 640;
                if (frameHeight <= 0) frameHeight = 480;

                // Créer le VideoWriter
                videoWriter = new VideoWriter(recordingPath,
                    FourCC.FromString("MJPG"),
                    fps,
                    new OpenCvSharp.Size(frameWidth, frameHeight));

                recordingStartTime = DateTime.Now;
                isRecording = true;

                GestureStatus.Text = $"Enregistrement en cours: {Path.GetFileName(recordingPath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur démarrage enregistrement: {ex.Message}", "Erreur");
            }
        }

        private void StopRecording()
        {
            if (!isRecording) return;

            try
            {
                isRecording = false;
                videoWriter?.Release();
                videoWriter?.Dispose();
                videoWriter = null;

                TimeSpan duration = DateTime.Now - recordingStartTime;
                GestureStatus.Text = $"Enregistrement terminé ({duration:mm\\:ss})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur arrêt enregistrement: {ex.Message}", "Erreur");
            }
        }

        private void FrameTimer_Tick(object sender, EventArgs e)
        {
            if (!isRunning || capture == null) return;

            try
            {
                using (Mat frame = new Mat())
                {
                    capture.Read(frame);
                    if (!frame.Empty())
                    {
                        // Convertir et afficher la frame
                        CameraFeed.Source = MatToBitmapSource(frame);

                        // Écrire dans le fichier vidéo si l'enregistrement est actif
                        if (isRecording && videoWriter != null && videoWriter.IsOpened())
                        {
                            videoWriter.Write(frame);

                            // Mettre à jour le statut avec la durée
                            TimeSpan duration = DateTime.Now - recordingStartTime;
                            RecordingStatus.Text = $"Enregistrement... {duration:mm\\:ss}";
                        }

                        // Détection de gestes simplifiée
                        DetectGestures(frame);
                    }
                }
            }
            catch (Exception ex)
            {
                GestureStatus.Text = $"Erreur: {ex.Message}";
            }
        }

        private BitmapSource MatToBitmapSource(Mat mat)
        {
            try
            {
                if (mat.Channels() == 1)
                {
                    Cv2.CvtColor(mat, mat, ColorConversionCodes.GRAY2BGR);
                }

                int width = mat.Width;
                int height = mat.Height;
                int stride = width * mat.Channels();
                byte[] data = new byte[height * stride];

                System.Runtime.InteropServices.Marshal.Copy(mat.Data, data, 0, data.Length);

                PixelFormat pixelFormat = mat.Channels() == 3
                    ? PixelFormats.Bgr24
                    : PixelFormats.Bgra32;

                BitmapSource bitmap = BitmapSource.Create(
                    width,
                    height,
                    96,
                    96,
                    pixelFormat,
                    null,
                    data,
                    stride);

                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur conversion Mat: {ex.Message}");
                return null;
            }
        }

        private void DetectGestures(Mat frame)
        {
            // TODO: Implémenter détection avancée de gestes
            // Pour l'instant, simple détection de mouvement basique

            if (isRecording)
            {
                // Déjà géré dans FrameTimer_Tick
            }
            else
            {
                GestureStatus.Text = "Caméra active - Analyse en cours...";
            }
        }

        public void StopCamera()
        {
            isRunning = false;
            StopRecording();
            frameTimer?.Stop();
            capture?.Release();
            capture?.Dispose();
            CameraFeed.Source = null;
            GestureStatus.Text = "Caméra désactivée";
            RecordingStatus.Text = "";

            // Désactiver les boutons
            RecordButton.IsEnabled = false;
            StopCameraButton.IsEnabled = false;
            RecordButton.Content = "🔴 Caméra désactivée";
            RecordButton.Background = new SolidColorBrush(Colors.Gray);
        }
    }
}