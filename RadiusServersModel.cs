using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flexinets.Radius
{
    public class RemoteAddress
    {
        public string address { get; set; }
    }

    public class RadiusClient
    {
        public string name { get; set; }
        public string secret { get; set; }
        public string handler { get; set; }
        public List<RemoteAddress> remoteAddresses { get; set; }
    }

    public class RadiusServersModel
    {
        public List<RadiusClient> radiusClients { get; set; }
    }
}
