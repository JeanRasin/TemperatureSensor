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

        public async Task UpdateValue(string key, string value)
        {
            var sql = $"UPDATE Settings SET {key} = {value}";

            await _sqliteConnection.QueryAsync(sql);
        }

    }
}
