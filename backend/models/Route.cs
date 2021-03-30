using System;
using System.Collections.Generic;
using backend.models;
using System.Collections.Concurrent;

namespace backend
{
    public class Route
    {
       public string server_id { get; set; }
       public string now { get; set; }
       public int num_routes { get; set; }
       public ConcurrentBag<RouteNode> routes { get; set; }
    }

    public class RouteNode
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