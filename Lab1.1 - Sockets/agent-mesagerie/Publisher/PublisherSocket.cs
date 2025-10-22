using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Publisher
{
    internal class PublisherSocket
    {
        private Socket _socket;
        public bool IsConnected;
        private ManualResetEvent _connectDone = new ManualResetEvent(false);

        public PublisherSocket()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(string ipAddress, int port)
        {
            /*_socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ipAddress), port), ConnectedCallback, null);
            // Spoate de imbunatatit - aici doar adormim threadul putin, dar se poate si altfel de facut
            Thread.Sleep(2000);*/
            _connectDone.Reset();
            _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ipAddress), port), ConnectedCallback, null);
            // asteptam pana cand conexiunea s-a stabilit
            _connectDone.WaitOne();
        }

        public void Send(byte[] data)
        {
            try
            {
                _socket.Send(data);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Could not send data. {ex.Message}");
            }
        }

        private void ConnectedCallback(IAsyncResult asyncResult)
        {
            if (_socket.Connected)
            {
                Console.WriteLine("Sender connected to Broker");
            }
            else
            {
                Console.WriteLine("Error: Sender not connected to Broker");
            }

            IsConnected = _socket.Connected;
            // signalul ca conexiunea a fost efectuata
            _connectDone.Set();
        }
    }
}