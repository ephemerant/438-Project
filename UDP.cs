using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualBasic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.ComponentModel;


namespace UNO
{
    public class UDP
    {
        public string name = "";

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

        public void SendMessage(string message)
        {
            // send via UDP
            UdpClient client = new UdpClient(24242, AddressFamily.InterNetwork);
            client.EnableBroadcast = true;

            IPEndPoint groupEp = new IPEndPoint(IPAddress.Broadcast, 42424);
            client.Connect(groupEp);

            var data = Encoding.ASCII.GetBytes(message);

            client.Send(data, data.Length);
            client.Close();
        }

        //alternative sendMessage with IP address
        public void SendMessage(string message, string inputIP)
        {
            // send via UDP
            UdpClient client = new UdpClient(24242, AddressFamily.InterNetwork);
            client.EnableBroadcast = true;

            IPEndPoint groupEp = new IPEndPoint(IPAddress.Parse(inputIP), 42424);
            client.Connect(groupEp);

            var data = Encoding.ASCII.GetBytes(message);

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

    }
    public class Message
    {
        public string Name { get; set; }
        public string IP { get; set; }

        public Message()
        {

        }

        public Message(string data)
        {
            var values = data.Split('|');

            Name = values[0];
            IP = values[1];
        }

        public override string ToString()
        {
            return string.Format("{0}|{1}", Name, IP);
        }
    }
}
