using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using Aurea.Services;
using Aurea.Database;
using WinFormsBrushes = System.Drawing.Brushes;
using WpfBrushes = System.Windows.Media.Brushes;
using WinFormsColor = System.Drawing.Color;
using WpfColor = System.Windows.Media.Color;

namespace Aurea.Views
{
    public partial class MainWindow : Window
    {
        private AIService aiService;
        private CameraPanel cameraPanel;
        private VoiceService voiceService;
        private DatabaseService databaseService;
        private DocumentService documentService;

        private string currentFilePath = null;
        private bool isModified = false;
        private bool isDarkTheme = true;

        public MainWindow()
        {
            InitializeComponent();
            InitializeServices();
            LoadFontSettings();
            SetupEventHandlers();
        }

        private void InitializeServices()
        {
            try
            {
                aiService = new AIService();
                voiceService = new VoiceService();
                databaseService = new DatabaseService();
                documentService = new DocumentService();

                // Activer le débogage vocal
                voiceService.EnableDebugLogging(true);

                StatusText.Text = "Services initialisés avec succès";
            }
            catch (Exception ex)
            {
                documentService ??= new DocumentService();
                MessageBox.Show($"Erreur lors de l'initialisation : {ex.Message}",
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFontSettings()
        {
            var fonts = System.Windows.Media.Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            FontFamilyCombo.ItemsSource = fonts;
            FontFamilyCombo.SelectedItem = new System.Windows.Media.FontFamily("Segoe UI");

            // Chargement des tailles
            FontSizeCombo.ItemsSource = new double[] { 8.0, 9.0, 10.0, 11.0, 12.0, 14.0, 16.0, 18.0, 20.0, 22.0, 24.0, 28.0, 32.0, 36.0, 48.0, 72.0 };
            FontSizeCombo.SelectedItem = 14.0;
        }

        private void SetupEventHandlers()
        {
            DocumentEditor.TextChanged += DocumentEditor_TextChanged;
            DocumentEditor.SelectionChanged += DocumentEditor_SelectionChanged;
        }

        // === ÉVÉNEMENTS FICHIER ===
        private void NewDocument_Click(object sender, RoutedEventArgs e)
        {
            if (isModified && !ConfirmSave())
                return;

            DocumentEditor.Document = new FlowDocument();
            currentFilePath = null;
            isModified = false;
            UpdateTitle();
            StatusText.Text = "Nouveau document créé";
        }

        private void OpenDocument_Click(object sender, RoutedEventArgs e)
        {
            if (isModified && !ConfirmSave())
                return;

            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Tous les documents|*.docx;*.txt;*.rtf;*.pdf;*.md;*.html;*.htm|" +
                        "Word (*.docx)|*.docx|" +
                        "Texte (*.txt)|*.txt|" +
                        "RTF (*.rtf)|*.rtf|" +
                        "PDF (*.pdf)|*.pdf|" +
                        "Markdown (*.md)|*.md|" +
                        "HTML (*.html;*.htm)|*.html;*.htm"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    documentService.LoadDocument(dialog.FileName, DocumentEditor);
                    currentFilePath = dialog.FileName;
                    isModified = false;
                    UpdateTitle();
                    StatusText.Text = $"Document ouvert : {Path.GetFileName(dialog.FileName)}";

                    databaseService.SaveDocumentBackup(dialog.FileName,
                        documentService.GetDocumentText(DocumentEditor));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'ouverture : {ex.Message}",
                                  "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveDocument_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveAs();
            }
            else
            {
                SaveDocument();
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        private bool SaveAs()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Word (*.docx)|*.docx|" +
                        "Texte (*.txt)|*.txt|" +
                        "RTF (*.rtf)|*.rtf|" +
                        "PDF (*.pdf)|*.pdf|" +
                        "Markdown (*.md)|*.md|" +
                        "HTML (*.html)|*.html"
            };

            if (dialog.ShowDialog() != true)
                return false;

            string previousFilePath = currentFilePath;
            currentFilePath = dialog.FileName;
            if (SaveDocument())
                return true;

            currentFilePath = previousFilePath;
            UpdateTitle();
            return false;
        }

        private bool SaveDocument()
        {
            try
            {
                string content = documentService.GetDocumentText(DocumentEditor);
                documentService.SaveDocument(currentFilePath, DocumentEditor);

                databaseService.SaveDocumentBackup(currentFilePath, content);

                isModified = false;
                UpdateTitle();
                StatusText.Text = $"Document sauvegardé : {Path.GetFileName(currentFilePath)}";
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}",
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // === ÉVÉNEMENTS FORMATAGE ===
        private void Bold_Click(object sender, RoutedEventArgs e)
        {
            DocumentEditor.Selection.ApplyPropertyValue(
                TextElement.FontWeightProperty,
                FontWeights.Bold);
        }

        private void Italic_Click(object sender, RoutedEventArgs e)
        {
            DocumentEditor.Selection.ApplyPropertyValue(
                TextElement.FontStyleProperty,
                FontStyles.Italic);
        }

        private void Underline_Click(object sender, RoutedEventArgs e)
        {
            DocumentEditor.Selection.ApplyPropertyValue(
                Inline.TextDecorationsProperty,
                TextDecorations.Underline);
        }

        private void AlignLeft_Click(object sender, RoutedEventArgs e)
        {
            DocumentEditor.Selection.ApplyPropertyValue(
                Paragraph.TextAlignmentProperty,
                TextAlignment.Left);
        }

        private void AlignCenter_Click(object sender, RoutedEventArgs e)
        {
            DocumentEditor.Selection.ApplyPropertyValue(
                Paragraph.TextAlignmentProperty,
                TextAlignment.Center);
        }

        private void AlignRight_Click(object sender, RoutedEventArgs e)
        {
            DocumentEditor.Selection.ApplyPropertyValue(
                Paragraph.TextAlignmentProperty,
                TextAlignment.Right);
        }

        private void AlignJustify_Click(object sender, RoutedEventArgs e)
        {
            DocumentEditor.Selection.ApplyPropertyValue(
                Paragraph.TextAlignmentProperty,
                TextAlignment.Justify);
        }

        private void FontFamily_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (FontFamilyCombo.SelectedItem != null)
            {
                DocumentEditor.Selection.ApplyPropertyValue(
                    TextElement.FontFamilyProperty,
                    FontFamilyCombo.SelectedItem);
            }
        }

        private void FontSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeCombo.SelectedItem is double size)
            {
                DocumentEditor.Selection.ApplyPropertyValue(
                    TextElement.FontSizeProperty,
                    size);
            }
        }

        private void TextColor_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var color = WpfColor.FromArgb(colorDialog.Color.A,
                                         colorDialog.Color.R,
                                         colorDialog.Color.G,
                                         colorDialog.Color.B);
                DocumentEditor.Selection.ApplyPropertyValue(
                    TextElement.ForegroundProperty,
                    new SolidColorBrush(color));
            }
        }

        private void Highlight_Click(object sender, RoutedEventArgs e)
        {
            DocumentEditor.Selection.ApplyPropertyValue(
                TextElement.BackgroundProperty,
                new SolidColorBrush(WpfColor.FromRgb(255, 255, 0)));
        }

        // === ÉVÉNEMENTS PANNEAUX LATÉRAUX ===
        private async void OpenAI_Click(object sender, RoutedEventArgs e)
        {
            StopCameraPanel();
            AnimateSidePanel(400);
            SidePanelTitle.Text = "🤖 Assistant IA";

            var aiPanel = new AIAssistantPanel(aiService, DocumentEditor);
            SidePanelContent.Children.Clear();
            SidePanelContent.Children.Add(aiPanel);

            string documentContent = documentService.GetDocumentText(DocumentEditor);
            if (!string.IsNullOrEmpty(documentContent))
            {
                StatusText.Text = "L'IA analyse votre document...";
                await aiService.AnalyzeDocument(documentContent);
            }
        }

        private void ToggleCamera_Click(object sender, RoutedEventArgs e)
        {
            if (cameraPanel == null)
            {
                AnimateSidePanel(400);
                SidePanelTitle.Text = "📹 Caméra & Gestes";

                cameraPanel = new CameraPanel();
                SidePanelContent.Children.Clear();
                SidePanelContent.Children.Add(cameraPanel);
            }
            else
            {
                StopCameraPanel();
                AnimateSidePanel(0);
            }
        }

        private void StopCameraPanel()
        {
            if (cameraPanel == null)
                return;

            cameraPanel.StopCamera();
            cameraPanel = null;
            SidePanelContent.Children.Clear();
        }

        private void ToggleVoice_Click(object sender, RoutedEventArgs e)
        {
            VoiceInputCheckbox.IsChecked = !VoiceInputCheckbox.IsChecked;
        }

        // === ÉVÉNEMENTS VOCAUX ===
        private async void VoiceInput_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Activation de la reconnaissance vocale...";

                // Vérifier si le service vocal est initialisé
                if (voiceService == null)
                {
                    StatusText.Text = "Service vocal non initialisé";
                    VoiceInputCheckbox.IsChecked = false;
                    return;
                }

                // Activer la dictée continue
                voiceService.SetContinuousDictation(true);

                await voiceService.StartListening((transcribedText) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            Console.WriteLine($"Texte reçu: '{transcribedText}'");

                            if (!string.IsNullOrWhiteSpace(transcribedText))
                            {
                                // Insérer le texte à la fin du document
                                DocumentEditor.AppendText(transcribedText + " ");

                                StatusText.Text = $"Transcrit: {transcribedText}";

                                // Mettre à jour le compteur
                                UpdateWordCount();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erreur insertion: {ex.Message}");
                            StatusText.Text = "Erreur d'insertion";
                        }
                    });
                });

                StatusText.Text = "🎤 Microphone activé - Parlez maintenant";
                VoiceInputCheckbox.IsChecked = true;

                // Feedback audio
                await voiceService.SpeakText("Saisie vocale activée");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception complète: {ex}");
                StatusText.Text = $"Erreur: {ex.Message}";
                VoiceInputCheckbox.IsChecked = false;
                MessageBox.Show($"Impossible d'activer la saisie vocale: {ex.Message}", "Erreur");
            }
        }

        private async void VoiceInput_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                await voiceService.StopListening();
                StatusText.Text = "Saisie vocale désactivée";
                VoiceInputCheckbox.IsChecked = false;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Erreur désactivation: {ex.Message}";
            }
        }

        private async void TestVoiceRecognition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Test de reconnaissance vocale...";

                // Tester si le service est disponible
                bool isAvailable = VoiceService.IsVoiceRecognitionAvailable();

                if (!isAvailable)
                {
                    MessageBox.Show("La reconnaissance vocale Windows n'est pas disponible sur ce système.\n" +
                                  "Assurez-vous que:\n" +
                                  "1. Vous êtes sur Windows\n" +
                                  "2. La reconnaissance vocale est installée\n" +
                                  "3. Un microphone est connecté",
                                  "Configuration requise");
                    return;
                }

                // Tester avec une reconnaissance simple
                MessageBoxResult result = MessageBox.Show(
                    "Test de reconnaissance vocale\n\n" +
                    "Cliquez sur OK, puis dites quelque chose.\n" +
                    "Le texte devrait apparaître dans l'éditeur.",
                    "Test vocal",
                    MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    StatusText.Text = "Écoute... dites quelque chose";

                    bool testComplete = false;
                    string testResult = "";

                    await voiceService.StartListening((text) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            testResult = text;
                            testComplete = true;

                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                DocumentEditor.AppendText($"[TEST: {text}] ");
                                MessageBox.Show($"Reconnu: '{text}'\n\nTest réussi!", "Résultat du test");
                                StatusText.Text = "Test réussi!";
                            }
                            else
                            {
                                MessageBox.Show("Aucun texte reconnu.\nVérifiez votre microphone.", "Échec du test");
                                StatusText.Text = "Test échoué";
                            }
                        });
                    });

                    // Attendre 10 secondes max pour le test
                    await Task.Delay(10000);

                    if (!testComplete)
                    {
                        await voiceService.StopListening();
                        MessageBox.Show("Aucune réponse dans le délai.\nVérifiez votre microphone.", "Délai expiré");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de test: {ex.Message}", "Erreur");
            }
            finally
            {
                await voiceService.StopListening();
            }
        }

        // === ÉVÉNEMENTS DIVERS ===
        private void GenerateTOC_Click(object sender, RoutedEventArgs e)
        {
            documentService.GenerateTableOfContents(DocumentEditor);
            StatusText.Text = "Table des matières générée";
        }

        private void InsertTable_Click(object sender, RoutedEventArgs e)
        {
            var tableDialog = new TableInsertDialog();
            if (tableDialog.ShowDialog() == true)
            {
                documentService.InsertTable(DocumentEditor,
                    tableDialog.Rows,
                    tableDialog.Columns);
            }
        }

        private void InsertImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Images|*.jpg;*.jpeg;*.png;*.gif;*.bmp"
            };

            if (dialog.ShowDialog() == true)
            {
                documentService.InsertImage(DocumentEditor, dialog.FileName);
            }
        }

        private void ImportPDF_Click(object sender, RoutedEventArgs e)
        {
            if (isModified && !ConfirmSave())
                return;

            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string text = documentService.ExtractTextFromPDF(dialog.FileName);
                    DocumentEditor.Document = new FlowDocument(new Paragraph(new Run(text)));
                    StatusText.Text = "PDF importé avec succès";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'import PDF : {ex.Message}");
                }
            }
        }

        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    documentService.ExportToPDF(DocumentEditor, dialog.FileName);
                    StatusText.Text = "Document exporté en PDF";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l’export PDF : {ex.Message}", "Export impossible",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintDocument(
                    ((IDocumentPaginatorSource)DocumentEditor.Document).DocumentPaginator,
                    "Impression Aurea");
            }
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            isDarkTheme = !isDarkTheme;
            ApplyTheme();
        }

        private void AddCollaborator_Click(object sender, RoutedEventArgs e)
        {
            var collabDialog = new CollaboratorDialog();
            collabDialog.ShowDialog();
        }

        private void FontSettings_Click(object sender, RoutedEventArgs e)
        {
            var fontDialog = new FontSettingsDialog();
            fontDialog.ShowDialog();
        }

        private void ParagraphSettings_Click(object sender, RoutedEventArgs e)
        {
            var paraDialog = new ParagraphSettingsDialog();
            paraDialog.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (isModified && !ConfirmSave())
                return;
            Application.Current.Shutdown();
        }

        // === MÉTHODES UTILITAIRES ===
        private void AnimateSidePanel(double targetWidth)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                To = targetWidth,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            SidePanel.BeginAnimation(WidthProperty, animation);
        }

        private void ApplyTheme()
        {
            if (isDarkTheme)
            {
                this.Background = new SolidColorBrush(WpfColor.FromRgb(30, 30, 46));
                DocumentEditor.Background = new SolidColorBrush(WpfColor.FromRgb(42, 42, 62));
                DocumentEditor.Foreground = WpfBrushes.White;
            }
            else
            {
                this.Background = WpfBrushes.White;
                DocumentEditor.Background = WpfBrushes.White;
                DocumentEditor.Foreground = WpfBrushes.Black;
            }
        }

        private void UpdateTitle()
        {
            string fileName = string.IsNullOrEmpty(currentFilePath)
                ? "Sans titre"
                : Path.GetFileName(currentFilePath);
            string modified = isModified ? "*" : "";
            this.Title = $"{fileName}{modified} - Aurea";
        }

        private bool ConfirmSave()
        {
            var result = MessageBox.Show(
                "Voulez-vous enregistrer les modifications ?",
                "Aurea",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                return string.IsNullOrEmpty(currentFilePath)
                    ? SaveAs()
                    : SaveDocument();
            }
            return result != MessageBoxResult.Cancel;
        }

        private void DocumentEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            isModified = true;
            UpdateTitle();
            UpdateWordCount();
        }

        private void DocumentEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Vous pouvez ajouter la mise à jour de la position du curseur ici
        }

        private void UpdateWordCount()
        {
            string text = documentService.GetDocumentText(DocumentEditor);
            int wordCount = text.Split(new[] { ' ', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries).Length;
            WordCount.Text = $"{wordCount} mots";
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (isModified && !ConfirmSave())
            {
                e.Cancel = true;
                return;
            }

            StopCameraPanel();
            voiceService?.Dispose();
            base.OnClosing(e);
        }
    }
}
