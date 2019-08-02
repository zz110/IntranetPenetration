using System;
using System.Net.Sockets;

namespace RelayService
{
    /// <summary>
    /// 连接断开事件
    /// </summary>
    public class ConnectionDisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Tcp连接
        /// </summary>
        public TcpClient Client
        {
            get;
            internal set;
        }
    }
}