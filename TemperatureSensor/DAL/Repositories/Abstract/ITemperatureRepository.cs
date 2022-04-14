using TemperatureSensor.WebUI.Model;

namespace TemperatureSensor.WebUI.DAL.Repositories.Abstract
{
    public interface ITemperatureRepository
    {
        Task<IEnumerable<Temperature>> GetTemperature();

        Task InsertTemperature(decimal temperature, decimal humidity);

        Task CreateDb();
    }
}
