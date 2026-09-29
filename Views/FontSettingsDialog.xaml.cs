using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Windows.FontStyle;
using MessageBox = System.Windows.MessageBox;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Reflection;

namespace Aurea.Views
{
    /// <summary>
    /// Logique d'interaction pour FontSettingsDialog.xaml
    /// </summary>
    public partial class FontSettingsDialog : Window
    {
        // Propriétés sélectionnées exposées
        public FontFamily SelectedFontFamily { get; private set; } = new FontFamily("Segoe UI");
        public double SelectedFontSize { get; private set; }
        public FontWeight SelectedFontWeight { get; private set; } = FontWeights.Normal;
        public FontStyle SelectedFontStyle { get; private set; } = FontStyles.Normal;
        public TextDecorationCollection SelectedTextDecorations { get; private set; } = new TextDecorationCollection();

        // Propriétés pour liaison de données
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }

        public FontSettingsDialog()
        {
            // Charger le XAML explicitement pour éviter l'ambiguïté avec InitializeComponent()
            try
            {
                var asmName = Assembly.GetAssembly(typeof(FontSettingsDialog))?.GetName().Name ?? "Aurea";
                Application.LoadComponent(this, new Uri($"/{asmName};component/Views/FontSettingsDialog.xaml", UriKind.Relative));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement du XAML: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                // On continue : certaines opérations suivantes peuvent échouer si le XAML n'a pas été chargé.
            }

            LoadFonts();
            InitializePreview();

            // Liaison de données pour les CheckBox via FindName pour éviter l'ambiguïté de noms
            var boldCheck = FindControl<CheckBox>("BoldCheckBox");
            var italicCheck = FindControl<CheckBox>("ItalicCheckBox");
            var underlineCheck = FindControl<CheckBox>("UnderlineCheckBox");

            if (boldCheck != null) boldCheck.DataContext = this;
            if (italicCheck != null) italicCheck.DataContext = this;
            if (underlineCheck != null) underlineCheck.DataContext = this;
        }

        public FontSettingsDialog(FontFamily currentFont, double currentSize,
                                FontWeight currentWeight, FontStyle currentStyle,
                                TextDecorationCollection currentDecorations) : this()
        {
            // Initialiser avec les valeurs courantes via les contrôles nommés récupérés par FindName
            var fontFamilySelector = FindControl<Selector>("FontFamilyList");
            var fontSizeSelector = FindControl<Selector>("FontSizeList");

            if (currentFont != null && fontFamilySelector != null)
                fontFamilySelector.SelectedItem = currentFont;

            if (currentSize > 0 && fontSizeSelector != null)
                fontSizeSelector.SelectedItem = currentSize;

            IsBold = currentWeight == FontWeights.Bold;
            IsItalic = currentStyle == FontStyles.Italic;
            IsUnderline = currentDecorations != null && currentDecorations.Count > 0;

            UpdatePreview();
        }

        private void LoadFonts()
        {
            try
            {
                // Charger les polices système
                var fonts = Fonts.SystemFontFamilies
                    .OrderBy(f => f.Source)
                    .ToList();

                         var fontFamilySelector = FindControl<Selector>("FontFamilyList");
                var fontSizeSelector = FindControl<Selector>("FontSizeList");

                if (fontFamilySelector != null)
                {
                    fontFamilySelector.ItemsSource = fonts;
                    var defaultFont = fonts.FirstOrDefault(f => f.Source.Contains("Segoe UI")) ?? fonts.FirstOrDefault();
                    if (defaultFont != null && fontFamilySelector.SelectedItem == null)
                        fontFamilySelector.SelectedItem = defaultFont;
                }

                // Tailles de police prédéfinies
                double[] fontSizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 28, 32, 36, 48, 72 };
                if (fontSizeSelector != null)
                {
                    fontSizeSelector.ItemsSource = fontSizes;
                    if (fontSizeSelector.SelectedItem == null)
                        fontSizeSelector.SelectedItem = 12.0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des polices: {ex.Message}",
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializePreview()
        {
            var preview = FindControl<TextBlock>("PreviewText");
            if (preview == null) return;

            preview.FontFamily = new FontFamily("Segoe UI");
            preview.FontSize = 16;
            preview.FontWeight = FontWeights.Normal;
            preview.FontStyle = FontStyles.Normal;
            preview.TextDecorations = new TextDecorationCollection();
        }

        private void UpdatePreview()
        {
            var preview = FindControl<TextBlock>("PreviewText");
            if (preview == null) return;

            var fontFamilySelector = FindControl<Selector>("FontFamilyList");
            var fontSizeSelector = FindControl<Selector>("FontSizeList");

            if (fontFamilySelector?.SelectedItem is FontFamily selectedFont)
                preview.FontFamily = selectedFont;

            if (fontSizeSelector?.SelectedItem is double size)
                preview.FontSize = size;

            preview.FontWeight = IsBold ? FontWeights.Bold : FontWeights.Normal;
            preview.FontStyle = IsItalic ? FontStyles.Italic : FontStyles.Normal;
            preview.TextDecorations = IsUnderline ? TextDecorations.Underline : new TextDecorationCollection();
        }

        private void FontFamilyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void FontSizeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void StyleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            // Ne pas fermer la fenêtre, permet d'appliquer et continuer
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveSettings())
            {
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool SaveSettings()
        {
            var fontFamilySelector = FindControl<Selector>("FontFamilyList");
            var fontSizeSelector = FindControl<Selector>("FontSizeList");

            if (fontFamilySelector == null || fontSizeSelector == null ||
                fontFamilySelector.SelectedItem == null || fontSizeSelector.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner une police et une taille.",
                              "Paramètres incomplets",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            SelectedFontFamily = (FontFamily)fontFamilySelector.SelectedItem;
            SelectedFontSize = (double)fontSizeSelector.SelectedItem;
            SelectedFontWeight = IsBold ? FontWeights.Bold : FontWeights.Normal;
            SelectedFontStyle = IsItalic ? FontStyles.Italic : FontStyles.Normal;
            SelectedTextDecorations = IsUnderline ? TextDecorations.Underline : new TextDecorationCollection();

            return true;
        }

        // Méthode statique pour faciliter l'appel
        public static bool ShowDialog(FontFamily currentFont, double currentSize,
                                     FontWeight currentWeight, FontStyle currentStyle,
                                     TextDecorationCollection currentDecorations,
                                     out FontFamily selectedFont, out double selectedSize,
                                     out FontWeight selectedWeight, out FontStyle selectedStyle,
                                     out TextDecorationCollection selectedDecorations)
        {
            var dialog = new FontSettingsDialog(currentFont, currentSize,
                                               currentWeight, currentStyle,
                                               currentDecorations);

            bool result = dialog.ShowDialog() == true;

            selectedFont = dialog.SelectedFontFamily;
            selectedSize = dialog.SelectedFontSize;
            selectedWeight = dialog.SelectedFontWeight;
            selectedStyle = dialog.SelectedFontStyle;
            selectedDecorations = dialog.SelectedTextDecorations;

            return result;
        }

        // Méthode utilitaire pour récupérer en toute sécurité les contrôles nommés du XAML
        private T? FindControl<T>(string name) where T : class
        {
            try
            {
                return this.FindName(name) as T;
            }
            catch
            {
                return null;
            }
        }
    }
}