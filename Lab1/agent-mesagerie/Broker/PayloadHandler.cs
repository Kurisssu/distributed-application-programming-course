using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Broker
{
    class PayloadHandler
    {
        public static void Handle(byte[] payloadBytes, ConnectionInfo connectionInfo)
        {
            var payloadString = Encoding.UTF8.GetString(payloadBytes);

            if (payloadString.StartsWith("subscribe#"))
            {
                connectionInfo.Topic = payloadString.Split("subscribe#").LastOrDefault();
                // Adaugam in storage de conexiuni informatia despre aceasta
                ConnectionStorage.Add(connectionInfo);
            }
            else
            {
                Payload payload = JsonConvert.DeserializeObject<Payload>(payloadString);
                PayloadStorage.Add(payload);
            }

            Console.WriteLine(payloadString);
        }
    }
}
