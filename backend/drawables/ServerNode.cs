using System.Collections.Concurrent;

namespace backend.drawables
{
    public class ServerNode 
    {
        public string server_id { get; set; }
        public string server_name { get; set; }
        public bool ntv_error { get; set; }
        public string ntv_cluster { get; set; }

        public ConcurrentBag<ConnectionNode> clients { get; set; }
    }
}