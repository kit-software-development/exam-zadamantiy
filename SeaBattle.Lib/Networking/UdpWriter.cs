using System.Net.Sockets;
using SeaBattle.Lib.Messaging;

namespace SeaBattle.Lib.Networking
{
    /// <summary>
    /// Class that expands Network Writer Interface
    /// </summary>
    /// <typeparam name="TE">Type of message</typeparam>
    class UdpWriter<TE> : INetworkWriter<TE>
    {
        private readonly UdpClient _socket;
        
        public UdpWriter(UdpClient client)
        {
            _socket = client;
        }


        public void Dispose() => _socket.Close();
        
        public void Write(TE message)
        {
            var data = message.Serialize();
            _socket.Send(data, data.Length);
        }
    }
}