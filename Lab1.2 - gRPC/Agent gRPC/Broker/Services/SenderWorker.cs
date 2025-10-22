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
                // Se obțin serviciile din scope (din containerul DI)
                _messageStorage = scope.ServiceProvider.GetRequiredService<IMessageStorageService>();
                _connectionStorage = scope.ServiceProvider.GetRequiredService<IConnectionStorageService>();
            } // scope.Dispose() apelat automat aici
        }

        // Metodă apelată la pornirea hosted service-ului
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Creează un timer care apelează DoSendWork imediat și apoi la fiecare TimeToWait ms
            _timer = new Timer(DoSendWork, null, 0, TimeToWait);
            return Task.CompletedTask;
        }

        // Metodă apelată la oprirea hosted service-ului
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Oprește timer-ul pentru a împiedica apeluri viitoare
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        // Funcția principală: preia mesaje din storage și le trimite abonaților relevanți
        private void DoSendWork(object? state)
        {
            // Atâta vreme cât există mesaje în coadă
            while (!_messageStorage.IsEmpty())
            {
                // Extrage următorul mesaj (probabil îl consumă din coadă)
                var message = _messageStorage.GetNext();

                if (message != null)
                {
                    // Obține lista conexiunilor care s-au abonat la topicul mesajului
                    var connections = _connectionStorage.GetConnectionsByTopic(message.Topic);

                    // Pentru fiecare conexiune trimitem notificarea
                    foreach (var connection in connections)
                    {
                        // Construim clientul gRPC folosind canalul stocat în Connection
                        var client = new Notifier.NotifierClient(connection.Channel);
                        var request = new NotifyRequest() { Content = message.Content };

                        try
                        {
                            // Apel sincronic către subscriber: Notify
                            // (blochează thread-ul curent; o versiune recomandată ar fi NotifyAsync)
                            var reply = client.Notify(request);
                            Console.WriteLine($"Notified subscriber {connection.Address} with {message.Content}. Response : {reply.IsSuccess}");
                        }
                        catch (RpcException rpcEx)
                        {
                            // Dacă serverul remote a returnat StatusCode.Internal considerăm conexiunea invalidă
                            // și o eliminăm din storage
                            if (rpcEx.StatusCode == StatusCode.Internal)
                            {
                                _connectionStorage.Remove(connection.Address);
                            }

                            // Logare eroare RPC
                            Console.WriteLine($"RPC Error notifying subscriber {connection.Address}. {rpcEx.Message}");
                        }
                        catch (Exception ex)
                        {
                            // Logare orice altă eroare
                            Console.WriteLine($"Error notifying subscriber {connection.Address}. {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
