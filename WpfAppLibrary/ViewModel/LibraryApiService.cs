using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using WpfAppLibrary.Models;

namespace WpfAppLibrary.ViewModel
{
    public class LibraryApiService
    {
        private readonly HttpClient _httpClient;

        public LibraryApiService(HttpClient httpClient, string BaseURL)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseURL);

            if (Application.Current.Properties.Contains("JwtToken"))
            {
                var token = Application.Current.Properties["JwtToken"]?.ToString();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        // GET
        public async Task<List<T>> GetModelListAsync<T>(string endpoint)
        {
            try
            {
                if (string.IsNullOrEmpty(endpoint))
                    throw new ArgumentException("Endpoint не может быть пустым!", nameof(endpoint));

                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<T>>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP_GET_ERROR: {ex.Source} - {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON_ERROR: {ex.Source} - {ex.Message}");
                throw;
            }
        }

        public async Task<T> GetModelByIdAsync<T>(string endpoint, int id)
        {
            try
            {
                if (string.IsNullOrEmpty(endpoint))
                    throw new ArgumentException("Endpoint не может быть пустым!", nameof(endpoint));

                if (id <= 0)
                    throw new ArgumentException("ID должен быть положительным числом!", nameof(id));

                var response = await _httpClient.GetAsync($"{endpoint}/{id}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP_GET_ERROR: {ex.Source} - {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON_ERROR: {ex.Source} - {ex.Message}");
                throw;
            }
        }

        // POST
        public async Task<T> CreateModelAsync<T>(object data, string endpoint)
        {
            try
            {
                if (data == null)
                    throw new NullReferenceException(nameof(data));

                if (string.IsNullOrEmpty(endpoint))
                    throw new ArgumentException("Endpoint не может быть пустым!", nameof(endpoint));

                var json = JsonSerializer.Serialize(data);
                
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP_POST_ERROR: {ex.Source} - {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON_ERROR: {ex.Source} - {ex.Message}");
                throw;
            }
        }

        // PUT
        public async Task<T> UpdateModelAsync<T>(string endpoint, object data, int id)
        {
            try
            {
                if (data == null)
                    throw new NullReferenceException(nameof(data));
                if (string.IsNullOrEmpty(endpoint))
                    throw new ArgumentException("Endpoint не может быть пустым!", nameof(endpoint));
                if (id <= 0)
                    throw new ArgumentException("ID должен быть положительным числом.", nameof(id));

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{endpoint}/{id}", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP_PUT_ERROR: {ex.Source} - {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON_ERROR: {ex.Source} - {ex.Message}");
                throw;
            }
        }

        // DELETE
        public async Task<bool> DeleteModelAsync(string endpoint, int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("ID должен быть положительным числом!", nameof(id));

                if (string.IsNullOrEmpty(endpoint))
                    throw new ArgumentException("Endpoint не может быть пустым!", nameof(endpoint));

                var response = await _httpClient.DeleteAsync($"{endpoint}/{id}");
                response.EnsureSuccessStatusCode();
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP_DELETE_ERROR: {ex.Source} - {ex.Message}");
                return false;
            }
        }
    }
}
