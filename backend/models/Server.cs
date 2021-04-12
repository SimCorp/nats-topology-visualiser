using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace backend.models
{
    public class Server
    {

        public string server_id { get; set; }
        public string server_name { get; set; }
        public string version { get; set; }
        public int proto { get; set; }
        public string git_commit { get; set; }
        public string go { get; set; }
        public string host { get; set; }
        public int port { get; set; }
        public bool auth_required { get; set; }
        public ICollection<string> connect_urls { get; set; } 
        public int max_connections { get; set; }
        public long ping_interval { get; set; }
        public int ping_max { get; set; }
        public string http_host { get; set; }
        public int http_port { get; set; }
        public string http_base_path { get; set; }
        public int https_port { get; set; }
        public int auth_timeout { get; set; }
        public int max_control_line { get; set; }
        public long max_payload { get; set; }
        public long max_pending { get; set; }
        public Cluster cluster { get; set; }
        public Gateway gateway { get; set; }
        public Route route { get; set; }
        public Connection connection { get; set; }
        public Jetstream jetstream { get; set; }
        public Leaf leaf { get; set; }
        public int tls_timeout { get; set; }
        public long write_deadline { get; set; }
        public string start { get; set; } 
        public string now { get; set; } 
        public string uptime { get; set; }
        public long mem { get; set; }
        public int cores { get; set; }
        public int gomaxprocs { get; set; }
        public int cpu { get; set; }
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
        public Dictionary<string,int> http_req_stats { get; set; }

        // Connection list has Connection as their elements - each of these connections have a list of the actual 
        // connections. 

        //public virtual Server Server { get; set; }
    }

    public class Jetstream
    {
        public Config config {get; set;}
        public string stats {get; set;}
    }

    public class Config
    {
        public int max_memory {get; set;}
        public int max_storage {get; set;}
    }
}