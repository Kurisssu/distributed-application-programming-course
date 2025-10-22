using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Subscriber
{
    // Creăm socketul pentru Subscriber care:
    // - va conecta abonatul la Broker,
    // - va trimite un mesaj de abonare de tip "subscriber#topic",
    // - va primi notificări de la Broker și le va trimite către PayloadHandler pentru prelucrare
    class SubscriberSocket
    {
        private Socket _socket;
        private string _topic;

        public SubscriberSocket(string topic)
        {
            _topic = topic;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        // Operație asincronă de conectare către Broker
        public void Connect(string ipAddress, int port)
        {
            _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ipAddress), port), ConnectedCallback, null);
            Console.WriteLine("Waiting for a connection");
        }

        // Chemată când BeginConnect a terminat
        // Verifică dacă este conectat, apoi se abonează și începe a asculta pentru notificări noi
        private void ConnectedCallback(IAsyncResult asyncResult)
        {
            if (_socket.Connected)
            {
                Console.WriteLine("Subcriber connected to broker.");
                Subscribe();
                StartReceive();
            }
            else
            {
                Console.WriteLine("Error: Subscriber could not connect to broker");
            }
        }

        // Construiește mesajul de abonare, îl coadează și trimite către Broker
        private void Subscribe()
        {
            // Putem imbunatati. Sa introducem in setari campul subscribe# pentru a nu schimba peste tot unul si acelasi camp
            var data = Encoding.UTF8.GetBytes("subscribe#" + _topic);
            Send(data);
        }

        // Primește și menține bufferul și socketul în ConnectionInfo...
        // ...și pornește operația asincronă de ascultare
        private void StartReceive()
        {
            ConnectionInfo connection = new ConnectionInfo();
            connection.Socket = _socket;

            _socket.BeginReceive
            (
                connection.Buffer, 0, connection.Buffer.Length,
                SocketFlags.None,
                ReceiveCallback,
                connection
            );

        }

        // Callback invocat când datele ajung sau starea socketului se schimbă 
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            ConnectionInfo connectionInfo = asyncResult.AsyncState as ConnectionInfo;

            try
            {
                SocketError response;
                int buffSize = _socket.EndReceive(asyncResult, out response);

                if (response == SocketError.Success)
                {
                    byte[] payloadBytes = new byte[buffSize];
                    Array.Copy(connectionInfo.Buffer, payloadBytes, payloadBytes.Length);

                    PayloadHandler.Handle(payloadBytes);
                }
            } 
            catch (Exception ex)
            {
                Console.WriteLine($"Can't receive data from broker. {ex.Message}");
            }
            finally
            {
                try
                {
                    connectionInfo.Socket.BeginReceive
                    (
                        connectionInfo.Buffer, 0, connectionInfo.Buffer.Length,
                        SocketFlags.None,
                        ReceiveCallback,
                        connectionInfo
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                    connectionInfo.Socket.Close();
                }
            }
        }

        // Trimite mesajul de abonare la Broker
        // Se utilizează în metoda Subscribe
        private void Send(byte[] data)
        {
            try
            {
                _socket.Send(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not send data: {ex.Message}");
            }
        }
    }
}
