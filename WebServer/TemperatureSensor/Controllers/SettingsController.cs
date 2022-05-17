using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TemperatureSensor.WebUI.DAL.Repositories.Abstract;

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
        [HttpPut("key/{key}/value/{value}")]
        public async Task<IActionResult> Put(string key, string value)
        {
            await _settings.UpdateValue(key, value);

            return Ok();
        }
        
    }
}
