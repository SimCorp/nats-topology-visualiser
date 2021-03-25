using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.StatusCodes;
using backend.models;

namespace backend
{
    [ApiController]
    [Produces("application/json")]
    [Route("[controller]")]
    public class Controller : ControllerBase
    {
        //private readonly ISuperheroRepository _repository;

        public Controller()
        {
            //_repository = repository;
        }


        [HttpGet]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Server>> GetVarz()
        {   
            return Program.servers;
        }

        [HttpGet("/connz")] //The route to the endpoint. Etc. localhost:5001/connz
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Connection>> GetConnz()
        {   
            return Program.connections;
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

    }
}