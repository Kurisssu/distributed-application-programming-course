using System;
using Common;

namespace Broker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Broker");

            BrokerSocket socket = new BrokerSocket();
            socket.Start(Settings.BROKER_IP, Settings.BROKER_PORT);

            var worker = new Worker();

            // Demararea unei operații asincrone care va crea un fir de execuție separat...
            // ...dedicat pentru managerul de mesaje (Worker)
            // Indicăm ca parametru/flag faptul că acest Task va fi de lungă durată
            Task.Factory.StartNew(worker.DoSendMessageWork, TaskCreationOptions.LongRunning);

            Console.ReadLine();
        }
    }
}