using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace RelayService
{
    /// <summary>
    /// 网络客户端
    /// </summary>
    public class NetworkClient
    {

        public event EventHandler<ConnectionDisconnectedEventArgs> ConnectionDisconnected;
        public event EventHandler<ConnectedEventArgs> Connected;

        public TcpClient tcpClient;
        public BinaryReader br { get; private set; }
        public BinaryWriter bw { get; private set; }
        public string remoteEndPoint { get; set; }
        public string ClientId { get; set; }
        public DateTime lastReceiveDataTime { get; set; }
        public int HeartBeatInterval { get; set; } = 5000;
        public bool isAlive { get; set; } = true;

        private Thread thread;
        private Thread daemonThread;

        public NetworkClient(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            this.remoteEndPoint = this.tcpClient.Client.RemoteEndPoint.ToString();
            this.br = new BinaryReader(this.tcpClient.GetStream());
            this.bw = new BinaryWriter(this.tcpClient.GetStream());
            this.lastReceiveDataTime = DateTime.Now;
            thread = new Thread(receive_data);
            thread.IsBackground = true;
            thread.Start();

            daemonThread = new Thread(daemon);
            daemonThread.IsBackground = true;
            daemonThread.Start();

        }

        void daemon() {

            while (isAlive) {

                try
                {
                    this.bw.Write("okok");
                    this.bw.Flush();
                    Thread.Sleep(HeartBeatInterval);
                }
                catch (Exception ex) {
                    isAlive = false;
                    ConnectionDisconnected?.Invoke(this, new ConnectionDisconnectedEventArgs()
                    {
                        Client = this.tcpClient
                    });
                    this.Close();
                    Console.WriteLine($"客户端:{this.remoteEndPoint},失去连接。");
                }
            }
        }
        


        void receive_data() {

            while (true) {

                string receiveString = null;
                try
                {
                    receiveString = this.br.ReadString();
                    lastReceiveDataTime = DateTime.Now;
                    string[] splitString = receiveString.Split(',');
                    switch (splitString[0])
                    {

                        case "login":
                            isAlive = true;
                            this.ClientId = splitString[1];
                            Console.WriteLine("连接客户端id:" + this.ClientId);
                            Connected?.Invoke(this, new ConnectedEventArgs() { Client = this.tcpClient });
                            break;
                        case "disconnected":
                            isAlive = false;
                            ConnectionDisconnected?.Invoke(this, new ConnectionDisconnectedEventArgs()
                            {
                                Client = this.tcpClient
                            });
                            this.Close();
                            break;
                    }
                }
                catch(Exception ex)
                {
                    break;
                }
              
            }
        }

        public void Close() {

            this.br.Close();
            this.bw.Close();
            this.tcpClient.Close();
            thread.Abort();
            daemonThread.Abort();
        }
    }
}
