using System.Collections.Generic;
using System.Collections.Concurrent;

namespace backend
{
    public class Gateway
    {

        public Gateway()
        {
            inbound_gateways = new ConcurrentDictionary<string, List<GatewayNodeWrapper>>();
            outbound_gateways = new ConcurrentDictionary<string, GatewayNodeWrapper>();
        }

        public string server_id { get; set; }
        public string now { get; set; }
        public string name { get; set; }
        public string host { get; set; }
        public int port { get; set; }
        public ConcurrentDictionary<string, List<GatewayNodeWrapper>> inbound_gateways { get; set; }
        public ConcurrentDictionary<string, GatewayNodeWrapper> outbound_gateways { get; set; }

        public class GatewayNode
        {
            public int cid { get; set; }
            public string ip { get; set; }
            public int port { get; set; }
            public string start { get; set; }
            public string last_activity { get; set; }
            public string rtt { get; set; }
            public string uptime { get; set; }
            public string idle { get; set; }
            public int pending_bytes { get; set; }
            public int in_msgs { get; set; }
            public int in_bytes { get; set; }
            public int out_bytes { get; set; }
            public int subscriptions { get; set; }
            public string name { get; set; }
            
        }

        public class GatewayNodeWrapper
        {
            public bool configured { get; set; }
            public GatewayNode connection { get; set; }
        }
        
    }
}