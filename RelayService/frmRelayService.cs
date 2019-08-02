using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace RelayService
{
    public partial class frmRelayService : Form
    {
        private int port = 10020;
        private TcpListener tcpListener;
        private Thread tcpListenerThread;
        private bool isExit = false;
        
        private static List<NetworkClient> clients = new List<NetworkClient>();

        private delegate void AddItemToListBoxDelegate(string str);
        private delegate void RemoveItemToListBoxDelegate(string str);


        private void AddItemToListBox(string str)
        {
            if (clientList.InvokeRequired)
            {
                AddItemToListBoxDelegate d = AddItemToListBox;
                clientList.Invoke(d, str);
            }
            else
            {
                clientList.Items.Add(str);
                clientList.Refresh();
            }
        }

        private void RemoveItemToListBox(string str)
        {
            if (clientList.InvokeRequired)
            {
                RemoveItemToListBoxDelegate d = RemoveItemToListBox;
                clientList.Invoke(d, str);
            }
            else
            {
                clientList.Items.Remove(str);
                clientList.Refresh();
            }
        }


        public frmRelayService()
        {
            InitializeComponent();

            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();

            tcpListenerThread = new Thread(acceptConnect);
            tcpListenerThread.IsBackground = true;
            tcpListenerThread.Start();

        }

        

        /// <summary>
        /// 接受客户端连接
        /// </summary>
        void acceptConnect() {

            while (!isExit)
            {
                try
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    if (tcpClient.Connected)
                    {
                        NetworkClient networkClient = new NetworkClient(tcpClient);
                        networkClient.ConnectionDisconnected += NetworkClient_ConnectionDisconnected;
                        networkClient.Connected += NetworkClient_Connected;
                        clients.Add(networkClient);
                    }
           
                }
                catch (Exception ex) {

                }
            }
        }

        private void NetworkClient_Connected(object sender, ConnectedEventArgs e)
        {

            NetworkClient networkClient = (NetworkClient)sender;
            AddItemToListBox($"客户端:{networkClient.remoteEndPoint}");

        }

        private void NetworkClient_ConnectionDisconnected(object sender, ConnectionDisconnectedEventArgs e)
        {

            NetworkClient networkClient = (NetworkClient)sender;
            clients.Remove(networkClient);
            RemoveItemToListBox($"客户端:{networkClient.remoteEndPoint}");

        }

        private void frmRelayService_FormClosing(object sender, FormClosingEventArgs e)
        {
            try {

                isExit = true;
                tcpListener.Stop();
                tcpListenerThread.Abort();
                Application.Exit();
            }
            catch(Exception ex)
            {

            }
        }
    }
}
