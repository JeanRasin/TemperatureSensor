using AspNetCore.Authentication.Basic;

namespace TemperatureSensor.WebUI.Authorization
{
	public class BasicUserValidationService : IBasicUserValidationService
	{
		private readonly ILogger<BasicUserValidationService> _logger;
		private readonly IConfiguration _сonfiguration;

		public BasicUserValidationService(ILogger<BasicUserValidationService> logger, IConfiguration сonfiguration)
		{
			_logger = logger;
			_сonfiguration = сonfiguration;
		}

		public async Task<bool> IsValidAsync(string username, string password)
		{
			try
			{
                IConfigurationSection? login = _сonfiguration.GetSection("Login");
                IConfigurationSection? pass = _сonfiguration.GetSection("Password");

				var isValid = username == login.Value && password == pass.Value;

				return isValid;
			}
			catch (Exception e)
			{
				_logger.LogError(e, e.Message);
				throw;
			}
		}
	}
}
