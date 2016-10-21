using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;

namespace UNO
{
    public partial class MenuWindow : Window
    {
        string resourcesPath;
        string imagesPath;

        public MenuWindow()
        {
            InitializeComponent();
        }
        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // First path is for general distribution, second is for when debugging
            string[] possibleDirectories = { @"resources", @"..\..\resources" };

            foreach (var dir in possibleDirectories)
                if (Directory.Exists(dir))
                {
                    resourcesPath = Path.GetFullPath(dir);
                    imagesPath = Path.Combine(resourcesPath, "cards");
                    break;
                }

        }
    }
}
