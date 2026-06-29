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
        public int Id { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }          // числовой ID статуса
        public string StatusName { get; set; }   // расшифровка
        public int RequesterId { get; set; }     // ID заявителя (поле 12)
        public int AssignedToId { get; set; }    // ID исполнителя (если нужно)
        public DateTime CreationDate { get; set; } // поле 19
        public DateTime? CloseDate { get; set; }
    }

    // Ответ на поиск задач
    public class GlpiSearchResponse
    {
        public List<GlpiTicket> Data { get; set; }
        public int TotalCount { get; set; }
    }
}