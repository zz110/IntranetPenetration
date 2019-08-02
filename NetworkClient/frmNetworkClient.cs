using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace NetworkClient
{
    public partial class frmNetworkClient : Form
    {
        private string remoteIp = "";
        private int remotePort = 0;
        private Thread thread;
        private Thread heartbeatThread;
        private Thread daemonThread;
        private TcpClient tcpClient;
        private bool Connected = false;

        public BinaryReader br { get; private set; }
        public BinaryWriter bw { get; private set; }

        public frmNetworkClient()
        {
            InitializeComponent();

            this.btnConnect.Enabled = true;
            this.btnUnConnect.Enabled = false;
        }

        void daemon() {

            while (true) {

                try
                {

                    if (!Connected && this.tcpClient != null)
                    {
                        Console.WriteLine("正在重新连接");
                        initConnection();
                    }
                    Thread.Sleep(5000);
                }
                catch (Exception ex) {

                }
            }
        }

        void receive_data() {

            while (Connected) {

                try
                {
                    string receiveString = null;
                    receiveString = br.ReadString();

                    Console.WriteLine(receiveString);
                }
                catch (Exception ex) {

                }
            }
        }

        void heartbeat() {

            while (true) {

                try
                {
                    this.bw.Write("okok");
                    this.bw.Flush();
                    Thread.Sleep(5000);

                }
                catch (Exception ex)
                {
                    this.UnConnect();
                    break;
                }

            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.btnConnect.Enabled = false;
            this.btnUnConnect.Enabled = true;
            this.remoteIp = this.txtRemoteIp.Text;
            this.remotePort = int.Parse(this.txtRemotePort.Text);

            initConnection();

            if (daemonThread == null)
            {
                daemonThread = new Thread(daemon);
                daemonThread.IsBackground = true;
                daemonThread.Start();
            }
        }

        void initConnection() {

            try
            {
                this.tcpClient = new TcpClient();
                this.tcpClient.Connect(IPAddress.Parse(this.remoteIp), this.remotePort);
                if (tcpClient.Connected) {

                    this.br = new BinaryReader(tcpClient.GetStream());
                    this.bw = new BinaryWriter(tcpClient.GetStream());
                    this.bw.Write($"login,{Guid.NewGuid().ToString()}");
                    this.bw.Flush();

                    thread = new Thread(receive_data);
                    thread.IsBackground = true;
                    thread.Start();

                    heartbeatThread = new Thread(heartbeat);
                    heartbeatThread.IsBackground = true;
                    heartbeatThread.Start();

                    Connected = true;
                    Console.WriteLine("连接成功");
                }

            }
            catch (Exception ex) {
                Console.WriteLine("连接失败：" + ex.Message);
            }

        }

        private void btnUnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                this.bw.Write("disconnected");
                this.bw.Flush();
                Thread.Sleep(1000);

                UnConnect();

                this.tcpClient = null;
                this.btnConnect.Enabled = true;
                this.btnUnConnect.Enabled = false;
                daemonThread.Abort();
                daemonThread = null;
            }
            catch (Exception ex)
            {

            }

        }

        void UnConnect() {

            try
            {
                Connected = false;
                this.br.Close();
                this.bw.Close();
                this.tcpClient.Close();
                this.thread.Abort();
                this.heartbeatThread.Abort();
            }
            catch (Exception ex)
            {

            }
        }

        private void frmNetworkClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {

                UnConnect();
                daemonThread.Abort();
                Application.Exit();
            }
            catch (Exception ex) {

            }
        }
    }
}
