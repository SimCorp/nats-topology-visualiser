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

        public DrawablesProcessor(DataStorage dataStorage){
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

            // Patch for a missing node from varz
            // TODO dynamically handle these types of errors
            foreach(var entry in _dataStorage.serverToMissingServer)
            {
                var source = entry.Key;
                foreach (var target in entry.Value)
                {
                    var node = new ServerNode {
                        server_id = target,
                        server_name = "Unknown name",
                        ntv_error = true
                    };
                    var cluster = _dataStorage.processedClusters.Where(c => c.ContainsServer(source)).Select(c => c).FirstOrDefault();
                    if (cluster is null) continue;
                    if (cluster.ContainsServer(target)) continue;
                    _dataStorage.processedServers.Add(node);
                    cluster.servers.Add(node);
                }
            }

            foreach(var cluster in _dataStorage.processedClusters) {
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
            foreach (var server in _dataStorage.leafs)
            {
                if (server.leafs is null) continue;
                foreach (var leaf in server.leafs)
                {
                    // TODO detection of errors
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
                        if (!identicalLink.accountToBidirectional.ContainsKey(leaf.account))
                        {
                            identicalLink.accountToBidirectional.Add(leaf.account, false);
                        }
                        else 
                        {
                            identicalLink.accountToBidirectional[leaf.account] = true;
                        }
                    }
                    else if (oppositeLink is null)
                    {
                        var link = new Link (
                            server.server_id,
                            targetId
                        );
                        link.accountToBidirectional.Add(leaf.account, false);
                        _dataStorage.leafLinks.Add(link);
                    }
                    else
                    {
                        if (!oppositeLink.accountToBidirectional.ContainsKey(leaf.account))
                        {
                            oppositeLink.accountToBidirectional.Add(leaf.account, false);
                        }
                        else 
                        {
                            oppositeLink.accountToBidirectional[leaf.account] = true;
                        }
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

                var cluster = new ClusterNode {
                    name = "cluster nr:" + id,
                    servers = new ConcurrentBag<ServerNode>()
                };
                id++;

                ServerNode processedServer = _dataStorage.processedServers.Where(p => p.server_id == server.server_id).Select(p => p).FirstOrDefault();
                if (processedServer is null) continue;
                cluster.servers.Add(processedServer);
                markedServers.Add(server.server_id);

                foreach (var route in server.routes)
                {
                    if (markedServers.Contains(route.remote_id)) continue;
                    markedServers.Add(route.remote_id);

                    processedServer = _dataStorage.processedServers.Where(p => p.server_id == route.remote_id).Select(p => p).FirstOrDefault();
                    if (processedServer is null) continue;
                    cluster.servers.Add(processedServer);
                    markedServers.Add(server.server_id);
                };

                _dataStorage.processedClusters.Add(cluster);
            }

            constructClustersOfBrokenGateways();
            
            detectGatewaysToCrashedServers();
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
                    if (!gateway.outbound_gateways.ContainsKey(target) /*|| !gateway.outbound_gateways.ContainsKey(source)*/)
                    { // TODO tjek hvilken cluster det er til og fra
                        connection.Value.Add("Missing gateway from cluster " + gateway.name + " to cluster " + target + " for server " + gateway.server_id);
                    }
                }
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
            foreach (var server in _dataStorage.servers) {
                _dataStorage.foundServers.Add(server.server_id);
                _dataStorage.processedServers.Add(new ServerNode {
                    server_id = server.server_id, 
                    server_name = server.server_name, 
                    ntv_error = false,
                    clients = new ConcurrentBag<ConnectionNode>()
                });
            }

            // Add connections to servers
            Parallel.ForEach(_dataStorage.connections, server => {
                Parallel.ForEach(server.connections, connection => {
                    var processedServer = _dataStorage.processedServers.Where(p => p.server_id == server.server_id).Select(p => p).FirstOrDefault();
                    if (processedServer != null) 
                    {
                        processedServer.clients.Add(new ConnectionNode{ip = connection.ip, port = connection.port});
                    }
                });
            });
        }
        
        public string clusterTupleString(string cluster1, string cluster2)
        {
            return cluster1 + " NAMESPLIT " + cluster2;
        }

        public (string, string) stringToTuple (string input)
        {
            var split = input.Split(" NAMESPLIT ");
            return (split[0], split[1]);
        }

        public void detectGatewaysToCrashedServers() {
            foreach (var gateway in _dataStorage.gateways)
            {
                if (gateway.name is null) continue;
                foreach (var outbound in gateway.outbound_gateways)
                {
                    if (!_dataStorage.clusterConnectionErrors.ContainsKey(clusterTupleString(gateway.name, outbound.Key)) && !_dataStorage.clusterConnectionErrors.ContainsKey(clusterTupleString(outbound.Key, gateway.name)))
                    {
                        _dataStorage.clusterConnectionErrors.Add(clusterTupleString(gateway.name, outbound.Key), new List<string>());
                    }
                    if (!_dataStorage.idToServer.ContainsKey(outbound.Value.connection.name))
                    {
                        _dataStorage.clusterConnectionErrors[clusterTupleString(gateway.name, outbound.Key)].Add("Outbound gateway to crashed server. From " + gateway.server_id + " to " + outbound.Value.connection.name);
                    }
                }
                foreach (var inbound in gateway.inbound_gateways)
                {
                    if (!_dataStorage.clusterConnectionErrors.ContainsKey(clusterTupleString(gateway.name, inbound.Key)) && !_dataStorage.clusterConnectionErrors.ContainsKey(clusterTupleString(inbound.Key, gateway.name)))
                    {
                        _dataStorage.clusterConnectionErrors.Add(clusterTupleString(gateway.name, inbound.Key), new List<string>());
                    }
                    foreach (var inboundEntry in inbound.Value)
                    {
                        if (!_dataStorage.idToServer.ContainsKey(inboundEntry.connection.name))
                        {
                            _dataStorage.clusterConnectionErrors[clusterTupleString(gateway.name, inbound.Key)].Add("Inbound gateway to crashed server. To " + gateway.server_id + " from " + inboundEntry.connection.name);
                        }
                        
                    }
                }
            }
        }

        public void detectLeafErrors ()
        {
            foreach (var leafLink in _dataStorage.leafLinks)
            {
                if (! _dataStorage.leafConnectionErrors.ContainsKey(clusterTupleString(leafLink.source, leafLink.target)))
                {
                    if (! _dataStorage.leafConnectionErrors.ContainsKey(clusterTupleString(leafLink.target, leafLink.source)))
                    {
                        _dataStorage.leafConnectionErrors.Add(clusterTupleString(leafLink.source, leafLink.target), new List<string>()); //<-- only one leaf connection is shown
                        continue;
                    }
                }

                if (! _dataStorage.leafConnectionErrors.ContainsKey(clusterTupleString(leafLink.target, leafLink.source)))
                {
                    if (! _dataStorage.leafConnectionErrors.ContainsKey(clusterTupleString(leafLink.source, leafLink.target)))
                    {
                        _dataStorage.leafConnectionErrors.Add(clusterTupleString(leafLink.target, leafLink.source), new List<string>()); //<-- only one leaf connection is shown
                     
                        continue;
                    }
                }

                // if (_dataStorage.leafConnectionErrors.ContainsKey(clusterTupleString(leafLink.source, leafLink.target)))
                // {
                    
                // }

                //_dataStorage.leafConnectionErrors[clusterTupleString(leafLink.source, leafLink.target)].
                //TODO: detect how leafs connect (in order to get arrow-direction?)
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
                            var clusterToAddTo = _dataStorage.errorClusters.Where(c => c.name == clusterName).Select(c => c).FirstOrDefault();
                            

                            var node = new ServerNode {
                                server_id = server.connection.name,
                                server_name = "Unknown name",
                                ntv_error = true,
                                clients = new ConcurrentBag<ConnectionNode>()
                            };

                            if (clusterToAddTo is null)
                            {
                                var newCluster = new ClusterNode {
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
                        var clusterToAddTo = _dataStorage.errorClusters.Where(c => c.name == clusterName).Select(c => c).FirstOrDefault();
                            
                            var node = new ServerNode {
                                server_id = outbound.Value.connection.name,
                                server_name = "Unknown name",
                                ntv_error = true,
                                clients = new ConcurrentBag<ConnectionNode>()
                            };

                            if (clusterToAddTo is null)
                            {
                                var newCluster = new ClusterNode {
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
    }
}