using System;
using backend.drawables;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace backend
{
    public class DrawablesProcessor
    {
        private DataStorage _dataStorage;

        public DrawablesProcessor(DataStorage dataStorage)
        {
            _dataStorage = dataStorage;

            ProcessData();
        }

        // For testing purposes
        public DrawablesProcessor(DataStorage dataStorage, string test)
        {
            _dataStorage = dataStorage;
        }

        public void ProcessData()
        {

            ProcessServers();
            ProcessClusters();
            ProcessLinks();
            ProcessLeafs();
            ProcessTreeNode();

            // Patch for a missing node from varz
            // TODO dynamically handle these types of errors
            foreach (var entry in _dataStorage.serverToMissingServer)
            {
                var source = entry.Key;
                foreach (var target in entry.Value)
                {
                    var node = new ServerNode
                    {
                        server_id = target,
                        server_name = "Unknown name",
                        ntv_error = true
                    };
                    var cluster = _dataStorage.processedClusters.Where(c => c.ContainsServer(source)).Select(c => c)
                        .FirstOrDefault();
                    if (cluster is null) continue;
                    if (cluster.ContainsServer(target)) continue;
                    _dataStorage.processedServers.Add(node);
                    cluster.servers.Add(node);
                }
            }

            foreach (var cluster in _dataStorage.processedClusters)
            {
                foreach (var server in cluster.servers)
                {
                    server.ntv_cluster = cluster.name;
                }
            }
        }

        public void ProcessLeafs()
        {

            var leafIps = new HashSet<string>();
            foreach (var entry in _dataStorage.leafs)
            {
                if (entry.leafs is null) continue;
                foreach (var leaf in entry.leafs)
                {
                    leafIps.Add(leaf.ip);
                }
            }

            foreach (var entry in _dataStorage.routes)
            {
                foreach (var route in entry.routes)
                {
                    if (_dataStorage.ipToServerId.ContainsKey(route.ip)) continue;
                    if (leafIps.Contains(route.ip))
                    {
                        _dataStorage.ipToServerId.Add(route.ip, route.remote_id);
                    }
                }
            }

            ConstructSingleLeafConnections();
        }

        public void ConstructSingleLeafConnections()
        {
            // TODO functionality about bidirectionality is currently not being used. Should maybe be removed.
            foreach (var server in _dataStorage.leafs)
            {
                if (server.leafs is null) continue;
                foreach (var leaf in server.leafs)
                {
                    // TODO leafs to unknown servers are not handled
                    if (!_dataStorage.ipToServerId.ContainsKey(leaf.ip)) continue;
                    var targetId = _dataStorage.ipToServerId[leaf.ip];

                    var oppositeLink = _dataStorage.leafLinks.Where(l =>
                        l.source == targetId &&
                        l.target == server.server_id
                    ).Select(l => l).FirstOrDefault();

                    var identicalLink = _dataStorage.leafLinks.Where(l =>
                        l.target == targetId &&
                        l.source == server.server_id
                    ).Select(l => l).FirstOrDefault();

                    if (identicalLink is not null)
                    {
                        identicalLink.connections.Add(leaf); // Add leafnode to be fetched when link is clicked
                    }
                    else if (oppositeLink is null)
                    {
                        var link = new LeafLink(
                            server.server_id,
                            targetId
                        );
                        link.connections.Add(leaf);
                        _dataStorage.leafLinks.Add(link);
                    }
                    else
                    {
                        oppositeLink.connections.Add(leaf);
                    }

                }
            }
        }

        public void ProcessClusters()
        {
            // TODO crashed node is not in cluster, decide whether it should be.
            var markedServers = new HashSet<string>();
            var id = 0;
            foreach (var server in _dataStorage.routes)
            {
                if (markedServers.Contains(server.server_id)) continue; // Probably only once for each cluster

                var cluster = new ClusterNode
                {
                    name = "cluster nr:" + id,
                    servers = new ConcurrentBag<ServerNode>()
                };
                id++;

                ServerNode processedServer = _dataStorage.processedServers.Where(p => p.server_id == server.server_id)
                    .Select(p => p).FirstOrDefault();
                if (processedServer is null) continue;
                cluster.servers.Add(processedServer);
                markedServers.Add(server.server_id);

                foreach (var route in server.routes)
                {
                    if (markedServers.Contains(route.remote_id)) continue;
                    markedServers.Add(route.remote_id);

                    processedServer = _dataStorage.processedServers.Where(p => p.server_id == route.remote_id)
                        .Select(p => p).FirstOrDefault();
                    if (processedServer is null) continue;
                    cluster.servers.Add(processedServer);
                    markedServers.Add(server.server_id);
                }

                ;

                _dataStorage.processedClusters.Add(cluster);
            }

            constructClustersOfBrokenGateways();

            detectGatewaysToCrashedServers();

            constructSingleGatewayLinks();
        }

        public void constructSingleGatewayLinks()
        {
            foreach (var connection in _dataStorage.clusterConnectionErrors)
            {
                var tuple = stringToTuple(connection.Key);
                var source = tuple.Item1;
                var target = tuple.Item2;
                foreach (var gateway in _dataStorage.gateways)
                {
                    if (gateway.name != source && gateway.name != target) continue;
                    if (source == target) continue;
                    if (gateway.name == target) continue;
                    if (!gateway.outbound_gateways
                        .ContainsKey(target) /*|| !gateway.outbound_gateways.ContainsKey(source)*/)
                    {
                        // TODO tjek hvilken cluster det er til og fra
                        connection.Value.Add("Missing gateway from cluster " + gateway.name + " to cluster " + target +
                                             " for server " + gateway.server_id);
                    }
                }
            }

            foreach (var cluster in _dataStorage.clusterConnectionErrors)
            {
                var split = cluster.Key.Split(" NAMESPLIT ");
                var source = split[0];
                var target = split[1];
                var link = new GatewayLink(source, target, cluster.Value.Count > 0);
                link.errors = cluster.Value;
                foreach (var err in cluster.Value)
                {
                    link.errorsAsString += "\n" + err;
                }

                _dataStorage.gatewayLinks.Add(link);
            }
        }

        public void ProcessLinks()
        {
            // Information about routes are also on server, no request to routez necessary
            // Maybe info on routes is more up-to-date?
            foreach (var server in _dataStorage.routes)
            {
                var source = server.server_id;
                foreach (var route in server.routes)
                {
                    var target = route.remote_id;
                    _dataStorage.links.Add(new Link(source, target));
                }
            }

            foreach (var link in _dataStorage.links)
            {
                if (!_dataStorage.idToServer.ContainsKey(link.target))
                {
                    link.ntv_error = true;
                    if (_dataStorage.serverToMissingServer.ContainsKey(link.source))
                    {
                        _dataStorage.serverToMissingServer[link.source].Add(link.target);
                    }
                    else
                    {
                        var list = new List<string>();
                        list.Add(link.target);
                        _dataStorage.serverToMissingServer.Add(link.source, list);
                    }
                }
            }
        }

        public void ProcessServers()
        {
            // Add serverNodes to processedServers
            foreach (var server in _dataStorage.servers)
            {
                _dataStorage.foundServers.Add(server.server_id);
                _dataStorage.processedServers.Add(new ServerNode
                {
                    server_id = server.server_id,
                    server_name = server.server_name,
                    ntv_error = false,
                    clients = new ConcurrentBag<ConnectionNode>()
                });
            }

            // Add connections to servers
            Parallel.ForEach(_dataStorage.connections, server =>
            {
                Parallel.ForEach(server.connections, connection =>
                {
                    var processedServer = _dataStorage.processedServers.Where(p => p.server_id == server.server_id)
                        .Select(p => p).FirstOrDefault();
                    if (processedServer != null)
                    {
                        processedServer.clients.Add(new ConnectionNode {ip = connection.ip, port = connection.port});
                    }
                });
            });
        }

        public string tupleString(string id1, string id2)
        {
            return id1 + " NAMESPLIT " + id2;
        }

        public (string, string) stringToTuple(string input)
        {
            var split = input.Split(" NAMESPLIT ");
            return (split[0], split[1]);
        }

        public void detectGatewaysToCrashedServers()
        {
            foreach (var gateway in _dataStorage.gateways)
            {
                if (gateway.name is null) continue;
                foreach (var outbound in gateway.outbound_gateways)
                {
                    if (!_dataStorage.clusterConnectionErrors.ContainsKey(tupleString(gateway.name, outbound.Key)) &&
                        !_dataStorage.clusterConnectionErrors.ContainsKey(tupleString(outbound.Key, gateway.name)))
                    {
                        _dataStorage.clusterConnectionErrors.Add(tupleString(gateway.name, outbound.Key),
                            new List<string>());
                    }

                    if (!_dataStorage.idToServer.ContainsKey(outbound.Value.connection.name))
                    {
                        _dataStorage.clusterConnectionErrors[tupleString(gateway.name, outbound.Key)].Add(
                            "Outbound gateway to crashed server. From " + gateway.server_id + " to " +
                            outbound.Value.connection.name);
                    }
                }

                foreach (var inbound in gateway.inbound_gateways)
                {
                    if (!_dataStorage.clusterConnectionErrors.ContainsKey(tupleString(gateway.name, inbound.Key)) &&
                        !_dataStorage.clusterConnectionErrors.ContainsKey(tupleString(inbound.Key, gateway.name)))
                    {
                        _dataStorage.clusterConnectionErrors.Add(tupleString(gateway.name, inbound.Key),
                            new List<string>());
                    }

                    foreach (var inboundEntry in inbound.Value)
                    {
                        if (!_dataStorage.idToServer.ContainsKey(inboundEntry.connection.name))
                        {
                            _dataStorage.clusterConnectionErrors[tupleString(gateway.name, inbound.Key)]
                                .Add("Inbound gateway to crashed server. To " + gateway.server_id + " from " +
                                     inboundEntry.connection.name);
                        }

                    }
                }
            }
        }

        public void detectLeafConnections()
        {
            foreach (var leafLink in _dataStorage.leafLinks)
            {
                if (!_dataStorage.leafConnections.Contains(tupleString(leafLink.source, leafLink.target)))
                {
                    if (!_dataStorage.leafConnections.Contains(tupleString(leafLink.target, leafLink.source)))
                    {
                        _dataStorage.leafConnections.Add(tupleString(leafLink.source,
                            leafLink.target)); //<-- only one leaf connection is shown
                        continue;
                    }
                }

                if (!_dataStorage.leafConnections.Contains(tupleString(leafLink.target, leafLink.source)))
                {
                    if (!_dataStorage.leafConnections.Contains(tupleString(leafLink.source, leafLink.target)))
                    {
                        _dataStorage.leafConnections.Add(tupleString(leafLink.target,
                            leafLink.source)); //<-- only one leaf connection is shown

                        continue;
                    }
                }
            }
        }

        public void constructClustersOfBrokenGateways()
        {
            foreach (var gateway in _dataStorage.gateways)
            {
                foreach (var inbound in gateway.inbound_gateways)
                {
                    var clusterName = inbound.Key;
                    foreach (var server in inbound.Value)
                    {
                        if (!_dataStorage.foundServers.Contains(server.connection.name))
                        {
                            var clusterToAddTo = _dataStorage.errorClusters.Where(c => c.name == clusterName)
                                .Select(c => c).FirstOrDefault();


                            var node = new ServerNode
                            {
                                server_id = server.connection.name,
                                server_name = "Unknown name",
                                ntv_error = true,
                                clients = new ConcurrentBag<ConnectionNode>()
                            };

                            if (clusterToAddTo is null)
                            {
                                var newCluster = new ClusterNode
                                {
                                    name = clusterName,
                                    servers = new ConcurrentBag<ServerNode>()
                                };
                                newCluster.servers.Add(node);
                                _dataStorage.errorClusters.Add(newCluster);
                            }
                            else
                            {
                                clusterToAddTo.servers.Add(node);
                            }

                            _dataStorage.processedServers.Add(node);
                            _dataStorage.foundServers.Add(server.connection.name);
                        }
                    }
                }

                foreach (var outbound in gateway.outbound_gateways)
                {
                    var clusterName = outbound.Key;
                    if (!_dataStorage.foundServers.Contains(outbound.Value.connection.name))
                    {
                        var clusterToAddTo = _dataStorage.errorClusters.Where(c => c.name == clusterName).Select(c => c)
                            .FirstOrDefault();

                        var node = new ServerNode
                        {
                            server_id = outbound.Value.connection.name,
                            server_name = "Unknown name",
                            ntv_error = true,
                            clients = new ConcurrentBag<ConnectionNode>()
                        };

                        if (clusterToAddTo is null)
                        {
                            var newCluster = new ClusterNode
                            {
                                name = clusterName,
                                servers = new ConcurrentBag<ServerNode>()
                            };
                            newCluster.servers.Add(node);
                            _dataStorage.errorClusters.Add(newCluster);
                        }
                        else
                        {
                            clusterToAddTo.servers.Add(node);
                        }

                        _dataStorage.processedServers.Add(node);
                        _dataStorage.foundServers.Add(outbound.Value.connection.name);
                    }
                }

                foreach (var cluster in _dataStorage.processedClusters)
                {
                    if (!cluster.ContainsServer(gateway.server_id)) continue;
                    if (gateway.name is null) continue;
                    cluster.name = gateway.name;
                }
            }

            foreach (var cluster in _dataStorage.errorClusters)
            {
                _dataStorage.processedClusters.Add(cluster);
            }
        }

        public void ProcessTreeNode()
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
                if (!idTo_supercluster.ContainsKey(uf.id[i]))
                {
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

                if (Supercluster.Count == 1) {
                    var ClusterCounter = counter+1;
                    foreach (var cluster in Supercluster)
                    {
                        var clusterTreeNode = new TreeNode
                        {
                            name = cluster.name,
                            id = ClusterCounter++,
                            pid = 0,
                            dragDisabled = true,
                            addTreeNodeDisabled = true,
                            addLeafNodeDisabled = true,
                            editNodeDisabled = true,
                            delNodeDisabled = true,
                            children = new List<TreeNode>(),
                        };

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
                        test.Add(clusterTreeNode);
                    }
                }
                else {


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
                _dataStorage.treeNodes = test;
            }
        }

        class UF
        {
            public int[] id;
            private int size;


            public UF(int n)
            {
                this.size = n;
                id = new int[n];
                for (int i = 0; i < n; i++)
                {
                    id[i] = i;
                }
            }

            public int find(int p)
            {
                return id[p];
            }

            public bool connected(int p, int q)
            {
                return id[p] == id[q];
            }

            public void union(int p, int q)
            {
                int pID = id[p];
                int qID = id[q];

                if (pID == qID)
                {
                    return;
                }

                for (int i = 0; i < this.size; i++)
                {
                    if (id[i] == pID)
                    {
                        id[i] = qID;
                    }
                }
            }
        }
    }
