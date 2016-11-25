using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UNO
{
    public class Network
    {

        #regionVariables 
        MainWindow window;
        //_____________________________________________________________________________________________
        //
        // Thread for client and server
        //_____________________________________________________________________________________________

        public Thread thread_receive_client;
        public Thread thread_receive_server;

        //_____________________________________________________________________________________________
        //
        // Server IP and port
        //_____________________________________________________________________________________________

        private string wServerIP;
        const int SERVERPORT = 12790;

        //_____________________________________________________________________________________________
        //
        // Loop control variables for client and server threads
        //_____________________________________________________________________________________________

        bool wReceivingServer = true;
        bool wReceivingClient = true;

        //_____________________________________________________________________________________________
        //
        // TCP e NetworkStream objects for client and server
        //_____________________________________________________________________________________________

        NetworkStream clientSockStream;
        NetworkStream serverSockStream;

        TcpClient tcpClient;
        TcpListener tcpListener;
        Socket soTcpServer;

        #endregion


        public void loadNetwork()
        {
            this.window = window;
        }
        #regionClient 

        public void ConnectServer(string pIP)
        {

            //_____________________________________________________________________________________________
            //
            // Connect to a game server
            //_____________________________________________________________________________________________

            wServerIP = pIP;
            byte[] buf = new byte[1];

            thread_receive_client = new Thread(new ThreadStart(ThreadReceivingClient));
            thread_receive_client.Start();

        }


        private void ThreadReceivingClient()
        {
            //_____________________________________________________________________________________________
            //
            // Thread for receiving packets from server
            //_____________________________________________________________________________________________

            try
            {

                byte[] buf = new byte[512];
                int bytesReceived = 0;

                tcpClient = new TcpClient(wServerIP, SERVERPORT);
                clientSockStream = tcpClient.GetStream();

                wReceivingClient = true;

                while (wReceivingClient)
                {

                    //_____________________________________________________________________________________________
                    //
                    // Thread is blocked until receives data
                    //_____________________________________________________________________________________________

                    try
                    {
                        bytesReceived = clientSockStream.Read(buf, 0, 2);
                    }
                    catch
                    {
                        return;
                    }

                    //_____________________________________________________________________________________________
                    //
                    // Processes network packet
                    //_____________________________________________________________________________________________

                    if (bytesReceived > 0)
                    {
                        //_____________________________________________________________________________________________
                        //
                        // Control packet for game restart
                        //_____________________________________________________________________________________________

                        if (buf[0] == byte.Parse(Asc("R").ToString()))
                        {
                            //objTicTacToe.RestartGame();
                            continue;
                        }

                        //_____________________________________________________________________________________________
                        //
                        // Packet indicating a game move
                        //_____________________________________________________________________________________________

                        int wRow = int.Parse(Convert.ToChar(buf[0]).ToString());
                        int wColumn = int.Parse(Convert.ToChar(buf[1]).ToString());

                        if ((wRow > 0 && wRow < 4) && (wColumn > 0 && wColumn < 4))
                        {
                            //objTicTacToe.wNetworkPlay = true;
                            //objTicTacToe.MakeMove(wRow, wColumn);
                        }

                    } //if (bytesReceived>0) 

                } //while (wReceivingClient)

            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                //objTicTacToe.mnDisconnect_Click(null, null);
                return;
            }
        }

        #endregion

        #regionServer 

        public void StartServer()
        {

            //_____________________________________________________________________________________________
            //
            // Starts game server
            //_____________________________________________________________________________________________

            thread_receive_server = new Thread(new ThreadStart(ThreadReceivingServer));
            thread_receive_server.Start();
        }


        private void ThreadReceivingServer()
        {
            //_____________________________________________________________________________________________
            //
            // Thread for receiving packets from client
            //_____________________________________________________________________________________________

            try
            {
                byte[] buf = new byte[512];
                IPHostEntry localHostEntry = Dns.GetHostByName(Dns.GetHostName());
                int bytesReceived = 0;

                tcpListener = new TcpListener(localHostEntry.AddressList[0], SERVERPORT);

                tcpListener.Start();

                //_____________________________________________________________________________________________
                //
                // Thread is blocked until it gets a connection from client
                //_____________________________________________________________________________________________

                soTcpServer = tcpListener.AcceptSocket();

                serverSockStream = new NetworkStream(soTcpServer);

                //objTicTacToe.RestartGame();
                //objTicTacToe.SetStatusMessage("Connected!");

                wReceivingServer = true;

                while (wReceivingServer)
                {

                    //_____________________________________________________________________________________________
                    //
                    // Thread is blocked until receives data
                    //_____________________________________________________________________________________________

                    try
                    {
                        bytesReceived = serverSockStream.Read(buf, 0, 2);
                    }
                    catch
                    {
                        return;
                    }

                    //_____________________________________________________________________________________________
                    //
                    // Processes network packet
                    //_____________________________________________________________________________________________

                    if (bytesReceived > 0)
                    {

                        //_____________________________________________________________________________________________
                        //
                        // Control packet for game restart
                        //_____________________________________________________________________________________________

                        if (buf[0] == byte.Parse(Asc("R").ToString()))
                        {
                            //objTicTacToe.RestartGame();
                            continue;
                        }

                        //_____________________________________________________________________________________________
                        //
                        // Packet indicating a game move
                        //_____________________________________________________________________________________________

                        int wRow = int.Parse(Convert.ToChar(buf[0]).ToString());
                        int wColumn = int.Parse(Convert.ToChar(buf[1]).ToString());

                        if ((wRow > 0 && wRow < 4) && (wColumn > 0 && wColumn < 4))
                        {
                            //objTicTacToe.wNetworkPlay = true;
                            //objTicTacToe.MakeMove(wRow, wColumn);
                        }

                    }   //if (bytesReceived>0) 

                }   //while (wReceivingServer)
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                //objTicTacToe.mnDisconnect_Click(null, null);
                return;
            }
        }

        #endregion

        #regionFunctions for sending packets/disconnect 

        public void SendPacketTCP(Byte[] pDados)
        {
            //_____________________________________________________________________________________________
            //
            // Sends a packet via TCP
            //_____________________________________________________________________________________________

            try
            {
                if (window.wClient == true)
                {
                    if (clientSockStream == null)
                        return;

                    if (clientSockStream.CanWrite)
                    {
                        clientSockStream.Write(pDados, 0, 2);
                        clientSockStream.Flush();
                    }
                }
                else
                {
                    if (serverSockStream == null)
                        return;

                    if (serverSockStream.CanWrite)
                    {
                        serverSockStream.Write(pDados, 0, 2);
                        serverSockStream.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                //objTicTacToe.mnDisconnect_Click(null, null);
                return;
            }

        }

        public void SendMove(int wRow, int wColumn)
        {
            //_____________________________________________________________________________________________
            //
            // Sends packet that shows move position
            //_____________________________________________________________________________________________

            byte[] buf = new byte[2];
            buf[0] = byte.Parse(Asc(wRow.ToString()).ToString());
            buf[1] = byte.Parse(Asc(wColumn.ToString()).ToString());

            SendPacketTCP(buf);

        }

        public void SendsRestartPacket()
        {
            //_____________________________________________________________________________________________
            //
            // Sends packet for the other game restart
            //_____________________________________________________________________________________________

            byte[] buf = new byte[2];
            buf[0] = byte.Parse(Asc("R").ToString());
            buf[1] = 0;

            SendPacketTCP(buf);

        }

        public void Disconnect()
        {
            //_____________________________________________________________________________________________
            //
            // Disconnect client and server
            //_____________________________________________________________________________________________

            if (window.wClient == true)
            {
                thread_receive_client.Abort();

                wReceivingClient = false;

                if (clientSockStream != null)
                    clientSockStream.Close();

                if (tcpClient != null)
                    tcpClient.Close();

            }

            if (window.wServer == true)
            {
                thread_receive_server.Abort();

                wReceivingServer = false;

                if (serverSockStream != null)
                    serverSockStream.Close();

                if (tcpListener != null)
                    tcpListener.Stop();

                if (soTcpServer != null)
                    soTcpServer.Shutdown(SocketShutdown.Both);

            }

        }

        private static int Asc(string character)
        {
            //_____________________________________________________________________________________________
            //
            // VB.NET ASC function
            //_____________________________________________________________________________________________

            if (character.Length == 1)
            {
                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                int intAsciiCode = (int)asciiEncoding.GetBytes(character)[0];
                return (intAsciiCode);
            }
            else
            {
                throw new ApplicationException("Character is not valid.");
            }

        }   //private static int Asc(string character)

        #endregion

    }	//public class Network
}
