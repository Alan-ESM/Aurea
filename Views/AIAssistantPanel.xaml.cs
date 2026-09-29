using System;
using System.Windows.Controls;                // ← WPF UserControl
using System.Windows;                        // ← WPF Window, MessageBox
using System.Windows.Input;                  // ← WPF KeyEventArgs
using System.Windows.Media;                   // ← WPF Brushes, Color
using Aurea.Services;
using UserControl = System.Windows.Controls.UserControl;

namespace Aurea.Views
{
    public partial class AIAssistantPanel : UserControl
    {
        private readonly AIService aiService;

        // Constructeur avec 2 paramètres (comme tu l'utilises dans MainWindow)
        public AIAssistantPanel(AIService service, System.Windows.Controls.RichTextBox editor)
        {
            InitializeComponent();
            aiService = service;
            AddWelcomeMessage();
        }

        private void AddWelcomeMessage()
        {
            AddMessage("Assistant IA", "Bonjour ! Je suis votre assistant Aurea. Comment puis-je vous aider ?", false);
        }

        private async void Send_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string message = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            AddMessage("Vous", message, true);
            InputBox.Clear();

            try
            {
                string response = await aiService.GetResponse(message);
                AddMessage("Assistant IA", response, false);
            }
            catch (Exception ex)
            {
                AddMessage("Système", $"Erreur : {ex.Message}", false);
            }
        }

        private void AddMessage(string sender, string content, bool isUser)
        {
            var border = new Border
            {
                Background = isUser
                    ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(108, 63, 181))
                    : new SolidColorBrush(System.Windows.Media.Color.FromRgb(45, 45, 68)),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(14),
                Margin = new Thickness(12, 8, 12, 8),
                HorizontalAlignment = isUser ? System.Windows.HorizontalAlignment.Right : System.Windows.HorizontalAlignment.Left,
                MaxWidth = 520,
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = sender,
                            Foreground = System.Windows.Media.Brushes.White,
                            FontWeight = FontWeights.SemiBold,
                            FontSize = 12,
                            Margin = new Thickness(0, 0, 0, 6)
                        },
                        new TextBlock
                        {
                            Text = content,
                            Foreground = System.Windows.Media.Brushes.White,
                            TextWrapping = TextWrapping.Wrap,
                            FontSize = 14.5,
                            LineHeight = 22
                        }
                    }
                }
            };

            ChatHistory.Items.Add(border);
            ChatScrollViewer.ScrollToEnd();
        }

        private void VoiceInput_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Saisie vocale en développement", "Aurea",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void InputBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                Send_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}