using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TemperatureSensor.WebUI.DAL.Repositories.Abstract;
using TemperatureSensor.WebUI.Model;

namespace TemperatureSensor.WebUI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TemperatureController : ControllerBase
    {
        private readonly ITemperatureRepository _temperatureRepository;

        public TemperatureController(ITemperatureRepository temperatureRepository)
        {
            _temperatureRepository = temperatureRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Temperature>> Get()
        {
            var result = await _temperatureRepository.GetTemperature();

            return result;
        }

        [HttpPost("temperature/{temperature:int}")]
        public async Task<IActionResult> Post(int temperature)
        {
            await _temperatureRepository.InsertTemperature(temperature);

            return Ok("Ok");
        }

        [HttpGet("ClearDB")]
        public async Task<IActionResult> ClearDB()
        {
            await _temperatureRepository.ClearDB();

            return Ok("Ok");
        }
    }
}
