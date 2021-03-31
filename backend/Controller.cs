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

        [HttpGet("/nodes")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Node>> GetNodes()
        {   
            return Program.instances[0].processedServers;
        }

        [HttpGet("/links")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Link>> GetLinks()
        {   
            return Program.instances[0].links;
        }

        [HttpGet("/varz")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Server>> GetVarz()
        {   
            var instance = (from entry in Program.instances
                where entry.id == 0
                select entry).FirstOrDefault();

            return instance.servers;
        }

        // TODO use to get info about server, when it is clicked in UI
        [HttpGet("/varz/{serverId}")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<Server> GetVarz([FromRoute] string serverId)
        {   
            var instance = Program.instances[0];

            var query = from server in instance.servers.AsParallel()
                where server.server_id == serverId
                select server;
            return query.FirstOrDefault();
        }

        [HttpGet("/connz")] //The route to the endpoint. Etc. localhost:5001/connz
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Connection>> GetConnz()
        {   
            return Program.instances[0].connections;
        }

        [HttpGet("/routez")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Route>> GetRoutez()
        {   
            return Program.instances[0].routes;
        }
        
        [HttpGet("/gatewayz")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Gateway>> GetGatewayz()
        {   
            return Program.instances[0].gateways;
        }

        [HttpGet("/leafz")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Leaf>> GetLeafz()
        {   
            return Program.instances[0].leafs;
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
