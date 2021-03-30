using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.StatusCodes;
using System.Collections.Concurrent;
using backend.models;
using System.Linq;

namespace backend
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class Controller : ControllerBase
    {

        private static ConcurrentBag<Link> links = new ConcurrentBag<Link>();
        private static ConcurrentBag<Node> processedServers = new ConcurrentBag<Node>();
        private static HashSet<string> missingServerIds = new HashSet<string>();


        [HttpGet("/nodes")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Node>> GetNodes()
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
            Parallel.ForEach(Program.servers, server => {
                processedServers.Add(new Node {server_id = server.server_id, server_name = server.server_name, ntv_error = false });
            });

            // Information about routes are also on server, no request to routez necessary
            Parallel.ForEach(Program.servers, server => {
                var source = server.server_id;
                Parallel.ForEach(server.route.routes, route => {
                    var target = route.remote_id;
                    links.Add(new Link(source, target));
                });
            });

            foreach (var link in links) 
            {
                // This may not be necessary
                if (!Program.idToServer.ContainsKey(link.source))
                {
                    missingServerIds.Add(link.source);
                    link.ntv_error = true;
                    links.Add(link);
                }
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
                processedServers.Add(new Node{server_id = serverId, ntv_error = true});
            }


        }

    }

    public class Link
    {
        public string source { get; }
        public string target { get; }

        public bool ntv_error { get; set; }

        public Link(string source, string target, bool ntv_error = false)
        {
            this.source = source;
            this.target = target;
            this.ntv_error = ntv_error;
        }
    }

    public class Node 
    {
        public string server_id { get; set; }
        public string server_name { get; set; }
        public bool ntv_error { get; set; }
    }
}
