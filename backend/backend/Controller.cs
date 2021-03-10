using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.StatusCodes;

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
        public async Task<ActionResult<IEnumerable<Server>>> Get()
        {   
            return Program.Servers;
        }

    }
}