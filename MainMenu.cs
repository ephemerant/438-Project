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
using System.Windows.Threading;

namespace UNO
{
    public partial class MainMenu : Screen
    {
        //------------------------------
        // Variables
        //------------------------------

        MainWindow window;

        //------------------------------
        // Functions
        //------------------------------

        public void Unload()
        {
            if (window.menuButtons.Count != 0)
            {
                for (int i = window.menuButtons.Count - 1; i >= 0; i--)
                {
                    window.canvas.Children.Remove(window.menuButtons[i]);
                    window.menuButtons.RemoveAt(i);
                }
            }
        }

        public void Load(MainWindow window)
        {
            this.window = window;

            foreach (var canvas in new Canvas[] { window.players, window.hand, window.inPlay, window.DrawDeck, window.turnDirection, window.turnDirectionReverse, window.hostingPlayerList })
                canvas.Visibility = Visibility.Hidden;

            // First path is for general distribution, second is for when debugging
            string[] possibleDirectories = { @"resources", @"..\..\resources" };

            foreach (var dir in possibleDirectories)
                if (Directory.Exists(dir))
                {
                    window.resourcesPath = Path.GetFullPath(dir);
                    window.imagesPath = Path.Combine(window.resourcesPath, "cards");
                    break;
                }

            //load host button
            var hostButton = Shared.LoadImage(Path.Combine(window.resourcesPath, "hostGame.png"), 395, 81);
            Canvas.SetTop(hostButton, 160);
            Canvas.SetLeft(hostButton, 200);
            window.canvas.Children.Add(hostButton);
            window.menuButtons.Add(hostButton);

            hostButton.MouseLeftButtonUp += hostButtonClick;
            hostButton.MouseEnter += ButtonBeginHover;
            hostButton.MouseLeave += ButtonEndHover;

            //load join button
            var joinButton = Shared.LoadImage(Path.Combine(window.resourcesPath, "joinGame.png"), 395, 85);
            Canvas.SetTop(joinButton, 250);
            Canvas.SetLeft(joinButton, 200);
            window.canvas.Children.Add(joinButton);
            window.menuButtons.Add(joinButton);

            joinButton.MouseLeftButtonUp += joinButtonClick;
            joinButton.MouseEnter += ButtonBeginHover;
            joinButton.MouseLeave += ButtonEndHover;

            //load quit button
            var quitButton = Shared.LoadImage(Path.Combine(window.resourcesPath, "quit.png"), 166, 87);
            Canvas.SetTop(quitButton, 340);
            Canvas.SetLeft(quitButton, 200);
            window.canvas.Children.Add(quitButton);
            window.menuButtons.Add(quitButton);

            quitButton.MouseLeftButtonUp += quitButtonClick;
            quitButton.MouseEnter += ButtonBeginHover;
            quitButton.MouseLeave += ButtonEndHover;
        }

        // Host a game
        private void hostButtonClick(object sender, MouseEventArgs e)
        {
            window.unloadMenuScreen();
            window.loadHostScreen();
        }

        // Join a game
        private void joinButtonClick(object sender, MouseEventArgs e)
        {
            window.unloadMenuScreen();
            window.loadJoinScreen();
        }

        private void quitButtonClick(object sender, MouseEventArgs e)
        {
            window.Close();
        }
    }
}
