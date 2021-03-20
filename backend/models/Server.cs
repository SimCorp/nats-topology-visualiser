using System;
using System.Collections.Generic;

namespace backend.models
{
    public class Server
    {

        public Server()
        {
            //connections = new HashSet<Connection>();
            connectionsList = new List<Connection>();
            routesList = new List<Route>();
        }

        public string server_id { get; set; }
        public string server_name { get; set; }
        public string version { get; set; }
        public int proto { get; set; }
        public string git_commit { get; set; }
        public string go { get; set; }
        public string host { get; set; }
        public int port { get; set; }
        public bool auth_required { get; set; }
        public int max_connections { get; set; }
        public long ping_interval { get; set; }
        public int ping_max { get; set; }
        public string http_host { get; set; }
        public int http_port { get; set; }
        public int auth_timeout { get; set; }
        public int max_control_line { get; set; }
        public long max_payload { get; set; }
        public long max_pending { get; set; }
        //public ICollection<Cluster> clusters { get; set; }
        //public ICollection<Gateway> gateways { get; set; }
        //public ICollection<Leaf> leafs { get; set; }
        //public ICollection<Jetstream> jetstreams { get; set; }
        public int tls_timeout { get; set; }
        public long write_deadline { get; set; }
        //public DateTime start { get; set; } //TODO DateTime does not work here
        //public DateTime now { get; set; } //TODO DateTime does not work here
        //public DateTime uptime { get; set; } //TODO DateTime does not work here
        public long mem { get; set; }
        public int cores { get; set; }
        public int connections { get; set; }
        public long total_connections { get; set; }
        public int routes { get; set; }
        public int remotes { get; set; }
        public int leafnodes { get; set; }
        public long in_msgs { get; set; }
        public long out_msgs { get; set; }
        public long in_bytes { get; set; }
        public long out_bytes { get; set; }
        public int slow_consumers { get; set; }
        public int subscriptions { get; set; }
        
        // Connection list has Connection as their elements - each of these connections have a list of the actual 
        // connections. 
        public ICollection<Connection> connectionsList { get; set; }
        public ICollection<Route> routesList { get; set; }

        //public virtual Server Server { get; set; }
    }
}