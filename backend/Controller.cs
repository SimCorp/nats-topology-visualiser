using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.StatusCodes;
using System.Collections.Concurrent;
using backend.models;

namespace backend
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class Controller : ControllerBase
    {

        private static ConcurrentBag<Link> links = new ConcurrentBag<Link>();
        private static ConcurrentBag<ServerErrorWrapper> processedServers = new ConcurrentBag<ServerErrorWrapper>();
        private static HashSet<string> missingServerIds = new HashSet<string>();


        [HttpGet]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<ServerErrorWrapper>> GetVarz()
        {   
            return processedServers;
        }

        [HttpGet("/connz")] //The route to the endpoint. Etc. localhost:5001/connz
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Connection>> GetConnz()
        {   
            return Program.connections;
        }
        
        [HttpGet("/routez")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Link>> GetRoutez()
        {   
            return links;
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

        public static void processData() 
        {
            Parallel.ForEach(Program.servers, server => {
                processedServers.Add(new ServerErrorWrapper(server));
            });

            Parallel.ForEach(Program.routes, server => {
                var source = server.server_id;
                Parallel.ForEach(server.routes, route => {
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
                processedServers.Add(new ServerErrorWrapper(new Server{server_id = serverId}, true));
            }


        }

    }

    public class Link
    {
        public string source {get;}
        public string target {get;}

        public bool ntv_error {get; set;}

        public Link(string source, string target, bool ntv_error = false)
        {
            this.source = source;
            this.target = target;
            this.ntv_error = ntv_error;
        }
    }

    public class ServerErrorWrapper
    {
        public Server server {get;}
        public bool ntv_error {get;}

        public ServerErrorWrapper(Server server, bool ntv_error = false)
        {
            this.server = server;
            this.ntv_error = ntv_error;
        }

    }
}