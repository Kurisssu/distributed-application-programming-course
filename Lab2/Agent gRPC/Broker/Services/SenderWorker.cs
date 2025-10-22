using Broker.Services.Interfaces;
using Grpc.Core;
using gRPCagent;

namespace Broker.Services
{
    public class SenderWorker : IHostedService
    {
        // Inițializăm resursele necesare
        private Timer? _timer;
        private const int TimeToWait = 2000;
        private readonly IMessageStorageService _messageStorage;
        private readonly IConnectionStorageService _connectionStorage;

        // Constructor pentru extragerea în scope a serviciilor necesare
        // IServiceScopeFactory - ajută la crearea propriului scope (context de viață pentru servicii),...
        // care nu rulează în contextul unui request, dar per aplicație *
        // În cazul dat avem un scope cu servicii ce ajută la prelucrarea mesajelor trimise către borker și conexiunilor efectuate
        public SenderWorker(IServiceScopeFactory serviceScopeFactory)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                _messageStorage = scope.ServiceProvider.GetRequiredService<IMessageStorageService>();
                _connectionStorage = scope.ServiceProvider.GetRequiredService<IConnectionStorageService>();
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoSendWork, null, 0, TimeToWait);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        // Funcția principală care, într-un loop, așteatpă mesaje noi în messageStorage și le prelucrează
        private void DoSendWork(object? state)
        {
            while (!_messageStorage.IsEmpty())
            {
                var message = _messageStorage.GetNext();

                if (message != null)
                {
                    var connections = _connectionStorage.GetConnectionsByTopic(message.Topic);

                    foreach (var connection in connections)
                    {
                        var client = new Notifier.NotifierClient(connection.Channel);
                        var request = new NotifyRequest() { Content = message.Content };

                        try
                        {
                            // La toate conexiunile va fi transmis mesajul nostru
                            var reply = client.Notify(request);
                            Console.WriteLine($"Notified subscriber {connection.Address} with {message.Content}. Response : {reply.IsSuccess}");
                        }
                        catch (RpcException rpcEx)
                        {
                            if (rpcEx.StatusCode == StatusCode.Internal)
                            {
                                _connectionStorage.Remove(connection.Address);
                            }

                            Console.WriteLine($"RPC Error notifying subscriber {connection.Address}. {rpcEx.Message}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error notifying subscriber {connection.Address}. {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
