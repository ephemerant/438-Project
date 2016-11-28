using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net;
using System.Net.Sockets;
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

        public String clientName;
        MainWindow window;
        int comcount;
        public Dictionary<string, string> hosts = new Dictionary<string, string>();
        public string HostID;

        //------------------------------
        // Functions
        //------------------------------

        public void Unload(bool isHost)
        {
            window.canvas.Children.Clear();
            if (isHost)
                UnloadHost();
            else
                UnloadClient();
        }

        public void UnloadHost()
        {
            window.hostingPlayerList.Visibility = Visibility.Hidden;
            window.hostingPlayerList.Children.Clear();
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
            hosts.Clear();

            window.hostingPlayerList.Visibility = Visibility.Hidden;
            window.hostingPlayerList.Children.Clear();
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
            comcount = 0;

            if (isHost)
                LoadHost();
            else
                LoadClient();
        }

        public void LoadHost()
        {
            HostID = window.UserID;

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

            for (int x = 0; x < 10; x++)
            {
                var playerNumber = new Label { Content = (x + 1) + ".", Foreground = Brushes.White, FontSize = 20 };
                Canvas.SetTop(playerNumber, 12 + (20 * x));
                Canvas.SetLeft(playerNumber, 16);
                window.hostingPlayerList.Children.Add(playerNumber);
            }

            clientName = window.udpConnect.getName();

            window.playerList.Add(new Player(clientName));
            window.playerList[0].isComputer = false;
            window.playerList[0].ID = window.UserID;

            var labelName = new Label { Content = window.playerList[0].name, Foreground = Brushes.Orange, FontSize = 20 };

            Canvas.SetTop(labelName, 12);
            Canvas.SetLeft(labelName, 50);

            window.hostingPlayerList.Children.Add(labelName);
            window.playerList[0].labelName = labelName;

            reloadPlayerList(true);

            // broadcast that we're hosting
            var threadBroadcast = new Thread(new ThreadStart(window.udpConnect.BroadcastHost));
            threadBroadcast.Start();

            // listen for clients wanting to join
            var threadListen = new Thread(new ThreadStart(window.udpConnect.ListenForClients));
            threadListen.Start();
        }

        // Waiting room for the client after it has joined a host
        public void LoadWaiting()
        {
            window.hostingPlayerList.Visibility = Visibility.Visible;

            //load Add Hosting Label
            var hostingLabel = Shared.LoadImage(Path.Combine(window.resourcesPath, "hostingLobby.png"), 548, 112);
            Canvas.SetTop(hostingLabel, -25);
            Canvas.SetLeft(hostingLabel, 100);
            window.canvas.Children.Add(hostingLabel);
            window.menuButtons.Add(hostingLabel);

            //load Add Return to menu button (TODO: Client can dump host)
            var returnToMenuButton = Shared.LoadImage(Path.Combine(window.resourcesPath, "returnMenu.png"), 212, 40);
            Canvas.SetTop(returnToMenuButton, 450);
            Canvas.SetLeft(returnToMenuButton, 250);
            window.canvas.Children.Add(returnToMenuButton);
            window.menuButtons.Add(returnToMenuButton);

            returnToMenuButton.MouseLeftButtonUp += hostReturnToMenuButtonClick;
            returnToMenuButton.MouseEnter += ButtonBeginHover;
            returnToMenuButton.MouseLeave += ButtonEndHover;
            
            for (int x = 0; x < 10; x++)
            {
                var playerNumber = new Label { Content = (x + 1) + ".", Foreground = Brushes.White, FontSize = 20 };
                Canvas.SetTop(playerNumber, 12 + (20 * x));
                Canvas.SetLeft(playerNumber, 16);
                window.hostingPlayerList.Children.Add(playerNumber);
            }
            
            reloadPlayerList(false);
            
            // listen for updates from the host
            var threadListen = new Thread(new ThreadStart(window.udpConnect.ListenForPlayerList));
            threadListen.Start();
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

            //load Connect to IP
            //var connectIP = Shared.LoadImage(Path.Combine(window.resourcesPath, "connectIP.png"), 198, 58);
            //Canvas.SetTop(connectIP, 400);
            //Canvas.SetLeft(connectIP, 520);
            //window.canvas.Children.Add(connectIP);
            //window.menuButtons.Add(connectIP);

            //connectIP.MouseLeftButtonUp += connectIPButtonClick;
            //connectIP.MouseEnter += ButtonBeginHover;
            //connectIP.MouseLeave += ButtonEndHover;
            clientName = window.udpConnect.getName();

            // listen for hosts
            var threadListen = new Thread(new ThreadStart(window.udpConnect.ListenForHosts));
            threadListen.Start();
        }

        //add computer player
        private void computerPlayerButtonClick(object sender, MouseEventArgs e)
        {
            if (window.playerList.Count < 10)
            {
                Player newPlayer = new Player("Computer " + (++comcount));
                newPlayer.isComputer = true;
                newPlayer.ID = window.UserID;

                window.playerList.Add(newPlayer);
                reloadPlayerList(true);
            }
        }

        //add local player
        private void localPlayerButtonClick(object sender, MouseEventArgs e)
        {
            if (window.playerList.Count < 10)
            {
                Player newPlayer = new Player(window.udpConnect.getName());
                newPlayer.isComputer = false;
                newPlayer.ID = window.UserID;

                window.playerList.Add(newPlayer);
                reloadPlayerList(true);
            }
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
        
        private void hostPlayButtonClick(object sender, MouseEventArgs e)
        {           
            window.unloadHostScreen();
            window.StartGame();
        }

        private void connectIPButtonClick(object sender, MouseEventArgs e)
        {
            //String inputIP = window.udpConnect.getIP();

            //window.udpConnect.SendMessage("a", inputIP);
        }

        private void deleteButtonClick(object sender, MouseEventArgs e)
        {
            if (e.Source != null)
            {
                var image = (Image)e.Source;
                window.playerList.RemoveAt((int)image.Tag);
                reloadPlayerList(true);
            }
        }

        public void reloadPlayerList(bool isHost)
        {
            window.hostingPlayerList.Children.Clear();

            //reloading the numbers each time isn't very efficient
            for (int x = 0; x < 10; x++)
            {
                var playerNumber = new Label { Content = (x + 1) + ".", Foreground = Brushes.White, FontSize = 20 };
                Canvas.SetTop(playerNumber, 12 + (20 * x));
                Canvas.SetLeft(playerNumber, 16);
                window.hostingPlayerList.Children.Add(playerNumber);
            }

            for (int x = 0; x < window.playerList.Count; x++)
            {
                Label thisplayer;
                if (window.playerList[x].isComputer == false && window.playerList[x].IP == null)
                {
                    thisplayer = new Label { Content = window.playerList[x].name, Foreground = Brushes.Orange, FontSize = 20 };
                }
                else if (window.playerList[x].isComputer == false && window.playerList[x].IP !=null)
                {
                    thisplayer = new Label { Content = window.playerList[x].name, Foreground = Brushes.LightGreen, FontSize = 20 };
                }
                else
                {
                    thisplayer = new Label { Content = window.playerList[x].name, Foreground = Brushes.LightBlue, FontSize = 20 };
                }

                //Label thisplayer = new Label { Content = window.playerList[x].name, Foreground = Brushes.White, FontSize = 20 };
                Canvas.SetTop(thisplayer, 12 + (20 * x));
                Canvas.SetLeft(thisplayer, 50);
                if (x > 0 && isHost)
                {
                    var delete = Shared.LoadImage(Path.Combine(window.resourcesPath, "delete.png"), 20, 20);
                    delete.Tag = x;
                    Canvas.SetTop(delete, 20 + (20 * x));
                    Canvas.SetLeft(delete, 400);
                    window.hostingPlayerList.Children.Add(delete);
                    delete.MouseLeftButtonUp += deleteButtonClick;
                }
                window.hostingPlayerList.Children.Add(thisplayer);
                window.playerList[x].labelName = thisplayer;
            }
        }

        public void hostingLabelClick(object sender, MouseEventArgs e)
        {
            if (e.Source != null)
            {
                var label = (Label)e.Source;

                var message = new Message { HostID = (String)label.Tag, Action = "join", PlayerID = window.UserID, PlayerName = clientName };

                window.udpConnect.SendMessage(message);

                window.lobby.UnloadClient();
                window.lobby.HostID = message.HostID;
                window.lobby.LoadWaiting();
            }
        }

        public void addClient(Message message)
        {
            foreach (var player in window.playerList)
                if (player.ID == message.PlayerID)
                    return;

            Player client = new Player(message.PlayerName);
            client.isComputer = false;
            client.IP = message.Extra;
            client.ID = message.PlayerID;
            window.playerList.Add(client);
            reloadPlayerList(true);
        }

        public void reloadHostList()
        {
            for(int x =0; x < hosts.Count; x++)
            {
                var playerNumber = new Label { Content = hosts.ElementAt(x).Value, Foreground = Brushes.White, FontSize = 20 };
                Canvas.SetTop(playerNumber, 12 + (20 * x));
                Canvas.SetLeft(playerNumber, 16);
                playerNumber.Tag = hosts.ElementAt(x).Key;
                window.hostingPlayerList.Children.Add(playerNumber);
                playerNumber.MouseLeftButtonUp += hostingLabelClick;
                //playerNumber.MouseEnter += ButtonBeginHover;
                //playerNumber.MouseLeave += ButtonEndHover;
            }
        }
    }
}