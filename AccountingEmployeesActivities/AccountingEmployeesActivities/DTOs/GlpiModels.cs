// Services/Models/GlpiModels.cs
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AccountingEmployeesActivities.Services.Models
{
    // Ответ на запрос сессии
    public class GlpiSessionResponse
    {
        [JsonPropertyName("session_token")]
        public string SessionToken { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }
    }

    // Задача из GLPI
    public class GlpiTicket
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("users_id_assign")]
        public int? UsersIdAssign { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("closedate")]
        public DateTime? CloseDate { get; set; }

        [JsonPropertyName("users_id_recipient")]
        public int? UsersIdRecipient { get; set; }
    }

    // Ответ на поиск задач
    public class GlpiSearchResponse
    {
        [JsonPropertyName("data")]
        public List<GlpiTicket> Data { get; set; }

        [JsonPropertyName("totalcount")]
        public int TotalCount { get; set; }
    }
}