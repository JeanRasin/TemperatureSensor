using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TemperatureSensorAndroid.Services.Abstract;

namespace TemperatureSensorAndroid.Services.Concrete
{
    /// <summary>
    /// Сервис отправки запросов.
    /// </summary>
    public class RequestSenderService : IRequestSenderService
    {
        /// <inheritdoc />
        public bool UseCamelCase { get; set; } = true;

        /// <inheritdoc />
        public Dictionary<string, string> DefaultHeaders { get; set; }
        

        /// <summary>
        /// Http клиент.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Заголовок авторизации.
        /// </summary>
        private AuthenticationHeaderValue _authenticationHeader { get; set; }

        /// <summary>
        /// Сервис отправки запросов.
        /// </summary>
        public RequestSenderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <inheritdoc />
        public void SetAuthorizationHeader(string login, string password)
        {
            _authenticationHeader = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}:{password}")));
        }

        /// <inheritdoc />
        public void SetAuthorizationHeader(string token)
        {
            _authenticationHeader = new AuthenticationHeaderValue("Token", token);
        }

        /// <inheritdoc />
        public async Task<T> GetJsonAsync<T>(string url, object body = null, Dictionary<string, string> headers = null)
        {
            return await SendJsonRequestAsync<T>(HttpMethod.Get, url, body, headers);
        }

        /// <inheritdoc />
        public T GetJson<T>(string url, object body = null, Dictionary<string, string> headers = null)
        {
            return SendJsonRequest<T>(HttpMethod.Get, url, body, headers);
        }

        /// <inheritdoc />
        public async Task GetJsonAsync(string url, object body = null, Dictionary<string, string> headers = null)
        {
            await SendJsonRequestAsync<object>(HttpMethod.Get, url, body, headers);
        }

        /// <inheritdoc />
        public async Task<T> PostJsonAsync<T>(string url, object body = null, Dictionary<string, string> headers = null)
        {
            return await SendJsonRequestAsync<T>(HttpMethod.Post, url, body, headers);
        }

        /// <inheritdoc />
        public async Task PostJsonAsync(string url, object body = null, Dictionary<string, string> headers = null)
        {
            await SendJsonRequestAsync<object>(HttpMethod.Post, url, body, headers);
        }

        /// <inheritdoc />
        public async Task<T> PutJsonAsync<T>(string url, object body = null, Dictionary<string, string> headers = null)
        {
            return await SendJsonRequestAsync<T>(HttpMethod.Put, url, body, headers);
        }

        /// <inheritdoc />
        public async Task PutJsonAsync(string url, object body = null, Dictionary<string, string> headers = null)
        {
            await SendJsonRequestAsync<object>(HttpMethod.Put, url, body, headers);
        }

        /// <inheritdoc />
        public void PutJson(string url, object body = null, Dictionary<string, string> headers = null)
        {
            SendJsonRequest<object>(HttpMethod.Put, url, body, headers);
        }

        /// <inheritdoc />
        public async Task<T> DeleteJsonAsync<T>(string url, object body = null, Dictionary<string, string> headers = null)
        {
            return await SendJsonRequestAsync<T>(HttpMethod.Delete, url, body, headers);
        }

        /// <inheritdoc />
        public async Task DeleteJsonAsync(string url, object body = null, Dictionary<string, string> headers = null)
        {
            await SendJsonRequestAsync<object>(HttpMethod.Delete, url, body, headers);
        }

        /// <inheritdoc />
        public async Task<T> SendJsonRequestAsync<T>(HttpMethod method,
                                                     string url,
                                                     object body = null,
                                                     Dictionary<string, string> headers = null)
        {
            var httpClient = ConfigureHttpClient();

            var requestMessage = new HttpRequestMessage(method, url);

            if (body != null)
            {
                var json = JsonConvert.SerializeObject(body);

                var requestBody = new StringContent(json, Encoding.UTF8, "application/json");

                requestMessage.Content = requestBody;
            }

            if (headers != null)
            {
                foreach (var pair in headers)
                {
                    requestMessage.Headers.Add(pair.Key, pair.Value);
                }
            }

            var response = await httpClient.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var result = DeserializeContentString<T>(content);

            return result;
        }

        public T SendJsonRequest<T>(HttpMethod method,
                                             string url,
                                             object body = null,
                                             Dictionary<string, string> headers = null)
        {
            var httpClient = ConfigureHttpClient();

            var requestMessage = new HttpRequestMessage(method, url);

            if (body != null)
            {
                var json = JsonConvert.SerializeObject(body);

                var requestBody = new StringContent(json, Encoding.UTF8, "application/json");

                requestMessage.Content = requestBody;
            }

            if (headers != null)
            {
                foreach (var pair in headers)
                {
                    requestMessage.Headers.Add(pair.Key, pair.Value);
                }
            }

            var response = httpClient.SendAsync(requestMessage).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            response.EnsureSuccessStatusCode();

            var result = DeserializeContentString<T>(content);

            return result;
        }

        /// <inheritdoc />
        public async Task<Stream> GetStreamAsync(string url, Dictionary<string, string> headers = null)
        {
            var httpClient = ConfigureHttpClient();

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

            if (headers != null)
            {
                foreach (var pair in headers)
                {
                    requestMessage.Headers.Add(pair.Key, pair.Value);
                }
            }

            var response = await httpClient.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStreamAsync();

            response.EnsureSuccessStatusCode();

            return content;
        }

        /// <inheritdoc />
        public async Task<T> PostStreamAsync<T>(string url, Stream stream, Dictionary<string, string> headers = null)
        {
            var httpClient = ConfigureHttpClient();

            var body = new StreamContent(stream);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = body
            };

            if (headers != null)
            {
                foreach (var pair in headers)
                {
                    requestMessage.Headers.Add(pair.Key, pair.Value);
                }
            }

            var response = await httpClient.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var result = DeserializeContentString<T>(content);

            return result;
        }

        /// <summary>
        /// Конфигурация HttpClient.
        /// </summary>
        private HttpClient ConfigureHttpClient()
        {
            if (_authenticationHeader != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = _authenticationHeader;
            }

            if (DefaultHeaders != null)
            {
                foreach (var pair in DefaultHeaders)
                {
                    _httpClient.DefaultRequestHeaders.Remove(pair.Key);
                    _httpClient.DefaultRequestHeaders.Add(pair.Key, pair.Value);
                }
            }

            return _httpClient;
        }

        /// <summary>
        /// Десериализация строки контента ответа.
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения.</typeparam>
        /// <param name="content">Строка контента.</param>
        private T DeserializeContentString<T>(string content)
        {
            T result = default;

            if (!string.IsNullOrWhiteSpace(content))
            {
                result = JsonConvert.DeserializeObject<T>(content);
            }

            return result;
        }
    }
}
