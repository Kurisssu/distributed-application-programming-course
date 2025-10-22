using Grpc.Core;
using gRPCagent;

namespace Receiver.Services
{
    // Primirea, procesarea și expunerea mesajului primit de la Broker în consolă
    public class NotificationService : Notifier.NotifierBase
    {
        public override Task<NotifyReply> Notify(NotifyRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Received: {request.Content}");

            return Task.FromResult(new NotifyReply() { IsSuccess = true });
        }
    }
}
