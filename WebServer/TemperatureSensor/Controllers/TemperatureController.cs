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
        private readonly ISettings _settings;

        public TemperatureController(ITemperatureRepository temperatureRepository, ISettings settings)
        {
            _temperatureRepository = temperatureRepository;
            _settings = settings;
        }

        [AllowAnonymous]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        [HttpGet]
        public async Task<IEnumerable<HouseIndicator>> Get()
        {
            IEnumerable<HouseIndicator>? result = await _temperatureRepository.Get();

            return result;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] decimal temperature, [FromForm] decimal humidity)
        {
            await _temperatureRepository.Insert(temperature, humidity);

            Settings? result = await _settings.Get();

            return Ok(result);
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
