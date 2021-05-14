using System;
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

        private readonly DataStorage _dataStorage;

        public Controller(DataStorage dataStorage)
        {
            _dataStorage = dataStorage;
        }

        [HttpGet("/updateEverything")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<DataTransfer> GetData()
        {
            return new DataTransfer {
                processedServers = _dataStorage.processedServers,
                links = _dataStorage.links,
                processedClusters = _dataStorage.processedClusters,
                gatewayLinks = _dataStorage.gatewayLinks,
                leafLinks = _dataStorage.leafLinks,
                varz = _dataStorage.servers,
                timeOfRequest = Program.dateOfNatsRequest
            };
        }

        [HttpGet("/nodes")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<ServerNode>> GetNodes()
        {   
            return _dataStorage.processedServers;
        }
        
        [HttpGet("/gatewayerrors")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<Dictionary<string, List<string>>> GetGatewayErrors()
        {   
            return _dataStorage.clusterConnectionErrors;
        }

        [HttpGet("/clusters")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<ClusterNode>> GetClusters()
        {   
            return _dataStorage.processedClusters;
        }
        
        [HttpGet("/timeOfRequest")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<DateTime> GetTimeOfRequest()
        {
            return Program.dateOfNatsRequest;
        }

        [HttpGet("/iptoserverid")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<Dictionary<string, string>> GetIpToServerId()
        {
            return _dataStorage.ipToServerId;
        }

        [HttpGet("/leaflinks")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<List<LeafLink>> GetLeafLinks()
        {  
            return _dataStorage.leafLinks;
        }

        [HttpGet("/gatewayLinks")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<GatewayLink>> GetGatewayLinks()
        {   
            return _dataStorage.gatewayLinks;
        }

        [HttpGet("/varz")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Server>> GetVarz()
        {   
            return _dataStorage.servers;
        }

        // TODO use to get info about server, when it is clicked in UI
        [HttpGet("/varz/{serverId}")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<Server> GetVarz([FromRoute] string serverId)
        {   
            var query = from server in _dataStorage.servers.AsParallel()
                where server.server_id == serverId
                select server;
            return query.FirstOrDefault();
        }

        [HttpGet("/connz")] //The route to the endpoint. Etc. localhost:5001/connz
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Connection>> GetConnz()
        {   
            return _dataStorage.connections;
        }
        
        [HttpGet("/links")]
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Link>> GetLinks()
        {   
            return _dataStorage.links;
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
