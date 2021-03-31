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
}
