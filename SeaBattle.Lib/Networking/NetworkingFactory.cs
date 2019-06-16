using System.Net;
using System.Net.Sockets;

namespace SeaBattle.Lib.Networking
{
    /// <summary>
    /// Networking factory to create new UDP readers and writers
    /// </summary>
    public static class NetworkingFactory
    {
        /// <summary>
        /// Creates new reader for TE message type on port
        /// </summary>
        /// <typeparam name="TE">Message type</typeparam>
        /// <param name="port">Port</param>
        /// <returns></returns>
        public static INetworkReader<TE> UdpReader<TE>(int port)
        {
            var client = new UdpClient(port);
            return new UdpReader<TE>(client);
        }

        /// <summary>
        ///  Creates new writer for TE message type on port to address
        /// </summary>
        /// <typeparam name="TE">Message type</typeparam>
        /// <param name="address">Receiver address</param>
        /// <param name="port">Port</param>
        /// <returns></returns>
        public static INetworkWriter<TE> UdpWriter<TE>(IPAddress address, int port)
        {
            var client = new UdpClient();
            client.Connect(address, port);
            return new UdpWriter<TE>(client);
        }

        /// <summary>
        /// Creates new broadcase write for TE message type on port
        /// </summary>
        /// <typeparam name="TE">Message type</typeparam>
        /// <param name="port">Port</param>
        /// <returns></returns>
        public static INetworkWriter<TE> Broadcast<TE>(int port)
        {
            var client = new UdpClient { EnableBroadcast = true };
            client.Connect(IPAddress.Broadcast, port);
            return new UdpWriter<TE>(client);
        }
    }
}