using System.Collections.Generic;

namespace backend.drawables
{
    public class SuperClusterNode
    {
        public string account { get; set; }
        public ICollection<ClusterNode> clusters { get; set; }
    }
}