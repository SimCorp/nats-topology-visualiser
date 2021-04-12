using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace backend.models
{
    public class Connection
    {

        public Connection()
        {
            connections = new ConcurrentBag<ConnectionEntry>();
        }

        public string server_id { get; set; }
        public string now { get; set; }
        public int num_connections { get; set; }
        public int offset { get; set; }
        public long limit { get; set; }
        public ConcurrentBag<ConnectionEntry> connections { get; set; }

        public void toStringPrint()
        {
            foreach (var connode in connections)
            {
                Console.WriteLine("Con id: " + connode.cid);
            }
        }
    }

    public class ConnectionEntry
    {
        public int cid { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
        public DateTime start { get; set; }
        public DateTime last_activity { get; set; }
        public string rtt { get; set; }
        public string uptime { get; set; }
        public long pending_byten { get; set; }
        public long in_msgs { get; set; }
        public long out_msgs { get; set; }
        public long in_bytes { get; set; }
        public long out_bytes { get; set; }
        public long subscriptions { get; set; }
        public string lang { get; set; }
        public string version { get; set; } 

    }

}