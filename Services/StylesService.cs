using System;
using System.Collections.Generic;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfColor = System.Windows.Media.Color;
using WpfColors = System.Windows.Media.Colors;
using WpfSolidColorBrush = System.Windows.Media.SolidColorBrush;
using WpfRichTextBox = System.Windows.Controls.RichTextBox;
using System.Windows;
using System.Windows.Documents;

namespace Aurea.Services
{
    public class DocumentStyle
    {
        public string Name { get; set; }
        public WpfFontFamily FontFamily { get; set; }
        public double FontSize { get; set; }
        public FontWeight FontWeight { get; set; }
        public WpfColor ForegroundColor { get; set; }
        public WpfColor BackgroundColor { get; set; }
        public TextAlignment TextAlignment { get; set; }
        public double LineHeight { get; set; }
    }

    public class StylesService
    {
        public Dictionary<string, DocumentStyle> Styles { get; set; }

        public StylesService()
        {
            Styles = new Dictionary<string, DocumentStyle>();
            LoadDefaultStyles();
        }

        private void LoadDefaultStyles()
        {
            Styles["Titre1"] = new DocumentStyle
            {
                Name = "Titre 1",
                FontFamily = new WpfFontFamily("Arial"),
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                ForegroundColor = WpfColors.Black,
                BackgroundColor = WpfColors.Transparent,
                TextAlignment = TextAlignment.Left,
                LineHeight = 1.5
            };

            Styles["Titre2"] = new DocumentStyle
            {
                Name = "Titre 2",
                FontFamily = new WpfFontFamily("Arial"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                ForegroundColor = WpfColors.DarkBlue,
                BackgroundColor = WpfColors.Transparent,
                TextAlignment = TextAlignment.Left,
                LineHeight = 1.4
            };

            Styles["Normal"] = new DocumentStyle
            {
                Name = "Normal",
                FontFamily = new WpfFontFamily("Segoe UI"),
                FontSize = 12,
                FontWeight = FontWeights.Normal,
                ForegroundColor = WpfColors.Black,
                BackgroundColor = WpfColors.Transparent,
                TextAlignment = TextAlignment.Left,
                LineHeight = 1.2
            };

            Styles["Citation"] = new DocumentStyle
            {
                Name = "Citation",
                FontFamily = new WpfFontFamily("Georgia"),
                FontSize = 11,
                FontWeight = FontWeights.Normal,
                ForegroundColor = WpfColors.Gray,
                BackgroundColor = WpfColor.FromArgb(255, 245, 245, 245),
                TextAlignment = TextAlignment.Left,
                LineHeight = 1.3
            };

            Styles["Code"] = new DocumentStyle
            {
                Name = "Code",
                FontFamily = new WpfFontFamily("Consolas"),
                FontSize = 10,
                FontWeight = FontWeights.Normal,
                ForegroundColor = WpfColors.DarkGreen,
                BackgroundColor = WpfColor.FromArgb(255, 240, 240, 240),
                TextAlignment = TextAlignment.Left,
                LineHeight = 1.1
            };
        }

        public void ApplyStyle(WpfRichTextBox editor, string styleName)
        {
            if (!Styles.ContainsKey(styleName))
            {
                throw new ArgumentException($"Style '{styleName}' introuvable");
            }

            var style = Styles[styleName];
            var selection = editor.Selection;

            if (!selection.IsEmpty)
            {
                selection.ApplyPropertyValue(TextElement.FontFamilyProperty, style.FontFamily);
                selection.ApplyPropertyValue(TextElement.FontSizeProperty, style.FontSize);
                selection.ApplyPropertyValue(TextElement.FontWeightProperty, style.FontWeight);
                selection.ApplyPropertyValue(TextElement.ForegroundProperty, new WpfSolidColorBrush(style.ForegroundColor));
                selection.ApplyPropertyValue(TextElement.BackgroundProperty, new WpfSolidColorBrush(style.BackgroundColor));
                selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, style.TextAlignment);
                selection.ApplyPropertyValue(Paragraph.LineHeightProperty, style.LineHeight);
            }
            else
            {
                editor.FontFamily = style.FontFamily;
                editor.FontSize = style.FontSize;
                editor.FontWeight = style.FontWeight;
                editor.Foreground = new WpfSolidColorBrush(style.ForegroundColor);
                editor.Background = new WpfSolidColorBrush(style.BackgroundColor);
            }
        }

        public void CreateCustomStyle(string name, DocumentStyle style)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Le nom du style ne peut pas être vide");
            }

            if (Styles.ContainsKey(name))
            {
                throw new ArgumentException($"Un style nommé '{name}' existe déjà");
            }

            style.Name = name;
            Styles[name] = style;
        }

        public void UpdateStyle(string name, DocumentStyle newStyle)
        {
            if (!Styles.ContainsKey(name))
            {
                throw new ArgumentException($"Style '{name}' introuvable");
            }

            newStyle.Name = name;
            Styles[name] = newStyle;
        }

        public void DeleteStyle(string name)
        {
            if (Styles.ContainsKey(name))
            {
                Styles.Remove(name);
            }
        }

        public List<string> GetStyleNames()
        {
            return new List<string>(Styles.Keys);
        }

        public DocumentStyle GetStyle(string name)
        {
            return Styles.ContainsKey(name) ? Styles[name] : null;
        }
    }
}