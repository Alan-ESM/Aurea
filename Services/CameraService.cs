using System;
using System.IO;
using System.Windows;
using OpenCvSharp;
using System.Windows.Media;
using System.Windows.Threading;

namespace Aurea.Services
{
    public class CameraService : IDisposable
    {
        private VideoCapture capture;
        private VideoWriter videoWriter;
        private DispatcherTimer recordingTimer;
        private bool isRunning = false;
        private bool isRecording = false;
        private string recordingPath = "";
        private DateTime recordingStartTime;

        private readonly string recordingsFolder;

        public CameraService()
        {
            // Créer le dossier d'enregistrement si nécessaire
            recordingsFolder = Path.Combine(Environment.CurrentDirectory, "Videos", "my-records");
            Directory.CreateDirectory(recordingsFolder);
        }

        public void StartCamera()
        {
            if (isRunning) return;

            try
            {
                capture = new VideoCapture(0);
                if (!capture.IsOpened())
                {
                    throw new Exception("Impossible d'ouvrir la caméra.");
                }

                isRunning = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur caméra: {ex.Message}", "Erreur");
            }
        }

        public void StopCamera()
        {
            if (!isRunning) return;

            StopRecording();

            capture?.Release();
            isRunning = false;
        }

        public void StartRecording()
        {
            if (!isRunning || isRecording) return;

            try
            {
                // Générer un nom de fichier unique
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                recordingPath = Path.Combine(recordingsFolder, $"recording_{timestamp}.avi");

                // Récupérer les propriétés de la caméra
                int frameWidth = (int)capture.Get(VideoCaptureProperties.FrameWidth);
                int frameHeight = (int)capture.Get(VideoCaptureProperties.FrameHeight);
                double fps = capture.Get(VideoCaptureProperties.Fps);

                if (fps <= 0) fps = 30; // Valeur par défaut

                // Créer le VideoWriter
                videoWriter = new VideoWriter(recordingPath,
                    FourCC.FromString("MJPG"),
                    fps,
                    new OpenCvSharp.Size(frameWidth, frameHeight));

                recordingStartTime = DateTime.Now;
                isRecording = true;

                // Démarrer le timer pour la durée d'enregistrement
                recordingTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                recordingTimer.Tick += RecordingTimer_Tick;
                recordingTimer.Start();

                OnRecordingStatusChanged?.Invoke(this, $"Enregistrement démarré: {recordingPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur démarrage enregistrement: {ex.Message}", "Erreur");
            }
        }

        public void StopRecording()
        {
            if (!isRecording) return;

            try
            {
                isRecording = false;
                recordingTimer?.Stop();

                videoWriter?.Release();
                videoWriter?.Dispose();
                videoWriter = null;

                TimeSpan duration = DateTime.Now - recordingStartTime;
                OnRecordingStatusChanged?.Invoke(this, $"Enregistrement terminé ({duration:mm\\:ss})");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur arrêt enregistrement: {ex.Message}", "Erreur");
            }
        }

        public Mat CaptureFrame()
        {
            if (!isRunning || capture == null) return null;

            try
            {
                var frame = new Mat();
                capture.Read(frame);

                if (!frame.Empty())
                {
                    // Écrire dans le fichier vidéo si l'enregistrement est actif
                    if (isRecording && videoWriter != null && videoWriter.IsOpened())
                    {
                        videoWriter.Write(frame);
                    }

                    return frame;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur capture: {ex.Message}");
            }

            return null;
        }

        private void RecordingTimer_Tick(object sender, EventArgs e)
        {
            if (isRecording)
            {
                TimeSpan duration = DateTime.Now - recordingStartTime;
                OnRecordingTimeUpdated?.Invoke(this, duration);
            }
        }

        public bool IsRecording => isRecording;
        public bool IsCameraRunning => isRunning;
        public string RecordingPath => recordingPath;

        public event EventHandler<string> GestureDetected;
        public event EventHandler<string> ScreamDetected;
        public event EventHandler<string> OnRecordingStatusChanged;
        public event EventHandler<TimeSpan> OnRecordingTimeUpdated;

        protected virtual void OnGestureDetected(string gesture)
        {
            GestureDetected?.Invoke(this, gesture);
        }

        protected virtual void OnScreamDetected(string screamInfo)
        {
            ScreamDetected?.Invoke(this, screamInfo);
        }

        public void Dispose()
        {
            StopRecording();
            StopCamera();
            capture?.Dispose();
            videoWriter?.Dispose();
        }
    }
}