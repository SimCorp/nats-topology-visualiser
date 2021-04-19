using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.StatusCodes;
using backend.models;
using System.Linq;
using backend.drawables;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace backend
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class Controller : ControllerBase
    {

        private static ConcurrentBag<Link> links = new ConcurrentBag<Link>();
        private static ConcurrentBag<ServerNode> processedServers = new ConcurrentBag<ServerNode>();
        private static ConcurrentBag<ClusterNode> processedClusters = new ConcurrentBag<ClusterNode>();
        private static Dictionary<string, List<string>> serverToMissingServer = new Dictionary<string, List<string>>();
        private static HashSet<string> missingServerIds = new HashSet<string>();
        private static HashSet<string> foundServers = new HashSet<string>();
        private static Dictionary<string, string> serverToCluster = new Dictionary<string, string>();
        private static Dictionary<string, List<string>> clusterConnectionErrors = new Dictionary<string, List<string>>();
        private static List<ClusterNode> errorClusters = new List<ClusterNode>();

        private static String timeOfRequest;

        [HttpGet("/nodes")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<ServerNode>> GetNodes()
        {   
            return processedServers;
        }
        
        [HttpGet("/gatewayerrors")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<Dictionary<string, List<string>>> GetGatewayErrors()
        {   
            return clusterConnectionErrors;
        }

        [HttpGet("/clusters")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<ClusterNode>> GetClusters()
        {   
            return processedClusters;
        }
        
        [HttpGet("/timeOfRequest")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<String> GetTimeOfRequest()
        {
            timeOfRequest = Program.dateOfNatsRequest.ToString("hh:mm tt - dd MMMM yyyy");
            return timeOfRequest;
        }

        [HttpGet("/gatewayLinks")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Link>> GetGatewayLinks()
        {   
            
            var gatewayLinks = new List<Link> {
                new Link ("bb1", "bb2", true),
                new Link ("bb1", "bb3", true),
                new Link ("bb2", "bb3", true),
                new Link("ehkd1", "ehkd2", false)
            };
            return gatewayLinks;
        }

        [HttpGet("/varz")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Server>> GetVarz()
        {   
            return Program.servers;
        }

        // TODO use to get info about server, when it is clicked in UI
        [HttpGet("/varz/{serverId}")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<Server> GetVarz([FromRoute] string serverId)
        {   
            var query = from server in Program.servers.AsParallel()
                where server.server_id == serverId
                select server;
            return query.FirstOrDefault();
        }

        [HttpGet("/connz")] //The route to the endpoint. Etc. localhost:5001/connz
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Connection>> GetConnz()
        {   
            return Program.connections;
        }
        
        [HttpGet("/links")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Link>> GetLinks()
        {   
            return links;
        }

        [HttpGet("/routez")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Route>> GetRoutez()
        {   
            return Program.routes;
        }
        
        [HttpGet("/gatewayz")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Gateway>> GetGatewayz()
        {   
            return Program.gateways;
        }

        [HttpGet("/leafz")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Leaf>> GetLeafz()
        {   
            return Program.leafs;
        }

        public static void ProcessData() 
        {
            
            ProcessServers();
            ProcessClusters();
            ProcessLinks();

            // Patch for a missing node from varz
            // TODO dynamically handle these types of errors
            foreach(var entry in serverToMissingServer)
            {
                var source = entry.Key;
                foreach (var target in entry.Value)
                {
                    var node = new ServerNode{
                        server_id = target, 
                        ntv_error = true
                    };
                    var cluster = processedClusters.Where(c => c.ContainsServer(source)).Select(c => c).FirstOrDefault();
                    if (cluster is null) continue;
                    if (cluster.ContainsServer(target)) continue;
                    processedServers.Add(node);
                    cluster.servers.Add(node);
                }
            }

            foreach(var cluster in processedClusters) {
                foreach (var server in cluster.servers)
                {
                    server.ntv_cluster = cluster.name;
                }
            }
        }

        public static void ProcessClusters()
        {
            // TODO crashed node is not in cluster, decide whether it should be.
            var markedServers = new HashSet<string>();
            var id = 0;
            foreach (var server in Program.routes)
            {
                if (markedServers.Contains(server.server_id)) continue; // Probably only once for each cluster

                var cluster = new ClusterNode {
                    name = "cluster nr:" + id,
                    servers = new ConcurrentBag<ServerNode>()
                };
                id++;

                ServerNode processedServer = processedServers.Where(p => p.server_id == server.server_id).Select(p => p).FirstOrDefault();
                if (processedServer is null) continue;
                cluster.servers.Add(processedServer);
                markedServers.Add(server.server_id);

                foreach (var route in server.routes)
                {
                    if (markedServers.Contains(route.remote_id)) continue;
                    markedServers.Add(route.remote_id);

                    processedServer = processedServers.Where(p => p.server_id == route.remote_id).Select(p => p).FirstOrDefault();
                    if (processedServer is null) continue;
                    cluster.servers.Add(processedServer);
                    markedServers.Add(server.server_id);
                };

                processedClusters.Add(cluster);
            }

            constructClustersOfBrokenGateways();
            
            detectGatewaysToCrashedServers();
            foreach (var connection in clusterConnectionErrors)
            {
                var tuple = stringToTuple(connection.Key);
                var source = tuple.Item1;
                var target = tuple.Item2;
                foreach (var gateway in Program.gateways)
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

        public static string clusterTupleString(string cluster1, string cluster2)
        {
            return cluster1 + " NAMESPLIT " + cluster2;
        }

        public static (string, string) stringToTuple (string input)
        {
            var split = input.Split(" NAMESPLIT ");
            return (split[0], split[1]);
        }

        public static void detectGatewaysToCrashedServers() {
            foreach (var gateway in Program.gateways)
            {
                if (gateway.name is null) continue;
                foreach (var outbound in gateway.outbound_gateways)
                {
                    if (!clusterConnectionErrors.ContainsKey(clusterTupleString(gateway.name, outbound.Key)) && !clusterConnectionErrors.ContainsKey(clusterTupleString(outbound.Key, gateway.name)))
                    {
                        clusterConnectionErrors.Add(clusterTupleString(gateway.name, outbound.Key), new List<string>());
                    }
                    if (!Program.idToServer.ContainsKey(outbound.Value.connection.name))
                    {
                        clusterConnectionErrors[clusterTupleString(gateway.name, outbound.Key)].Add("Outbound gateway to crashed server. From " + gateway.server_id + " to " + outbound.Value.connection.name);
                    }
                }
                foreach (var inbound in gateway.inbound_gateways)
                {
                    if (!clusterConnectionErrors.ContainsKey(clusterTupleString(gateway.name, inbound.Key)) && !clusterConnectionErrors.ContainsKey(clusterTupleString(inbound.Key, gateway.name)))
                    {
                        clusterConnectionErrors.Add(clusterTupleString(gateway.name, inbound.Key), new List<string>());
                    }
                    foreach (var inboundEntry in inbound.Value)
                    {
                        if (!Program.idToServer.ContainsKey(inboundEntry.connection.name))
                        {
                            clusterConnectionErrors[clusterTupleString(gateway.name, inbound.Key)].Add("Inbound gateway to crashed server. To " + gateway.server_id + " from " + inboundEntry.connection.name);
                        }
                        
                    }
                }
            }
        }

        public static void constructClustersOfBrokenGateways()
        {
            foreach (var gateway in Program.gateways)
            {
                foreach (var inbound in gateway.inbound_gateways)
                {
                    var clusterName = inbound.Key;
                    foreach (var server in inbound.Value)
                    {
                        if (!foundServers.Contains(server.connection.name))
                        {
                            var clusterToAddTo = errorClusters.Where(c => c.name == clusterName).Select(c => c).FirstOrDefault();
                            

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
                                errorClusters.Add(newCluster);
                            }
                            else 
                            {
                                clusterToAddTo.servers.Add(node);
                            }

                            processedServers.Add(node);
                            foundServers.Add(server.connection.name);
                        }
                    }
                }

                foreach (var outbound in gateway.outbound_gateways)
                {
                    var clusterName = outbound.Key;
                    if (!foundServers.Contains(outbound.Value.connection.name))
                    {
                        var clusterToAddTo = errorClusters.Where(c => c.name == clusterName).Select(c => c).FirstOrDefault();
                            
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
                                errorClusters.Add(newCluster);
                            }
                            else 
                            {
                                clusterToAddTo.servers.Add(node);
                            }
                            processedServers.Add(node);
                            foundServers.Add(outbound.Value.connection.name);
                    }
                }

                foreach (var cluster in processedClusters)
                {
                    if (!cluster.ContainsServer(gateway.server_id)) continue;
                    if (gateway.name is null) continue;
                    cluster.name = gateway.name;
                }
            }
            foreach (var cluster in errorClusters)
            {
                processedClusters.Add(cluster);
            }
        }

        public static void ProcessLinks()
        {
            // Information about routes are also on server, no request to routez necessary
            // Maybe info on routes is more up-to-date?
            Parallel.ForEach(Program.servers, server => {
                var source = server.server_id;
                Parallel.ForEach(server.route.routes, route => {
                    var target = route.remote_id;
                    links.Add(new Link(source, target));
                });
            });

            foreach (var link in links) 
            {
                if (!Program.idToServer.ContainsKey(link.target))
                {
                    link.ntv_error = true;
                    links.Add(link);
                    if (serverToMissingServer.ContainsKey(link.source))
                    {
                        serverToMissingServer[link.source].Add(link.target);
                    }
                    else
                    {
                        var list = new List<string>();
                        list.Add(link.target);
                        serverToMissingServer.Add(link.source, list);
                    }
                }
            }
        }
        public static void ProcessServers()
        {
            // Add serverNodes to processedServers
            foreach (var server in Program.servers) {
                foundServers.Add(server.server_id);
                processedServers.Add(new ServerNode {
                    server_id = server.server_id, 
                    server_name = server.server_name, 
                    ntv_error = false,
                    clients = new ConcurrentBag<ConnectionNode>()
                });
            }

            // Add connections to servers
            Parallel.ForEach(Program.connections, server => {
                Parallel.ForEach(server.connections, connection => {
                    var processedServer = processedServers.Where(p => p.server_id == server.server_id).Select(p => p).FirstOrDefault();
                    if (processedServer != null) 
                    {
                        processedServer.clients.Add(new ConnectionNode{ip = connection.ip, port = connection.port});
                    }
                });
            });
        }

    }
}
