using TemperatureSensor.WebUI.Model;

namespace TemperatureSensor.WebUI.DAL.Repositories.Abstract
{
    public interface ITemperatureRepository
    {
        Task<IEnumerable<HouseIndicator>> Get();

        Task Insert(decimal temperature, decimal humidity);

        Task InsertRandom(int rows);

        Task CreateDb();
    }
}
