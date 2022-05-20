using Dapper;
using Microsoft.Data.Sqlite;
using TemperatureSensor.WebUI.DAL.Repositories.Abstract;
using TemperatureSensor.WebUI.Model;

namespace TemperatureSensor.WebUI.DAL.Repositories.Concrete
{
    public class SettingsRepository : ISettings
    {
        private readonly SqliteConnection _sqliteConnection;

        public SettingsRepository(SqliteConnection sqliteConnection)
        {
            _sqliteConnection = sqliteConnection;
        }

        public async Task<Settings> Get()
        {
            var sql = $"SELECT DelayMinutes, SendSmsMaxDay, TemperatureMin, TemperatureMax, UrlChart, UrlTable FROM Settings";

            var result = await _sqliteConnection.QueryFirstAsync<Settings>(sql);

            return result;
        }

        public async Task Update(Settings settings)
        {
            var sql = $"UPDATE Settings SET DelayMinutes = @DelayMinutes, SendSmsMaxDay = @SendSmsMaxDay, TemperatureMin = @TemperatureMin, TemperatureMax = @TemperatureMax, UrlChart = @UrlChart, UrlTable = @UrlTable";

            var parms = new {
                settings.DelayMinutes,
                settings.SendSmsMaxDay,
                settings.TemperatureMin,
                settings.TemperatureMax,
                settings.UrlChart,
                settings.UrlTable
            };

            await _sqliteConnection.QueryAsync(sql, parms);
        }

    }
}
