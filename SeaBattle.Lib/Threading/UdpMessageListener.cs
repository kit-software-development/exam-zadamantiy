using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using SeaBattle.Lib.Networking;

namespace SeaBattle.Lib.Threading
{
    /// <summary>
    /// Class with the incoming message args
    /// </summary>
    /// <typeparam name="TE">Message type</typeparam>
    public sealed class IncomingMessageEventArgs<TE> : EventArgs
    {
        /// <summary>
        /// Message
        /// </summary>
        public TE Message { get; internal set; }

        /// <summary>
        /// Info about sender
        /// </summary>
        public IPEndPoint Sender { get; internal set; }
    }

    /// <summary>
    /// Message listener
    /// </summary>
    /// <typeparam name="TE">type of message</typeparam>
    public class UdpMessageListener<TE> : IDisposable
    {
        private bool _exit;
        private readonly INetworkReader<TE> _reader;
        public event EventHandler<IncomingMessageEventArgs<TE>> IncomingMessage;

        /// <summary>
        /// Udp message listener
        /// </summary>
        /// <param name="port">port which we are going to check</param>
        public UdpMessageListener(int port)
        {
            _reader = NetworkingFactory.UdpReader<TE>(port);
        }

        /// <summary>
        /// Listener thread
        /// </summary>
        private void ThreadProc()
        {
            while (!_exit)
            {
                try
                {
                    //E massage = reader.Read();
                    var args = new IncomingMessageEventArgs<TE>
                    {
                        Message = _reader.Read(),
                        Sender = _reader.Sender
                    };
                    IncomingMessage?.Invoke(this, args);
                }
                catch (Exception e)
                {
                    //TODO: REMOVE THIS
                    string message = e.Message;
                    string caption = "Error Detected in Input";
                    var buttons = MessageBoxButtons.OK;

                    // Displays the MessageBox.
                    MessageBox.Show(message, caption, buttons);
                }
            }
        }

        /// <summary>
        /// Starts listener
        /// </summary>
        public void Start()
        {
            var thread = new Thread(ThreadProc) { IsBackground = true };
            thread.Start();
        }
        
        /// <summary>
        /// Disposing listener
        /// </summary>
        public void Dispose()
        {
            _exit = true;
            _reader.Dispose();
        }
    }
}