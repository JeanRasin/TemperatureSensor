using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TemperatureSensorAndroid.Services.Abstract
{
    /// <summary>
    /// Сервис отправки запросов.
    /// </summary>
    public interface IRequestSenderService
    {
        /// <summary>
        /// Использовать CamelCase при сериализации. (Default: true)
        /// </summary>
        bool UseCamelCase { get; set; }

        /// <summary>
        /// Заголовки запроса по умолчанию.
        /// </summary>
        Dictionary<string, string> DefaultHeaders { get; set; }

        /// <summary>
        /// Указать данные для базовой авторизации.
        /// </summary>
        /// <param name="login">Логин.</param>
        /// <param name="password">Пароль.</param>
        void SetAuthorizationHeader(string login, string password);

        /// <summary>
        /// Указать данные для авторизации по токену.
        /// </summary>
        /// <param name="token">Токен авторизации.</param>
        void SetAuthorizationHeader(string token);

        /// <summary>
        /// Отправить GET запрос application/json.
        /// </summary>
        /// <typeparam name="T">Тип ответа.</typeparam>
        /// <param name="url">Url запроса.</param>
        /// <param name="body">Объект тела запроса.</param>
        /// <param name="headers">Дополнительные заголовки запроса.</param>
        Task<T> GetJsonAsync<T>(string url, object body = null, Dictionary<string, string> headers = null);

        /// <summary>
        /// Отправить GET запрос application/json.
        /// </summary>
        /// <typeparam name="T">Тип ответа.</typeparam>
        /// <param name="url">Url запроса.</param>
        /// <param name="body">Объект тела запроса.</param>
        /// <param name="headers">Дополнительные заголовки запроса.</param>
        T GetJson<T>(string url, object body = null, Dictionary<string, string> headers = null);

        /// <summary>
        /// Отправить GET запрос application/json.
        /// </summary>
        /// <param name="url">Url запроса.</param>
        /// <param name="body">Объект тела запроса.</param>
        /// <param name="headers">Дополнительные заголовки запроса.</param>
        Task GetJsonAsync(string url, object body = null, Dictionary<string, string> headers = null);

        /// <summary>
        /// Отправить POST запрос application/json.
        /// </summary>
        /// <typeparam name="T">Тип ответа.</typeparam>
        /// <param name="url">Url запроса.</param>
        /// <param name="body">Объект тела запроса.</param>
        /// <param name="headers">Дополнительные заголовки запроса.</param>
        Task<T> PostJsonAsync<T>(string url, object body = null, Dictionary<string, string> headers = null);

        /// <summary>
        /// Отправить POST запрос application/json.
        /// </summary>
        /// <param name="url">Url запроса.</param>
        /// <param name="body">Объект тела запроса.</param>
        /// <param name="headers">Дополнительные заголовки запроса.</param>
        Task PostJsonAsync(string url, object body = null, Dictionary<string, string> headers = null);

        /// <summary>
        /// Отправить PUT запрос application/json.
        /// </summary>
        /// <typeparam name="T">Тип ответа.</typeparam>
        /// <param name="url">Url запроса.</param>
        /// <param name="body">Объект тела запроса.</param>
        /// <param name="headers">Дополнительные заголовки запроса.</param>
        Task<T> PutJsonAsync<T>(string url, object body = null, Dictionary<string, string> headers = null);

        /// <summary>
        /// Отправить PUT запрос application/json.
        /// </summary>
        /// <param name="url">Url запроса.</param>
        /// <param name="body">Объект тела запроса.</param>
        /// <param name="headers">Дополнительные заголовки запроса.</param>
        Task PutJsonAsync(string url, object body = null, Dictionary<string, string> headers = null);

        /// <summary>
        /// Отправить PUT запрос application/json.
        /// </summary>
        /// <param name="url">Url запроса.</param>
        /// <param name="body">Объект тела запроса.</param>
        /// <param name="headers">Дополнительные заголовки запроса.</param>
        void PutJson(string url, object body = null, Dictionary<string, string> headers = null);

        /// <summary>
        /// Отправить DELETE запрос application/json.
        /// </summary>
        /// <typeparam name="T">Тип ответа.</typeparam>
        /// <param name="url">Url запроса.</param>
        /// <param name="body">Объект тела запроса.</param>
        /// <param name="headers">Дополнительные заголовки запроса.</param>
        Task<T> DeleteJsonAsync<T>(string url, object body = null, Dictionary<string, string> headers = null);

        /// <summary>
        /// Отправить DELETE запрос application/json.
        /// </summary>
        /// <typeparam name="T">Тип ответа.</typeparam>
        /// <param name="url">Url запроса.</param>
        /// <param name="body">Объект тела запроса.</param>
        /// <param name="headers">Дополнительные заголовки запроса.</param>
        Task DeleteJsonAsync(string url, object body = null, Dictionary<string, string> headers = null);

        /// <summary>
        /// Отправка запроса с типом контента application/json.
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения.</typeparam>
        /// <param name="method">Метод.</param>
        /// <param name="url">Url запроса.</param>
        /// <param name="body">Тело запроса.</param>
        /// <param name="headers">Дополнительные заголовки запроса.</param>
        Task<T> SendJsonRequestAsync<T>(HttpMethod method,
                                        string url,
                                        object body = null,
                                        Dictionary<string, string> headers = null);

        /// <summary>
        /// Отправка запроса с типом контента application/json.
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения.</typeparam>
        /// <param name="method">Метод.</param>
        /// <param name="url">Url запроса.</param>
        /// <param name="body">Тело запроса.</param>
        /// <param name="headers">Дополнительные заголовки запроса.</param>
        T SendJsonRequest<T>(HttpMethod method,
                                        string url,
                                        object body = null,
                                        Dictionary<string, string> headers = null);

        /// <summary>
        /// Отправить GET запрос на получение потока Stream.
        /// </summary>
        /// <param name="url">Url запроса.</param>
        /// <param name="headers">Дополнительные заголовки запроса.</param>
        Task<Stream> GetStreamAsync(string url, Dictionary<string, string> headers = null);

        /// <summary>
        /// Отправить POST запрос с потоком.
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения.</typeparam>
        /// <param name="url">Url запроса.</param>
        /// <param name="stream">Поток.</param>
        /// <param name="headers">Дополнительные заголовки запроса.</param>
        Task<T> PostStreamAsync<T>(string url, Stream stream, Dictionary<string, string> headers = null);
    }
}
