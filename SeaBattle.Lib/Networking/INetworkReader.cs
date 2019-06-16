using System;
using System.Net;

namespace SeaBattle.Lib.Networking
{
    /// <summary>
    /// Network reader interface for message of TE type
    /// </summary>
    /// <typeparam name="TE">Message type</typeparam>
    public interface INetworkReader<out TE> : IDisposable
    {
        IPEndPoint Sender { get; }

        TE Read();
    }
}