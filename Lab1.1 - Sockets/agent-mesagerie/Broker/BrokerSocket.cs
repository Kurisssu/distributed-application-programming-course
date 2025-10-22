using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Broker
{
    class BrokerSocket
    {
        // Nr. maxim de conexiuni simultane acceptate
        private const int CONNECTIONS_LIMIT = 10;

        // Socketul principal folosit pentru ascultarea conexiunilor noi
        private Socket _socket;

        public BrokerSocket()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        // Pornim brokerul pe IPul și portul specificat
        public void Start(String ip, int port)
        {
            // Asocierea socketului la adresă și port
            _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            // Începem ascultarea și acceptarea pentru conexiuni noi, cu limita specificată
            _socket.Listen(CONNECTIONS_LIMIT);
            Accept();
        }

        // Răspunde de acceptarea asincronă a unei conexiuni noi
        private void Accept()
        {
            _socket.BeginAccept(AcceptedCallback, null);
        }

        // Răspunde de prelucrearea conexiunii nou apărute către socket
        private void AcceptedCallback(IAsyncResult asyncResult)
        {
            ConnectionInfo connection = new Common.ConnectionInfo();

            try
            {
                // Finalizăm acceptarea conexiunii și înregistrăm datele despre aceasta
                connection.Socket = _socket.EndAccept(asyncResult);
                // Returneaza ip si port (127.0.0.1:9000)
                connection.Address = connection.Socket.RemoteEndPoint.ToString();
                Console.WriteLine($"[Broker] New connection accepted: {connection.Address};");

                // Începem recepția datelor de la client (publisher/subscriber)
                connection.Socket.BeginReceive
                (
                    connection.Buffer, 0, connection.Buffer.Length, 
                    SocketFlags.None, 
                    ReceiveCallback, 
                    connection
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Can't accept. {ex.Message}");
            }
            finally
            {
                // Din nou acceptam conexiuni noi (dupa ce am conectat unul, trecem mai departe)
                Accept();
            }
        }

        // Răspunde de prelucrarea datelor obținute despre conexiunea nouă
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            ConnectionInfo connection = asyncResult.AsyncState as ConnectionInfo;
            try
            {
                Socket senderSocket = connection.Socket;
                SocketError response;
                int buffSize = senderSocket.EndReceive(asyncResult, out response);

                if (response == SocketError.Success && buffSize > 0)
                {
                    byte[] payload = new byte[buffSize];
                    Array.Copy(connection.Buffer, payload, payload.Length);

                    Console.WriteLine($"[Broker] Received {buffSize} bytes from {connection.Address}");

                    PayloadHandler.Handle(payload, connection);

                    // Doar dacă totul a mers bine, continuăm să ascultăm
                    senderSocket.BeginReceive(
                        connection.Buffer, 0, connection.Buffer.Length,
                        SocketFlags.None,
                        ReceiveCallback,
                        connection
                    );
                }
                else
                {
                    // Dacă socketul a fost închis din exterior, ștergem conexiunea conexiunea
                    Console.WriteLine($"Connection closed by remote host: {connection.Address}");
                    ConnectionStorage.Remove(connection.Address);
                    senderSocket.Close();
                }
            }
            catch (Exception ex)
            {
                // În caz de eroare la recepție, curățăm conexiunea și închidem socketul
                Console.WriteLine($"Can't receive data: {ex.Message}");
                try
                {
                    var address = connection.Socket.RemoteEndPoint.ToString();
                    ConnectionStorage.Remove(address);
                    connection.Socket.Close();
                }
                catch { }
            }
        }
    }
}
