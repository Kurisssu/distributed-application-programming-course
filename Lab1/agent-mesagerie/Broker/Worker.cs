using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broker
{
    public class Worker
    {
        private const int TIME_TO_SLEEP = 500;

        public void DoSendMessageWork()
        {
            while(true)
            {
                while (!PayloadStorage.IsEmpty())
                {
                    // Preluarea mesajelor din coadă
                    var payload = PayloadStorage.GetNext();

                    if (payload != null)
                    {

                        var connections = ConnectionStorage.GetConnectionByTopic(payload.Topic);

                        // Transmiterea mesajelor către toți abonații la topicul specific
                        foreach (var connection in connections)
                        {
                            // Serializăm, convertim și trimitem mesajul
                            var payloadString = JsonConvert.SerializeObject(payload);
                            byte[] data = Encoding.UTF8.GetBytes(payloadString);

                            connection.Socket.Send(data);
                        }
                    }
                }
                
                // Prevenim consumul inutil de resurse ale procesorului
                Thread.Sleep(TIME_TO_SLEEP);
            }
        }
    }
}
