using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace UNO
{
    static class Shared
    {
        internal static Image LoadImage(string path, double w, double h)
        {           
            // Load the image
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(path);
            src.EndInit();

            return new Image { Source = src, Width = w, Height = h };
        }
    }
}
