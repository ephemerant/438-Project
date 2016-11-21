using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UNO
{
    public partial class Lobby : Screen
    {
        //------------------------------
        // Variables
        //------------------------------

        MainWindow window;

        //------------------------------
        // Functions
        //------------------------------

        public void Unload(bool isHost)
        {
            if (isHost)
                UnloadHost();
            else
                UnloadClient();
        }

        public void UnloadHost()
        {
            window.hostingPlayerList.Visibility = Visibility.Hidden;
            if (window.menuButtons.Count != 0)
            {
                for (int i = window.menuButtons.Count - 1; i >= 0; i--)
                {
                    window.canvas.Children.Remove(window.menuButtons[i]);
                    window.menuButtons.RemoveAt(i);
                }
            }
        }

        public void UnloadClient()
        {
            window.hostingPlayerList.Visibility = Visibility.Hidden;
            if (window.menuButtons.Count != 0)
            {
                for (int i = window.menuButtons.Count - 1; i >= 0; i--)
                {
                    window.canvas.Children.Remove(window.menuButtons[i]);
                    window.menuButtons.RemoveAt(i);
                }
            }
        }

        public void Load(MainWindow window, bool isHost)
        {
            this.window = window;

            if (isHost)
                LoadHost();
            else
                LoadClient();
        }

        public void LoadHost()
        {
            window.hostingPlayerList.Visibility = Visibility.Visible;

            //load Add Hosting Label
            var hostingLabel = Shared.LoadImage(Path.Combine(window.resourcesPath, "hostingLobby.png"), 548, 112);
            Canvas.SetTop(hostingLabel, -25);
            Canvas.SetLeft(hostingLabel, 100);
            window.canvas.Children.Add(hostingLabel);
            window.menuButtons.Add(hostingLabel);

            //load Add Local Player button
            var localPlayerButton = Shared.LoadImage(Path.Combine(window.resourcesPath, "addLocalPlayer.png"), 224, 42);
            Canvas.SetTop(localPlayerButton, 370);
            Canvas.SetLeft(localPlayerButton, 100);
            window.canvas.Children.Add(localPlayerButton);
            window.menuButtons.Add(localPlayerButton);

            localPlayerButton.MouseLeftButtonUp += localPlayerButtonClick;
            localPlayerButton.MouseEnter += ButtonBeginHover;
            localPlayerButton.MouseLeave += ButtonEndHover;

            //load Add Computer button
            var computerPlayerButton = Shared.LoadImage(Path.Combine(window.resourcesPath, "addComputer.png"), 195, 42);
            Canvas.SetTop(computerPlayerButton, 370);
            Canvas.SetLeft(computerPlayerButton, 450);
            window.canvas.Children.Add(computerPlayerButton);
            window.menuButtons.Add(computerPlayerButton);

            computerPlayerButton.MouseLeftButtonUp += computerPlayerButtonClick;
            computerPlayerButton.MouseEnter += ButtonBeginHover;
            computerPlayerButton.MouseLeave += ButtonEndHover;

            //load Add Return to menu button
            var returnToMenuButton = Shared.LoadImage(Path.Combine(window.resourcesPath, "returnMenu.png"), 212, 40);
            Canvas.SetTop(returnToMenuButton, 450);
            Canvas.SetLeft(returnToMenuButton, 250);
            window.canvas.Children.Add(returnToMenuButton);
            window.menuButtons.Add(returnToMenuButton);

            returnToMenuButton.MouseLeftButtonUp += hostReturnToMenuButtonClick;
            returnToMenuButton.MouseEnter += ButtonBeginHover;
            returnToMenuButton.MouseLeave += ButtonEndHover;

            //load Add Play button
            var playButton = Shared.LoadImage(Path.Combine(window.resourcesPath, "play.png"), 79, 42);
            Canvas.SetTop(playButton, 100);
            Canvas.SetLeft(playButton, 700);
            window.canvas.Children.Add(playButton);
            window.menuButtons.Add(playButton);

            playButton.MouseLeftButtonUp += hostPlayButtonClick;
            playButton.MouseEnter += ButtonBeginHover;
            playButton.MouseLeave += ButtonEndHover;
        }

        public void LoadClient()
        {
            window.hostingPlayerList.Visibility = Visibility.Visible;

            //load Add Hosting Label
            var joiningLabel = Shared.LoadImage(Path.Combine(window.resourcesPath, "joiningLobby.png"), 528, 112);
            Canvas.SetTop(joiningLabel, -25);
            Canvas.SetLeft(joiningLabel, 116);
            window.canvas.Children.Add(joiningLabel);
            window.menuButtons.Add(joiningLabel);

            //load Add Return to menu button
            var returnToMenuButton = Shared.LoadImage(Path.Combine(window.resourcesPath, "returnMenu.png"), 212, 40);
            Canvas.SetTop(returnToMenuButton, 400);
            Canvas.SetLeft(returnToMenuButton, 250);
            window.canvas.Children.Add(returnToMenuButton);
            window.menuButtons.Add(returnToMenuButton);

            returnToMenuButton.MouseLeftButtonUp += joinReturnToMenuButtonClick;
            returnToMenuButton.MouseEnter += ButtonBeginHover;
            returnToMenuButton.MouseLeave += ButtonEndHover;


            //load Add Play button
            var playButton = Shared.LoadImage(Path.Combine(window.resourcesPath, "play.png"), 79, 42);
            Canvas.SetTop(playButton, 100);
            Canvas.SetLeft(playButton, 700);
            window.canvas.Children.Add(playButton);
            window.menuButtons.Add(playButton);

            playButton.MouseLeftButtonUp += joinPlayButtonClick;
            playButton.MouseEnter += ButtonBeginHover;
            playButton.MouseLeave += ButtonEndHover;
        }

        //add computer player
        private void computerPlayerButtonClick(object sender, MouseEventArgs e)
        {

        }

        //add local player
        private void localPlayerButtonClick(object sender, MouseEventArgs e)
        {

        }

        //return to menu from host
        private void joinReturnToMenuButtonClick(object sender, MouseEventArgs e)
        {
            window.unloadJoinScreen();
            window.Window_Loaded(null, null);
        }

        //return to menu from host
        private void hostReturnToMenuButtonClick(object sender, MouseEventArgs e)
        {
            window.unloadHostScreen();
            window.Window_Loaded(null, null);
        }

        private void joinPlayButtonClick(object sender, MouseEventArgs e)
        {
            window.unloadJoinScreen();
            window.StartMainScreen();
        }

        private void hostPlayButtonClick(object sender, MouseEventArgs e)
        {
            window.unloadHostScreen();
            window.StartMainScreen();
        }
    }
}
