using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.StatusCodes;
using backend.models;
using System.Linq;
using backend.drawables;

namespace backend
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class Controller : ControllerBase
    {

        private readonly DataStorage _dataStorage;

        private static String timeOfRequest;

        public Controller(DataStorage dataStorage)
        {
            _dataStorage = dataStorage;
        }
        
        [HttpGet("/nodes")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<ServerNode>> GetNodes()
        {   
            return _dataStorage.processedServers;
        }
        
        [HttpGet("/gatewayerrors")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<Dictionary<string, List<string>>> GetGatewayErrors()
        {   
            return _dataStorage.clusterConnectionErrors;
        }

        [HttpGet("/clusters")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<ClusterNode>> GetClusters()
        {   
            return _dataStorage.processedClusters;
        }
        
        [HttpGet("/timeOfRequest")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<String> GetTimeOfRequest()
        {
            timeOfRequest = Program.dateOfNatsRequest.ToString("hh:mm tt - dd MMMM yyyy");
            return timeOfRequest;
        }

        [HttpGet("/iptoserverid")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<Dictionary<string, string>> GetIpToServerId()
        {
            return _dataStorage.ipToServerId;
        }

        [HttpGet("/leaflinks")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<List<LeafLink>> GetLeafLinks()
        {  
            return _dataStorage.leafLinks;
        }

        [HttpGet("/gatewayLinks")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<GatewayLink>> GetGatewayLinks()
        {   
            return _dataStorage.gatewayLinks;
        }

        [HttpGet("/varz")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Server>> GetVarz()
        {   
            return _dataStorage.servers;
        }

        // TODO use to get info about server, when it is clicked in UI
        [HttpGet("/varz/{serverId}")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<Server> GetVarz([FromRoute] string serverId)
        {   
            var query = from server in _dataStorage.servers.AsParallel()
                where server.server_id == serverId
                select server;
            return query.FirstOrDefault();
        }

        [HttpGet("/connz")] //The route to the endpoint. Etc. localhost:5001/connz
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Connection>> GetConnz()
        {   
            return _dataStorage.connections;
        }
        
        [HttpGet("/links")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Link>> GetLinks()
        {   
            return _dataStorage.links;
        }

        [HttpGet("/routez")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Route>> GetRoutez()
        {   
            return _dataStorage.routes;
        }
        
        [HttpGet("/gatewayz")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Gateway>> GetGatewayz()
        {   
            return _dataStorage.gateways;
        }

        [HttpGet("/leafz")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Leaf>> GetLeafz()
        {   
            return _dataStorage.leafs;
        }
        
        [HttpGet("/TreeViewData")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<TreeNode>> GetTreeViewData()
        {
            HashSet<string> used_servers = new HashSet<string>();

            List<TreeNode> test = new List<TreeNode>();
            

            Dictionary<string, int> cluster_toId = new Dictionary<string, int>();

            UF uf = new UF(_dataStorage.processedClusters.Count);

            var count = 0;
            foreach (var cluster in _dataStorage.processedClusters)
            {
                Console.WriteLine(cluster.name);
                cluster_toId.Add(cluster.name, count++);
            }

            foreach (var item in _dataStorage.gatewayLinks)
            {
                var p = cluster_toId[item.source];
                var q = cluster_toId[item.target];

                uf.union(p, q);
            }

            Dictionary<int, HashSet<ClusterNode>> idTo_supercluster = new Dictionary<int, HashSet<ClusterNode>>();

            var i = 0;
            foreach (var item in _dataStorage.processedClusters)
            {
                if (!idTo_supercluster.ContainsKey(uf.id[i])) {
                    idTo_supercluster.Add(uf.id[i], new HashSet<ClusterNode>());
                }

                var set = idTo_supercluster[uf.id[i]];
                set.Add(item);

                idTo_supercluster.Remove(uf.id[i]);

                idTo_supercluster.Add(uf.id[i], set);
                i++;
            }
                
            var counter = 1;
            foreach (var Supercluster in idTo_supercluster.Values)
            {
                var SuperClusterTreeNode = new TreeNode
                {
                    name = "supercluster " + counter,
                    id = counter++,
                    pid = 0,
                    dragDisabled = true,
                    addTreeNodeDisabled = true,
                    addLeafNodeDisabled = true,
                    editNodeDisabled = true,
                    delNodeDisabled = true,
                    children = new List<TreeNode>(),
                };
                test.Add(SuperClusterTreeNode);

                var ClusterCounter = counter+1;
                foreach (var cluster in Supercluster)
                {
                    var clusterTreeNode = new TreeNode
                    {
                        name = cluster.name,
                        id = ClusterCounter++,
                        pid = counter,
                        dragDisabled = true,
                        addTreeNodeDisabled = true,
                        addLeafNodeDisabled = true,
                        editNodeDisabled = true,
                        delNodeDisabled = true,
                        children = new List<TreeNode>(),
                    };
                    SuperClusterTreeNode.children.Add(clusterTreeNode);

                    var nodeServerCounter = ClusterCounter+1;
                    foreach (var server in cluster.servers)
                    {
                        used_servers.Add(server.server_id);
                        var serverTreeNode = new TreeNode
                        {
                            name = server.server_name ,
                            id = nodeServerCounter++,
                            server_id = server.server_id,
                            pid = ClusterCounter,
                            dragDisabled = true,
                            addTreeNodeDisabled = true,
                            addLeafNodeDisabled = true,
                            editNodeDisabled = true,
                            delNodeDisabled = true,
                            isLeaf = true,
                        };

                        clusterTreeNode.children.Add(serverTreeNode);
                    }
                }
            }

            //Process servers not in any clusters
            foreach (var server in _dataStorage.servers)
            {
                if(!used_servers.Contains(server.server_id)) {
                    var soloServerTreeNode = new TreeNode
                    {
                        name = server.server_name,
                        id = counter++,
                        server_id = server.server_id,
                        pid = 0,
                        dragDisabled = true,
                        addTreeNodeDisabled = true,
                        addLeafNodeDisabled = true,
                        editNodeDisabled = true,
                        delNodeDisabled = true,
                        isLeaf = true,
                    };
                    used_servers.Add(server.server_id);
                    test.Add(soloServerTreeNode);
                }
            }


            return test;
        }  
    }

     class UF {
        public int[] id;
        private int size;



        public UF(int n){
            this.size = n;
            id = new int[n];
            for (int i = 0; i < n; i++)
            {
                id[i] = i;
            }
        }

        public int find(int p) {
            return id[p];
        }

        public bool connected(int p, int q){
            return id[p] == id[q];
        }

        public void union(int p, int q){
            int pID = id[p];
            int qID = id[q];

            if (pID == qID){
                return;
            }

            for (int i = 0; i < this.size; i++){
                if (id[i] == pID) {
                    id[i] = qID;
                }
            }
        }

    }
}
