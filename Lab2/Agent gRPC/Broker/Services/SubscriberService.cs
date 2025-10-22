using Broker.Models;
using Broker.Services.Interfaces;
using Grpc.Core;
using gRPCagent;

namespace Broker.Services
{
    // Serviciu de tip singleton care va pastra conexiunile noastre intr-o colectie concurenta
    public class SubscriberService : Subscriber.SubscriberBase
    {
        private readonly IConnectionStorageService _connectionStorage;

        public SubscriberService(IConnectionStorageService connectionStorage)
        {
            _connectionStorage = connectionStorage;
        }
        
        // Metoda care răspunde de primirea requestului de abonare de către Receiver
        public override Task<SubscribeReply> Subscribe(SubscribeRequest request, ServerCallContext context)
        {
            Console.WriteLine($"New client trying to subscribe: {request.Address} {request.Topic}");

            try
            {
                var connection = new Connection(request.Address, request.Topic);
                _connectionStorage.Add(connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not add the new connection : {request.Address}, {request.Topic}. {ex.Message}");
            }

            return Task.FromResult(new SubscribeReply()
            {
                IsSuccess = true
            });
        }
    }
}
