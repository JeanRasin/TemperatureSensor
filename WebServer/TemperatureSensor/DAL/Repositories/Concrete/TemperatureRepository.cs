using Dapper;
using Microsoft.Data.Sqlite;
using TemperatureSensor.WebUI.DAL.Repositories.Abstract;
using TemperatureSensor.WebUI.Model;

namespace TemperatureSensor.WebUI.DAL.Repositories.Concrete
{
    public class TemperatureRepository : ITemperatureRepository
    {
        private readonly SqliteConnection _sqliteConnection;
        private readonly IConfiguration _сonfiguration;

        public TemperatureRepository(SqliteConnection sqliteConnection, IConfiguration сonfiguration)
        {
            _sqliteConnection = sqliteConnection;
            _сonfiguration = сonfiguration;
        }

        public async Task<IEnumerable<HouseIndicator>> Get()
        {
            if (DateTime.Now.Month == 28)
            {
                await ClearDB();
            }

            // Discreteness in minutes
            IConfigurationSection? discret = _сonfiguration.GetSection("Discreteness");

            // 1440 minutes per day
            const int minitPerDay = 24 * 60; 

            var limit = Convert.ToInt32(minitPerDay / Convert.ToInt32(discret.Value));

            var sql = $"SELECT Id, Date, TemperatureData, Humidity FROM Temperature ORDER BY Id DESC LIMIT {limit}";

            var result = await _sqliteConnection.QueryAsync<HouseIndicator>(sql);

            return result;
        }

        public async Task Insert(decimal temperature, decimal humidity)
        {
            var sql = "INSERT INTO Temperature(Date, TemperatureData, Humidity) Values(DATETIME('now','+05:00'), @TemperatureData, @Humidity)";

            var data = new HouseIndicator
            {
                TemperatureData = temperature,
                Humidity = humidity
            };

            await _sqliteConnection.QueryAsync(sql, data);
        }

        public async Task InsertRandom(int rows)
        {
            var random = new Random();

            for (int i = 0; i < rows; i++)
            {
                int rTemperature = random.Next(1500, 3000);
                var resultTemperature = (decimal)(rTemperature / 100.00);

                int rHumidity = random.Next(0, 10000);
                var resultHumidity = (decimal)(rHumidity / 100.00);

                var item = (await Get()).FirstOrDefault();

                var date = item != null ? item.Date.AddMinutes(5) : DateTime.Now;

                var sql = "INSERT INTO Temperature(Date, TemperatureData, Humidity) Values(@Date, @TemperatureData, @Humidity)";

                var data = new HouseIndicator
                {
                    Date = date,
                    TemperatureData = resultTemperature,
                    Humidity = resultHumidity
                };

                await _sqliteConnection.QueryAsync(sql, data);
            }
        }

        public async Task CreateDb()
        {
            var sql = $@"CREATE TABLE IF NOT EXISTS Temperature (
                  {nameof(HouseIndicator.Id)} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                  {nameof(HouseIndicator.Date)} TEXT NOT NULL,
                  {nameof(HouseIndicator.TemperatureData)} REAL NOT NULL,
                  {nameof(HouseIndicator.Humidity)} REAL NOT NULL
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
