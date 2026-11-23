
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FootballLeague.Cliente.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress =
                    new Uri("https://localhost:7058/")
            };
        }

        public async Task<List<T>> ObtenerListaAsync<T>(
            string ruta)
        {
            using var respuesta =
                await _httpClient.GetAsync(ruta);

            respuesta.EnsureSuccessStatusCode();

            var datos = await respuesta.Content
                .ReadFromJsonAsync<List<T>>(_jsonOptions);

            return datos ?? new List<T>();
        }

        public async Task<T?> ObtenerAsync<T>(string ruta)
        {
            using var respuesta =
                await _httpClient.GetAsync(ruta);

            if (respuesta.StatusCode ==
                HttpStatusCode.NotFound)
            {
                return default;
            }

            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content
                .ReadFromJsonAsync<T>(_jsonOptions);
        }

        public async Task<HttpResponseMessage> CrearAsync<T>(
            string ruta, T datos)
        {
            return await _httpClient.PostAsJsonAsync(
                ruta, datos);
        }

        public async Task<HttpResponseMessage> ModificarAsync<T>(
            string ruta, T datos)
        {
            return await _httpClient.PutAsJsonAsync(
                ruta, datos);
        }

        public async Task<HttpResponseMessage> EliminarAsync(
            string ruta)
        {
            return await _httpClient.DeleteAsync(ruta);
        }
    }
}

