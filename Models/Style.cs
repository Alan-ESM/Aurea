using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Aurea.Models
{
    public class Style
    {
        public string Name { get; set; }
        public FontFamily FontFamily { get; set; }
        public double FontSize { get; set; }
        public FontWeight FontWeight { get; set; }
        public Color ForegroundColor { get; set; }
        public Color BackgroundColor { get; set; }
    }
}