using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TemperatureSensor.WebUI.DAL.Repositories.Abstract;
using TemperatureSensor.WebUI.Model;

namespace TemperatureSensor.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemperatureController : ControllerBase
    {
        private readonly ITemperatureRepository _temperatureRepository;

        public TemperatureController(ITemperatureRepository temperatureRepository)
        {
            _temperatureRepository = temperatureRepository;
        }

        [AllowAnonymous]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        [HttpGet]
        public async Task<IEnumerable<HouseIndicator>> Get()
        {
            var result = await _temperatureRepository.Get();

            return result;
        }

        [Authorize]
        [HttpPost("temperature/{temperature}/humidity/{humidity}")]
        public async Task<IActionResult> Post(decimal temperature, decimal humidity)
        {
            await _temperatureRepository.Insert(temperature, humidity);

            return Ok("Ok");
        }

        /*
        [Authorize]
        [HttpPost("randomGenerate")]
        public async Task<IActionResult> Post()
        {
            await _temperatureRepository.InsertRandom(10);

            return Ok("Ok");
        }
        */
    }
}
