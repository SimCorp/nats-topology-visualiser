using System;
using System.Collections.Generic;
using backend.models;

namespace backend
{
    public class Gateway
    {

        public Gateway()
        {
            inbound_gateways = new Dictionary<string, List<ConnectionNodeWrapper>>();
            outbound_gateways = new Dictionary<string, ConnectionNodeWrapper>();
            /*outbound_gateways = new outboundGateways();
            inbound_gateways = new inboundGateways();*/
        }

        public string server_id { get; set; }
        public string now { get; set; }
        public string name { get; set; }
        public string host { get; set; }
        public int port { get; set; }
        public Dictionary<string, List<ConnectionNodeWrapper>> inbound_gateways { get; set; }
        public Dictionary<string, ConnectionNodeWrapper> outbound_gateways { get; set; }
        //public inboundGateways inbound_gateways { get; set; }


        public class outboundGateways
        {
            public bool configured { get; set; }
            public ICollection<connection_node> connode { get; set; }

            public outboundGateways()
            {
                connode = new List<connection_node>();
            }
        }
        public class inboundGateways
        {
            public bool configured { get; set; }
            public ICollection<connection_node> connode { get; set; }

            public inboundGateways()
            {
                connode = new List<connection_node>();
            }
        }

        public class connection_node
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

        public class ConnectionNodeWrapper
        {
            public bool configured { get; set; }
            public connection_node connection { get; set; }
        }
        
    }
}