using Dapper;
using Microsoft.Data.Sqlite;
using TemperatureSensor.WebUI.DAL.Repositories.Abstract;
using TemperatureSensor.WebUI.Model;

namespace TemperatureSensor.WebUI.DAL.Repositories.Concrete
{
    public class TemperatureRepository : ITemperatureRepository
    {
        private readonly SqliteConnection _sqliteConnection;

        public TemperatureRepository(SqliteConnection sqliteConnection)
        {
            _sqliteConnection = sqliteConnection;
        }

        public async Task ClearDB()
        {
            var sql = "DELETE FROM Temperature";

            await _sqliteConnection.QueryAsync(sql);
        }

        public async Task CreateDb()
        {
            var sql = $@"CREATE TABLE IF NOT EXISTS Temperature (
                  {nameof(Temperature.Id)} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                  {nameof(Temperature.Date)} TEXT NOT NULL,
                  {nameof(Temperature.TemperatureData)} INTEGER NOT NULL
                 );";

            await _sqliteConnection.ExecuteAsync(sql);
        }

        public async Task<IEnumerable<Temperature>> GetTemperature()
        {
            var sql = "SELECT Id, Date, TemperatureData FROM Temperature ORDER BY Id DESC LIMIT 2";

            var result = await _sqliteConnection.QueryAsync<Temperature>(sql);

            return result;
        }

        public async Task InsertTemperature(int temperature)
        {
            var sql = "INSERT INTO Temperature(Date, TemperatureData) Values(@Date, @TemperatureData)";

            var data = new Temperature
            {
                Date = DateTime.Now,
                TemperatureData = temperature
            };

            await _sqliteConnection.QueryAsync(sql, data);
        }
    }
}
