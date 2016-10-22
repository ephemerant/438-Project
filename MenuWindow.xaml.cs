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

        List<Image> menuButtons = new List<Image>();

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

            //load host button
            var hostButton = Shared.LoadImage(Path.Combine(resourcesPath, "hostGame.png"), 395, 81);
            Canvas.SetTop(hostButton, 160);
            Canvas.SetLeft(hostButton, 200);
            canvas.Children.Add(hostButton);
            menuButtons.Add(hostButton);

            hostButton.MouseLeftButtonDown += hostButtonClick;

            //load join button
            var joinButton = Shared.LoadImage(Path.Combine(resourcesPath, "joinGame.png"), 395, 85);
            Canvas.SetTop(joinButton, 250);
            Canvas.SetLeft(joinButton, 200);
            canvas.Children.Add(joinButton);
            menuButtons.Add(joinButton);

            joinButton.MouseLeftButtonDown += joinButtonClick;

            //load quit button
            var quitButton = Shared.LoadImage(Path.Combine(resourcesPath, "quit.png"),166, 87);
            Canvas.SetTop(quitButton, 340);
            Canvas.SetLeft(quitButton, 200);
            canvas.Children.Add(quitButton);
            menuButtons.Add(quitButton);

            quitButton.MouseLeftButtonDown += quitButtonClick;

        }

        void CanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
                 
        }

        void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void hostButtonClick(object sender, MouseEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }

        private void joinButtonClick(object sender, MouseEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }

        private void quitButtonClick(object sender, MouseEventArgs e)
        {
            Close();
        }

    }
}
