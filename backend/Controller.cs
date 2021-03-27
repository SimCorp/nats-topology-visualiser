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
        private readonly DataStorage _dataStorage;

        public Controller(DataStorage dataStorage)
        {
            _dataStorage = dataStorage;
        }


        [HttpGet]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Server>> GetVarz()
        {   
            return _dataStorage.servers;
        }

        [HttpGet("/connz")] //The route to the endpoint. Etc. localhost:5001/connz
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Connection>> GetConnz()
        {   
            return _dataStorage.connections;
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

    }
}