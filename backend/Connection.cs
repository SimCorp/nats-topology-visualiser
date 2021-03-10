using System;

namespace backend
{
    public class Connection
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


        //public string server_id { get; set; }

        //public virtual Server Server { get; set; }
    }
}