using System.Collections.Concurrent;

namespace backend.drawables
{
    public class ClusterNode
    {
        public string name { get; set; }
        public ConcurrentBag<ServerNode> servers { get; set; }
    }
}