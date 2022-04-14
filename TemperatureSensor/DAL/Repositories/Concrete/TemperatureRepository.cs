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

        public async Task<IEnumerable<Temperature>> GetTemperature()
        {
            if (DateTime.Now.Month == 28)
            {
                await ClearDB();
            }

            var sql = "SELECT Id, Date, TemperatureData, Humidity FROM Temperature ORDER BY Id DESC LIMIT 30";

            var result = await _sqliteConnection.QueryAsync<Temperature>(sql);

            return result;
        }

        public async Task InsertTemperature(decimal temperature, decimal humidity)
        {
            var sql = "INSERT INTO Temperature(Date, TemperatureData, Humidity) Values(DATETIME('now','+05:00'), @TemperatureData, @Humidity)";

            var data = new Temperature
            {
                TemperatureData = temperature,
                Humidity = humidity
            };

            await _sqliteConnection.QueryAsync(sql, data);
        }

        public async Task CreateDb()
        {
            var sql = $@"CREATE TABLE IF NOT EXISTS Temperature (
                  {nameof(Temperature.Id)} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                  {nameof(Temperature.Date)} TEXT NOT NULL,
                  {nameof(Temperature.TemperatureData)} REAL NOT NULL,
                  {nameof(Temperature.Humidity)} REAL NOT NULL
                 );";

            await _sqliteConnection.ExecuteAsync(sql);
        }

        private async Task ClearDB()
        {
            var sql = $"DELETE FROM Temperature WHERE date(Date) BETWEEN DATETIME('now', '-10 years') AND DATETIME('now', 'start of month', '-1 day')";

            await _sqliteConnection.QueryAsync(sql);
        }
    }
}
