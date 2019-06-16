using System;

namespace SeaBattle.Lib.Networking
{
    /// <summary>
    /// Network writer interface for message of TE type
    /// </summary>
    /// <typeparam name="TE">Message type</typeparam>
    public interface INetworkWriter<in TE> : IDisposable
    {
        void Write(TE message);
    }
}