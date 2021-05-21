using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using backend.drawables;
using backend.models;

namespace backend
{
    public class DataTransfer
    {
        public ConcurrentBag<ServerNode> processedServers {get; set;}
        public List<Link> links {get; set;}
        public ConcurrentBag<ClusterNode> processedClusters {get; set;}
        public List<GatewayLink> gatewayLinks {get; set;}
        public List<LeafLink> leafLinks {get; set;}
        public List<Server> varz {get; set;}
        public List<TreeNode> treeNodes { get; set; }
        public DateTime timeOfRequest {get; set;}
    }
}
