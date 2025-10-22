using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class EndpointsConstants
    {
        // Indicam locahost in loc de 127.0.0.1, deoarece avem eroare ca "remote certificate is invalid"...
        // ...si conexiunea de tip SSL nu poate fi stabilita
        public const string BrokerAddress = "https://localhost:5001";

        // Indicam 0 ca sistemul de operare sa atribuie un port liber existent
        public const string SubscriberAddress = "https://[::1]:0";
    }
}
