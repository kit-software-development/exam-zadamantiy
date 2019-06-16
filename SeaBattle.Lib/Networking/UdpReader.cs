using System.Net;
using System.Net.Sockets;
using SeaBattle.Lib.Messaging;

namespace SeaBattle.Lib.Networking
{
    /// <summary>
    /// Class that expands Network Reader Interface
    /// </summary>
    /// <typeparam name="TE">Message type</typeparam>
    class UdpReader<TE> : INetworkReader<TE>
    {
        private IPEndPoint _sender;

        private readonly UdpClient _socket;
        public IPEndPoint Sender => _sender;
        
        public UdpReader(UdpClient client)
        {
            _socket = client;
        }

        /// <summary>
        /// Disposing message
        /// </summary>
        public void Dispose() => _socket.Close();

        /// <summary>
        /// Reads message
        /// </summary>
        /// <returns>Message</returns>
        public TE Read()
        {
            byte[] data = _socket.Receive(ref _sender);
            return MessageFactory.Deserialize<TE>(data);
        }
    }
}