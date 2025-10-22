using Common;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Publisher (ak Sender)");

            var publisherSocket = new PublisherSocket();
            publisherSocket.Connect(Settings.BROKER_IP, Settings.BROKER_PORT);

            if (publisherSocket.IsConnected)
            {
                while (true)
                {
                    // Construirea payloadului pentru prelucrarea mesajului
                    var payload = new Payload();

                    Console.Write("Enter the topic: ");
                    payload.Topic = Console.ReadLine().ToLower();

                    Console.Write("Enter the message: ");
                    payload.Message = Console.ReadLine();

                    // Serializarea payloadului (format JSON)
                    var payloadString = JsonConvert.SerializeObject(payload);
                    // Convertim șirul de caractere într-un tablou de octeți folosind codificarea UTF-8
                    byte[] data = Encoding.UTF8.GetBytes(payloadString);

                    // Trimitem datele convertite către Broker
                    publisherSocket.Send(data);
                }
            }

            Console.ReadLine();
        }
    }
}