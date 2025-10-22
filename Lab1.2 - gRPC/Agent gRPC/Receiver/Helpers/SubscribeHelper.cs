using Common;
using Grpc.Net.Client;
using gRPCagent;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Receiver.Helpers
{
    // Înregistrăm receiverul la topic pentru ascultarea mesajelor
    public class SubscribeHelper
    {
        public static async Task SubscribeAsync(IHost host)
        {
            // Creăm un grpcChannel către Broker
            using var channel = GrpcChannel.ForAddress(EndpointsConstants.BrokerAddress);
            var client = new Subscriber.SubscriberClient(channel);

            // Obținem din DI (dependency injection) serviciile necesare pentru ca să aflăm adresa concretă a Brokeruli
            var server = host.Services.GetRequiredService<IServer>();
            var address = server.Features.Get<IServerAddressesFeature>()?.Addresses.First();
            Console.WriteLine($"Subscriber listening at: {address}");

            Console.WriteLine("Enter the topic: ");
            var topic = Console.ReadLine()?.ToLower() ?? string.Empty;

            var request = new SubscribeRequest() { Address = address, Topic = topic };

            try
            {
                var reply = await client.SubscribeAsync(request);
                Console.WriteLine($"Subscribed reply: {reply.IsSuccess}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error subscribing: {ex.Message}");
            }
        }
    }
}
