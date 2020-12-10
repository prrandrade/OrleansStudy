namespace Client.Controllers
{
    using System.Threading.Tasks;
    using Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Orleans;

    [ApiController]
    [Route("[controller]")]
    public class DateTimeController : ControllerBase
    {
        private readonly IClusterClient _client;

        public DateTimeController(IClusterClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var grain = _client.GetGrain<IDateTimeGrain>(id);
            return Ok(await grain.CurrentDateTime());
        }
    }
}
