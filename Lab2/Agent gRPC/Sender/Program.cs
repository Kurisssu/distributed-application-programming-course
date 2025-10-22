using Common;
using Grpc.Net.Client;
using gRPCagent;
using System;

namespace Sender
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Publisher");

            var channel = GrpcChannel.ForAddress(EndpointsConstants.BrokerAddress);
            var client = new Publisher.PublisherClient(channel);

            while (true)
            {
                Console.Write("Enter the topic: ");
                var topic = Console.ReadLine().ToLower();

                Console.Write("Enter content: ");
                var content = Console.ReadLine();

                var request = new PublishRequest() { Topic = topic, Content = content };

                try
                {
                    // Aici PublishMessage ar fi o metoda sincrona care ar bloca firul de executie pe o aplicatie reala...
                    // ..., din cauza aceasta folosim Async si asteptam rezultatul
                    var reply = await client.PublishMessageAsync(request);
                    Console.WriteLine($"Publish Reply: {reply.IsSuccess}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error publishing the message: {ex.Message}");
                }
            }
        }
    }
}
