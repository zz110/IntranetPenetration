using System;
using System.Net.Sockets;

namespace RelayService
{
    /// <summary>
    /// 连接事件
    /// </summary>
    public class ConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Tcp
        /// </summary>
        public TcpClient Client
        {
            get;
            internal set;
        }
    }
}