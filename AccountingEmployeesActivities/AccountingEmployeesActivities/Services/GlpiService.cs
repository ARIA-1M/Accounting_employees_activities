// Services/Implementations/GlpiService.cs
using AccountingEmployeesActivities.Services.Interfaces;
using AccountingEmployeesActivities.Services.Models;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Buffers.Text;
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
        private readonly string _baseUrl = "http://10.10.129.9/apirest.php";
        private readonly string _username = "sedo_admin";
        private readonly string _password = "jySrE2yuTLcTCMi4";

        private string _sessionToken;

        public GlpiService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public GlpiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
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
        }
        public async Task<string> GetSessionToken(string login, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "initSession");
            request.Headers.Add("Authorization",
                "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}:{password}")));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            System.Diagnostics.Debug.WriteLine("Get ticket "+json);
            return doc.RootElement.GetProperty("session_token").GetString();
        }
        // Поиск задач
        public async Task<int> GetTicketCountAsync(
            DateTime startDate,
            DateTime endDate,
            int? employeeId = null)
        {
            if (string.IsNullOrEmpty(_sessionToken))
                throw new InvalidOperationException("Session token is missing. Call GetSessionToken first.");

            var url = $"search/Ticket?is_deleted=0&sort=19&order=DESC&limit=1"; // limit=1, чтобы получить только count
            int idx = 0;

            // Фильтр по дате начала
            url += $"&criteria[{idx}][link]=AND&criteria[{idx}][field]=19&criteria[{idx}][searchtype]=morethan&criteria[{idx}][value]={startDate:yyyy-MM-dd HH:mm:ss}";
            idx++;

            // Фильтр по дате конца
            url += $"&criteria[{idx}][link]=AND&criteria[{idx}][field]=19&criteria[{idx}][searchtype]=lessthan&criteria[{idx}][value]={endDate:yyyy-MM-dd HH:mm:ss}";
            idx++;

            // Фильтр по заявителю (только если employeeId > 0)
            if (employeeId.HasValue && employeeId.Value > 0)
            {
                url += $"&criteria[{idx}][link]=AND&criteria[{idx}][field]=12&criteria[{idx}][searchtype]=equals&criteria[{idx}][value]={employeeId.Value}";
                idx++;
            }

            System.Diagnostics.Debug.WriteLine($"URL запроса (count): {url}");

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Session-Token", _sessionToken);

            var response = await _httpClient.SendAsync(request);

            // Проверяем успешность
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Ошибка запроса: {response.StatusCode}, содержимое: {errorContent}");
                return 0;
            }

            var json = await response.Content.ReadAsStringAsync();

            // Проверяем, что ответ начинается с '{' (JSON)
            if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
            {
                System.Diagnostics.Debug.WriteLine($"Ответ не является JSON: {json.Substring(0, Math.Min(200, json.Length))}");
                return 0;
            }

            System.Diagnostics.Debug.WriteLine($"Ответ (count): {json}");

            try
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("totalcount", out var totalProp))
                {
                    int total = totalProp.GetInt32();
                    System.Diagnostics.Debug.WriteLine($"Найдено задач: {total}");
                    return total;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Поле totalcount не найдено в ответе");
                    return 0;
                }
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка парсинга JSON: {ex.Message}");
                return 0;
            }
        }
        // Получение списка задач за период
        public async Task<List<GlpiTicket>> GetTicketsAsync(
            DateTime startDate,
            DateTime endDate,
            int? employeeId = null)
        {
            System.Diagnostics.Debug.WriteLine($"Запрос к GLPI: {_baseUrl}");

            var result = await SearchTicketsAsync(startDate, endDate, employeeId);

            var request = new HttpRequestMessage(HttpMethod.Get, _baseUrl);
            request.Headers.Add("Session-Token", _sessionToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            // ЛОГИРУЕМ ОТВЕТ (можно частично, если большой)
            System.Diagnostics.Debug.WriteLine($"Ответ GLPI: {json.Substring(0, Math.Min(500, json.Length))}...");
            var doc = JsonDocument.Parse(json);
            var dataArray = doc.RootElement.GetProperty("data").EnumerateArray();

            var tickets = new List<GlpiTicket>();
            foreach (var item in dataArray)
            {
                var ticket = new GlpiTicket
                {
                    Id = item.GetProperty("2").GetInt32(),
                    Name = item.GetProperty("1").GetString(),
                    Status = item.TryGetProperty("5", out var statusProp) && statusProp.ValueKind != JsonValueKind.Null
                             ? statusProp.GetInt32()
                             : 0, // если null – считаем неизвестным
                    RequesterId = item.GetProperty("12").GetInt32(),
                    CreationDate = DateTime.Parse(item.GetProperty("19").GetString())
                };

                // Если есть дата закрытия (поле "15"?)
                if (item.TryGetProperty("15", out var closeProp) && closeProp.ValueKind != JsonValueKind.Null)
                    ticket.CloseDate = DateTime.Parse(closeProp.GetString());

                ticket.StatusName = MapStatus(ticket.Status);
                tickets.Add(ticket);
            }

            return tickets;
        }

        public async Task<GlpiSearchResponse> SearchTicketsAsync(DateTime startDate, DateTime endDate, int? employeeId = null)
        {
            var tickets = await GetTicketsAsync(startDate, endDate, employeeId);
            return new GlpiSearchResponse { Data = tickets, TotalCount = tickets.Count };
        }

        private string MapStatus(int statusId)
        {
            return statusId switch
            {
                1 => "Новая",
                2 => "В работе (назначена)",
                3 => "В ожидании",
                4 => "Решена",
                5 => "Закрыта",
                6 => "Отклонена",
                54 => "Закрыта (архив)",
                _ => "Неизвестно"
            };
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