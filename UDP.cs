using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualBasic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Windows.Threading;
using System.Collections.Generic;

namespace UNO
{
    public class UDP
    {
        public string name = "";
        public MainWindow window;

        public UdpClient udpResponse = new UdpClient(42424, AddressFamily.InterNetwork);

        public String getName()
        {
            String name = "";
            while (name.Length == 0)
                name = Interaction.InputBox("What is your name?").Trim();
            return name;
        }

        public String getIP()
        {
            String IP = "";
            while (IP.Length == 0)
                IP = Interaction.InputBox("What is the IP?").Trim();
            return IP;
        }

        public void SendMessage(object message)
        {
            // send via UDP
            UdpClient client = new UdpClient(24242, AddressFamily.InterNetwork);
            client.EnableBroadcast = true;

            IPEndPoint groupEp = new IPEndPoint(IPAddress.Broadcast, 42424);
            client.Connect(groupEp);

            var data = Encoding.ASCII.GetBytes(message.ToString());
            client.Send(data, data.Length);
            Thread.Sleep(25);
            client.Send(data, data.Length);
            Thread.Sleep(25);
            client.Send(data, data.Length);
            Thread.Sleep(25);
            client.Send(data, data.Length);

            client.Close();
        }

        public void ReceiveMessage()
        {
            try
            {
                while (true)
                {
                    IPEndPoint recvEp = new IPEndPoint(IPAddress.Any, 0);

                    var text = Encoding.ASCII.GetString(udpResponse.Receive(ref recvEp));

                    var message = new Message(text);

                }
            }
            catch (Exception ex)
            {
                // did we force close the connection?
                if (ex.Message != "A blocking operation was interrupted by a call to WSACancelBlockingCall")
                    throw ex;
            }
        }

        private void EndReceive(object sender, CancelEventArgs e)
        {
            udpResponse.Close();
        }

        public void BroadcastHost()
        {
            var counter = 0;

            while (window.currentScreen == window.lobby)
            {
                if (counter == 0)
                    SendMessage(new Message { HostID = window.UserID, Action = "hosting", PlayerName = window.lobby.clientName, PlayerList = Shared.Strip(window.playerList) });

                counter = (counter + 1) % 50;

                Thread.Sleep(100);
            }
        }

        public void ListenForClients()
        {
            try
            {
                while (window.currentScreen == window.lobby)
                {
                    IPEndPoint recvEp = new IPEndPoint(IPAddress.Any, 0);

                    var text = Encoding.ASCII.GetString(udpResponse.Receive(ref recvEp));

                    var message = new Message(text);

                    // if the message is from a client talking to us
                    if (message.HostID == window.UserID)
                    {
                        if (message.Action == "join")
                            SendMessage(new Message { HostID = window.UserID, Action = "joinAck", PlayerID = message.PlayerID, PlayerName = message.PlayerName, PlayerList = Shared.Strip(window.playerList) });

                        if (message.Action == "joinAckAck")
                            Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
                            {
                                window.lobby.addClient(message);
                            }));
                    }
                }
            }
            catch (Exception ex)
            {
                // did we force close the connection?
                if (ex.Message != "A blocking operation was interrupted by a call to WSACancelBlockingCall")
                    throw ex;
            }
        }

        public void ListenForHosts()
        {
            try
            {
                while (window.currentScreen == window.lobby)
                {
                    IPEndPoint recvEp = new IPEndPoint(IPAddress.Any, 0);

                    var text = Encoding.ASCII.GetString(udpResponse.Receive(ref recvEp));

                    var message = new Message(text);

                    if (message.Action.Equals("hosting") && !window.lobby.hosts.ContainsKey(message.HostID))
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            if (!window.lobby.hosts.ContainsKey(message.HostID))
                            {
                                window.lobby.hosts.Add(message.HostID, message.PlayerName);
                                window.lobby.reloadHostList();
                            }
                        }));
                    }
                    if (message.Action.Equals("joinAck") && window.lobby.hosts.ContainsKey(message.HostID) && message.PlayerID == window.UserID)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            message = new Message { HostID = message.HostID, PlayerID = window.UserID, Action = "joinAckAck", PlayerName = window.lobby.clientName, Extra = recvEp.Address.ToString() };

                            SendMessage(message);
                            window.lobby.UnloadClient();
                            window.lobby.HostID = message.HostID;
                            window.playerList = message.PlayerList;

                            Player client = new Player(message.PlayerName);
                            client.isComputer = false;
                            client.IP = message.Extra;
                            client.ID = message.PlayerID;
                            window.playerList.Add(client);

                            window.lobby.LoadWaiting();
                        }));
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
                // did we force close the connection?
                if (ex.Message != "A blocking operation was interrupted by a call to WSACancelBlockingCall")
                    throw ex;
            }
        }

        public void ListenForPlayerList()
        {
            try
            {
                while (window.currentScreen == window.lobby)
                {
                    IPEndPoint recvEp = new IPEndPoint(IPAddress.Any, 0);

                    var text = Encoding.ASCII.GetString(udpResponse.Receive(ref recvEp));

                    var message = new Message(text);

                    if (window.lobby.HostID == message.HostID)
                    {
                        if (message.Action == "hosting")
                            Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
                            {
                                window.playerList = message.PlayerList;
                                window.lobby.reloadPlayerList(false);
                            }));
                        else if (message.Action == "begin")
                        {
                            Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
                            {
                                window.lobby.UnloadHost();

                                window.StartGame(message);
                            }));

                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // did we force close the connection?
                if (ex.Message != "A blocking operation was interrupted by a call to WSACancelBlockingCall")
                    throw ex;
            }
        }

        public void ListenForMove()
        {
            try
            {
                IPEndPoint recvEp = new IPEndPoint(IPAddress.Any, 0);

                var text = Encoding.ASCII.GetString(udpResponse.Receive(ref recvEp));

                var message = new Message(text);

                Application.Current.Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    window.game.Process(message);
                }));
            }
            catch (Exception ex)
            {
                // did we force close the connection?
                if (ex.Message != "A blocking operation was interrupted by a call to WSACancelBlockingCall")
                    throw ex;
            }
        }
    }

    public class Message
    {
        public string HostID { get; set; }
        public string PlayerID { get; set; }
        public string Action { get; set; }
        public string Extra { get; set; }
        public string PlayerName { get; set; }
        public List<Player> PlayerList { get; set; }
        public Card Card { get; set; }
        public string TurnCount { get; set; }

        public Message()
        {

        }

        public Message(string data)
        {
            Console.WriteLine(data);

            var message = JsonConvert.DeserializeObject<Message>(data);

            HostID = message.HostID;
            PlayerID = message.PlayerID;
            Action = message.Action;
            PlayerName = message.PlayerName;
            PlayerList = message.PlayerList;
            Card = message.Card;
            Extra = message.Extra;
            TurnCount = message.TurnCount;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }).Replace(@"""color"":0,""value"":0,", "");
        }
    }
}
