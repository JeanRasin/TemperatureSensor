using TemperatureSensor.WebUI.Model;

namespace TemperatureSensor.WebUI.DAL.Repositories.Abstract
{
    public interface ISettings
    {
        Task<Settings> Get();

        Task UpdateValue(string key, string value);
    }
}
