using System;
using System.Collections.Generic;

namespace backend.models
{
    public class Connection
    {

        public Connection()
        {
            connections = new HashSet<connection_node>();
        }

        public string server_id { get; set; }
        //public DateTime now { get; set; }
        public int num_connections { get; set; }
        public int offset { get; set; }
        public long limit { get; set; }
        public ICollection<connection_node> connections { get; set; }

        public void toStringPrint()
        {
            foreach (var connode in connections)
            {
                Console.WriteLine("Con id: " + connode.cid);
            }
        }
    }

    public class connection_node
    {
        public int cid { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
        public DateTime start { get; set; }
        public DateTime last_activity { get; set; }
        public string rtt { get; set; }
        public string uptime { get; set; }
        public int pending_byten { get; set; }
        public int in_msgs { get; set; }
        public int out_msgs { get; set; }
        public int in_bytes { get; set; }
        public int out_bytes { get; set; }
        public int subscriptions { get; set; }
        public string lang { get; set; }
        public string version { get; set; } 

    }

}