using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TemperatureSensor.WebUI.DAL.Repositories.Abstract;
using TemperatureSensor.WebUI.Model;

namespace TemperatureSensor.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISettings _settings;

        public SettingsController(ISettings settings)
        {
            _settings = settings;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            Settings? settings = await _settings.Get();

            return Ok(settings);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Put(Settings data)
        {
            await _settings.Update(data);

            return Ok();
        }
        
    }
}
