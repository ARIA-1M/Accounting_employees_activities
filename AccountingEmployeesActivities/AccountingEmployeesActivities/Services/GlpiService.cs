// Services/Implementations/GlpiService.cs
using AccountingEmployeesActivities.Services.Interfaces;
using AccountingEmployeesActivities.Services.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AccountingEmployeesActivities.Services.Implementations
{
    public class GlpiService : IGlpiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://sp.52gov.ru/apirest.php";
        private readonly string _username = "sedo_admin";
        private readonly string _password = "jySrE2yuTLcTCMi4";
        private string _sessionToken;

        public GlpiService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // Services/Implementations/GlpiService.cs

        public async Task<string> GetSessionToken()
        {
            if (!string.IsNullOrEmpty(_sessionToken))
            {
                System.Diagnostics.Debug.WriteLine($"Токен уже есть: {_sessionToken}");
                return _sessionToken;
            }

            var request = new
            {
                login = _username,
                password = _password
                // ★ app_token УБИРАЕМ ★
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            System.Diagnostics.Debug.WriteLine($"Запрос к {_baseUrl}/initSession");
            System.Diagnostics.Debug.WriteLine($"Тело: {JsonSerializer.Serialize(request)}");

            var response = await _httpClient.PostAsync($"{_baseUrl}/initSession", content);

            var responseBody = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Статус: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Ответ: {responseBody}");

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<GlpiSessionResponse>(responseBody);
                _sessionToken = result?.SessionToken;
                System.Diagnostics.Debug.WriteLine($"Токен получен: {_sessionToken}");
                return _sessionToken;
            }

            System.Diagnostics.Debug.WriteLine($"Ошибка получения токена: {response.StatusCode}");
            throw new Exception($"Ошибка получения токена: {response.StatusCode}. Ответ: {responseBody}");
        }        // Поиск задач
        public async Task<GlpiSearchResponse> SearchTicketsAsync(
            DateTime startDate,
            DateTime endDate,
            int? employeeId = null)
        {
            var token = await GetSessionToken();

            var url = $"{_baseUrl}/search/Ticket?session_token={token}";
            url += $"&is_deleted=0";
            url += $"&sort=19";
            url += $"&order=DESC";
            url += $"&limit=100";  // Загружаем до 100 задач за раз

            // Фильтр по дате закрытия
            url += $"&criteria[3][link]=AND";
            url += $"&criteria[3][field]=19";
            url += $"&criteria[3][searchtype]=morethan";
            url += $"&criteria[3][value]={startDate:yyyy-MM-dd HH:mm:ss}";

            url += $"&criteria[4][link]=AND";
            url += $"&criteria[4][field]=19";
            url += $"&criteria[4][searchtype]=lessthan";
            url += $"&criteria[4][value]={endDate:yyyy-MM-dd HH:mm:ss}";

            // Фильтр по исполнителю (поле 5 - users_id_assign)
            if (employeeId.HasValue)
            {
                url += $"&criteria[1][link]=AND";
                url += $"&criteria[1][field]=5";  // ← users_id_assign
                url += $"&criteria[1][searchtype]=equals";
                url += $"&criteria[1][value]={employeeId}";
            }

            url += $"&limit=100";

            System.Diagnostics.Debug.WriteLine($"URL: {url}");

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Session-Token", token);

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Статус: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"Ответ (первые 500 символов): {responseBody.Substring(0, Math.Min(500, responseBody.Length))}");

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<GlpiSearchResponse>(responseBody);
                System.Diagnostics.Debug.WriteLine($"Найдено задач: {result?.Data?.Count ?? 0} из {result?.TotalCount ?? 0}");
                return result;
            }

            System.Diagnostics.Debug.WriteLine($"Ошибка: {response.StatusCode}");
            return new GlpiSearchResponse { Data = new List<GlpiTicket>(), TotalCount = 0 };
        }
        // Получение списка задач за период
        public async Task<List<GlpiTicket>> GetTicketsAsync(
            DateTime startDate,
            DateTime endDate,
            int? employeeId = null)
        {
            var result = await SearchTicketsAsync(startDate, endDate, employeeId);
            return result?.Data ?? new List<GlpiTicket>();
        }

        // Получение задач для конкретного сотрудника
        public async Task<List<GlpiTicket>> GetTicketsByEmployeeAsync(
            int employeeId,
            DateTime startDate,
            DateTime endDate)
        {
            return await GetTicketsAsync(startDate, endDate, employeeId);
        }
    }
}