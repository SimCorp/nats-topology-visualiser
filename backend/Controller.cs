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
        public ActionResult<IEnumerable<Server>> Get()
        {   
            return Program.Servers;
        }

        [HttpGet("{connz}")] //The route to the endpoint. Etc. localhost:5001/connz
        [ProducesResponseType(Status200OK)]
        public ActionResult<IEnumerable<Connection>> Get_Connz()
        {   
            return Program.Connections;
        }

    }
}