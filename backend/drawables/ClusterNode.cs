using System.Collections.Concurrent;

namespace backend.drawables
{
    public class ClusterNode
    {
        public string name { get; set; }
        public ConcurrentBag<ServerNode> servers { get; set; }

        public bool ContainsServer (string serverId)
        {
            foreach (var server in servers)
            {
                if (server.server_id == serverId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}