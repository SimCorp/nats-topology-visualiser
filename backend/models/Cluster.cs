using System.Collections.Generic;

namespace backend.models
{
    public class Cluster
    {
        public string name { get; set; }
        public string addr { get; set; }
        public int cluster_port { get; set; }
        public int auth_timeout { get; set; }
        public ICollection<string> urls { get; set; }
        public int tls_timeout { get; set; }
    }
}