using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicManager.Logic;

class Color {
    public static Brush AsSolidColorBrush(string colorName) {
        var color = System.Drawing.Color.FromName(colorName);
        return new SolidColorBrush(Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B));
    }
}