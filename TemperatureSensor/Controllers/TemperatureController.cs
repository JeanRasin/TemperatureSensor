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

        [HttpPost("temperature/{temperature}/humidity/{humidity}")]
        public async Task<IActionResult> Post(decimal temperature, decimal humidity)
        {
            await _temperatureRepository.InsertTemperature(temperature, humidity);

            return Ok("Ok");
        }
    }
}
