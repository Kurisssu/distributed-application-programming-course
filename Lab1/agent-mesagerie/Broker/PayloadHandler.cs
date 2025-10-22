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
            // Convertim tabloul de octeți primit de la Publisher într-un șir de caractere
            var payloadString = Encoding.UTF8.GetString(payloadBytes);

            // Verificăm tipul payloadului primit. Dacă este mesaj de abonare...
            // ...sau mesaj ce trebuie trimis către abonați
            if (payloadString.StartsWith("subscribe#"))
            {
                connectionInfo.Topic = payloadString.Split("subscribe#").LastOrDefault();
                // Adaugam in storage de conexiuni informatia despre aceasta
                ConnectionStorage.Add(connectionInfo);
            }
            else
            {
                // Deserializăm payloadul obținut și îl adăugăm în coada noastră de mesaje
                Payload payload = JsonConvert.DeserializeObject<Payload>(payloadString);
                PayloadStorage.Add(payload);
            }

            Console.WriteLine(payloadString);
        }
    }
}
