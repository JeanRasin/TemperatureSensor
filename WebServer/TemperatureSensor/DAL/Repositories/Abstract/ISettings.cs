using TemperatureSensor.WebUI.Model;

namespace TemperatureSensor.WebUI.DAL.Repositories.Abstract
{
    public interface ISettings
    {
        Task<Settings> Get();

        Task Update(Settings settings);
    }
}
