using backend.models;
using backend.drawables;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace backend
{
    public class DataStorage
    {

        public ConcurrentDictionary<string, Server> idToServer;
        public List<Server> servers;
        public ConcurrentBag<backend.models.Connection> connections;
        public ConcurrentBag<Route> routes;
        public ConcurrentBag<Gateway> gateways;
        public List<Leaf> leafs;
        public List<Link> links;
        public ConcurrentBag<ServerNode> processedServers;
        public ConcurrentBag<ClusterNode> processedClusters;
        public Dictionary<string, List<string>> serverToMissingServer;

        public HashSet<string> missingServerIds = new HashSet<string>();
        public HashSet<string> foundServers = new HashSet<string>();
        public Dictionary<string, string> serverToCluster = new Dictionary<string, string>();
        public Dictionary<string, List<string>> clusterConnectionErrors = new Dictionary<string, List<string>>();
        public List<ClusterNode> errorClusters = new List<ClusterNode>();
        public Dictionary<string, string> ipToServerId;
        public List<LeafLink> leafLinks;
        public HashSet<string> leafConnections;

        public DataStorage() {

            idToServer = new ConcurrentDictionary<string, Server>();
            servers = new List<Server>();
            connections = new ConcurrentBag<backend.models.Connection>();
            routes = new ConcurrentBag<Route>();
            gateways = new ConcurrentBag<Gateway>();
            leafs = new List<Leaf>();

            links = new List<Link>();
            processedServers = new ConcurrentBag<ServerNode>();
            processedClusters = new ConcurrentBag<ClusterNode>();
            serverToMissingServer = new Dictionary<string, List<string>>();
            ipToServerId = new Dictionary<string, string>();
            leafLinks = new List<LeafLink>();
            

            missingServerIds = new HashSet<string>();
            foundServers = new HashSet<string>();
            serverToCluster = new Dictionary<string, string>();
            clusterConnectionErrors = new Dictionary<string, List<string>>();
            errorClusters = new List<ClusterNode>();
            leafConnections = new HashSet<string>();
        }
    }
}
