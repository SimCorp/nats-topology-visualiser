using System;

namespace backend
{
    public class Route
    {
        public int rid { get; set; }
        public string remote_id { get; set; }
        public bool did_solicit { get; set; }
        public bool is_configured { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
        public long pending_size { get; set; }
        public string rtt { get; set; }
        public long in_msgs { get; set; }
        public long out_msgs { get; set; }
        public long in_bytes { get; set; }
        public long out_bytes { get; set; }
        public int subscriptions { get; set; }
    }
}