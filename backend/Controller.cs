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
        private static HashSet<string> missingServerIds = new HashSet<string>();


        [HttpGet("/nodes")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<ServerNode>> GetNodes()
        {   
            return processedServers;
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
            // Add serverNodes to processedServers
            Parallel.ForEach(Program.servers, server => {
                processedServers.Add(new ServerNode {
                    server_id = server.server_id, 
                    server_name = server.server_name, 
                    ntv_error = false,
                    clients = new ConcurrentBag<ConnectionNode>()
                });
            });

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

            var alreadyProcessedServers = new ConcurrentBag<string>();




            //     var processedServer = processedServers.Where(p => p.server_id == gateway.server_id).Select(p => p).FirstOrDefault();
            //     if (!(processedServer is null)) 
            //     {
            //         if (processedClusters.ContainsKey(gateway.name))
            //         {
            //             processedClusters[gateway.name].servers.Add(processedServer);
            //         }
            //         else
            //         {
            //             processedClusters.TryAdd(
            //                 gateway.name,
            //                 new ClusterNode {
            //                     name = gateway.name, 
            //                     servers = new ConcurrentBag<ServerNode>(
            //                         new []{processedServer}
            //                     )
            //                 }
            //             );
            //         }
            //     }
                
            // });
            // processedClustersAsList = (List<ClusterNode>)processedClusters.Values;


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
                    missingServerIds.Add(link.target);
                    link.ntv_error = true;
                    links.Add(link);
                }
            }

            // Patch for a missing node from varz
            // TODO dynamically handle these types of errors
            foreach(var serverId in missingServerIds)
            {
                processedServers.Add(new ServerNode{server_id = serverId, ntv_error = true});
            }


        }

    }
}
