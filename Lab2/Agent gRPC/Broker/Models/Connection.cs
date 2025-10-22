using Grpc.Net.Client;
using System.Net.Http;

namespace Broker.Models
{
    public class Connection
    {
        public Connection(string address, string topic)
        {
            Address = address;
            Topic = topic;

            // La crearea canalului Grpc, utilizăm un handler care va dezactiva verificarea certificatelor SSL.
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            Channel = GrpcChannel.ForAddress(
                address,
                new GrpcChannelOptions { HttpHandler = httpHandler }
            );
        }

        public string Address { get; }
        
        public string Topic { get; }

        public GrpcChannel Channel { get; }
    }
}
