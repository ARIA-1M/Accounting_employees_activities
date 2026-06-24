using System;
using System.Collections.Generic;
using System.Text;
using AccountingEmployeesActivities.DTOs;
using AccountingEmployeesActivities.Services.Interfaces;
using ClosedXML.Excel;
using global::AccountingEmployeesActivities.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingEmployeesActivities.Services
{


    public class ExportService : IExportService
    {
        public async Task ExportToExcelAsync(
            string filePath,
            StatisticsDto statistics,
            List<StatusDistributionDto> statusDistribution,
            List<EmployeeTasksDto> employeeTasks,
            string employeeName,
            string dateRange)
        {
            await Task.Run(() =>
            {
                using var workbook = new XLWorkbook();

                // ===== Лист 1: Общая статистика =====
                var wsStats = workbook.Worksheets.Add("Общая статистика");

                // Заголовок
                wsStats.Range("A1:D1").Merge();
                wsStats.Cell("A1").Value = "Отчёт по статистике";
                wsStats.Cell("A1").Style.Font.Bold = true;
                wsStats.Cell("A1").Style.Font.FontSize = 16;
                wsStats.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Информация о фильтре
                wsStats.Cell("A3").Value = "Сотрудник:";
                wsStats.Cell("B3").Value = employeeName;
                wsStats.Cell("A3").Style.Font.Bold = true;

                wsStats.Cell("A4").Value = "Период:";
                wsStats.Cell("B4").Value = dateRange;
                wsStats.Cell("A4").Style.Font.Bold = true;

                // Карточка статистики
                wsStats.Cell("A6").Value = "Показатель";
                wsStats.Cell("B6").Value = "Значение";
                wsStats.Range("A6:B6").Style.Font.Bold = true;
                wsStats.Range("A6:B6").Style.Fill.BackgroundColor = XLColor.FromHtml("#3b82f6");
                wsStats.Range("A6:B6").Style.Font.FontColor = XLColor.White;

                wsStats.Cell("A7").Value = "Всего задач";
                wsStats.Cell("B7").Value = statistics.TotalTasks;

                wsStats.Cell("A8").Value = "Выполнено";
                wsStats.Cell("B8").Value = statistics.CompletedTasks;

                wsStats.Cell("A9").Value = "В работе";
                wsStats.Cell("B9").Value = statistics.InProgressTasks;

                wsStats.Cell("A10").Value = "Прогресс";
                wsStats.Cell("B10").Value = statistics.ProgressPercentage + "%";

                wsStats.Column(1).Width = 25;
                wsStats.Column(2).Width = 20;

                // Добавляем бордеры
                var statsRange = wsStats.Range("A6:B10");
                statsRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                statsRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // ===== Лист 2: Распределение по статусам =====
                var wsStatus = workbook.Worksheets.Add("Статусы");

                wsStatus.Cell("A1").Value = "Статус";
                wsStatus.Cell("B1").Value = "Количество";
                wsStatus.Cell("C1").Value = "Доля (%)";
                wsStatus.Range("A1:C1").Style.Font.Bold = true;
                wsStatus.Range("A1:C1").Style.Fill.BackgroundColor = XLColor.FromHtml("#3b82f6");
                wsStatus.Range("A1:C1").Style.Font.FontColor = XLColor.White;

                int row = 2;
                int totalCount = statusDistribution.Sum(s => s.Count);

                foreach (var status in statusDistribution)
                {
                    wsStatus.Cell(row, 1).Value = status.StatusName;
                    wsStatus.Cell(row, 2).Value = status.Count;

                    double percentage = totalCount > 0
                        ? Math.Round((double)status.Count / totalCount * 100, 1)
                        : 0;
                    wsStatus.Cell(row, 3).Value = percentage + "%";

                    row++;
                }

                wsStatus.Column(1).Width = 25;
                wsStatus.Column(2).Width = 15;
                wsStatus.Column(3).Width = 15;

                if (row > 2)
                {
                    var statusRange = wsStatus.Range($"A1:C{row - 1}");
                    statusRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    statusRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                // ===== Лист 3: Задачи по сотрудникам =====
                var wsEmployees = workbook.Worksheets.Add("Сотрудники");

                wsEmployees.Cell("A1").Value = "Сотрудник";
                wsEmployees.Cell("B1").Value = "Всего задач";
                wsEmployees.Cell("C1").Value = "Выполнено";
                wsEmployees.Cell("D1").Value = "Осталось";
                wsEmployees.Cell("E1").Value = "Прогресс (%)";
                wsEmployees.Range("A1:E1").Style.Font.Bold = true;
                wsEmployees.Range("A1:E1").Style.Fill.BackgroundColor = XLColor.FromHtml("#3b82f6");
                wsEmployees.Range("A1:E1").Style.Font.FontColor = XLColor.White;

                row = 2;
                foreach (var emp in employeeTasks)
                {
                    wsEmployees.Cell(row, 1).Value = emp.EmployeeName;
                    wsEmployees.Cell(row, 2).Value = emp.TotalTasks;
                    wsEmployees.Cell(row, 3).Value = emp.CompletedTasks;
                    wsEmployees.Cell(row, 4).Value = emp.TotalTasks - emp.CompletedTasks;

                    double empProgress = emp.TotalTasks > 0
                        ? Math.Round((double)emp.CompletedTasks / emp.TotalTasks * 100, 1)
                        : 0;
                    wsEmployees.Cell(row, 5).Value = empProgress + "%";

                    row++;
                }

                wsEmployees.Column(1).Width = 30;
                wsEmployees.Column(2).Width = 15;
                wsEmployees.Column(3).Width = 15;
                wsEmployees.Column(4).Width = 15;
                wsEmployees.Column(5).Width = 15;

                if (row > 2)
                {
                    var empRange = wsEmployees.Range($"A1:E{row - 1}");
                    empRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    empRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                workbook.SaveAs(filePath);
            });
        }
        public async Task ExportToPdfAsync(
string filePath,
StatisticsDto statistics,
List<StatusDistributionDto> statusDistribution,
List<EmployeeTasksDto> employeeTasks,
string employeeName,
string dateRange)
        {
            // Указываем тип лицензии (для бесплатного использования)
            QuestPDF.Settings.License = LicenseType.Community;

            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        //  HEADER 
                        page.Header().Element(header =>
                        {
                            header
                                .PaddingBottom(20)
                                .Column(col =>
                                {
                                    col.Item()
                                        .AlignCenter()
                                        .Text("Отчёт по статистике сотрудников")
                                        .FontSize(20)
                                        .Bold()
                                        .FontColor("#1e293b");

                                    col.Item()
                                        .PaddingTop(5)
                                        .AlignCenter()
                                        .Text($"Сотрудник: {employeeName}")
                                        .FontSize(12)
                                        .FontColor("#64748b");

                                    col.Item()
                                        .AlignCenter()
                                        .Text($"Период: {dateRange}")
                                        .FontSize(12)
                                        .FontColor("#64748b");

                                    col.Item()
                                        .PaddingTop(10)
                                        .LineHorizontal(1)
                                        .LineColor("#e2e8f0");
                                });
                        });

                        //  CONTENT 
                        page.Content().Column(col =>
                        {
                            // --- Блок: Общая статистика ---
                            col.Item()
                                .PaddingBottom(10)
                                .Text("Общая статистика")
                                .FontSize(14)
                                .Bold()
                                .FontColor("#1e293b");

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(1);
                                });

                                // Заголовок таблицы
                                table.Header(h =>
                                {
                                    h.Cell().Element(HeaderCell).Text("Показатель");
                                    h.Cell().Element(HeaderCell).Text("Значение");
                                });

                                table.Cell().Element(DataCell).Text("Всего задач");
                                table.Cell().Element(DataCell).Text(statistics.TotalTasks.ToString());

                                table.Cell().Element(DataCell).Text("Выполнено");
                                table.Cell().Element(DataCell).Text(statistics.CompletedTasks.ToString());

                                table.Cell().Element(DataCell).Text("В работе");
                                table.Cell().Element(DataCell).Text(statistics.InProgressTasks.ToString());

                                table.Cell().Element(DataCell).Text("Прогресс");
                                table.Cell().Element(DataCell).Text(statistics.ProgressPercentage + "%");
                            });

                            col.Item().PaddingVertical(15);

                            // --- Блок: Распределение по статусам ---
                            col.Item()
                                .PaddingBottom(10)
                                .Text("Распределение по статусам")
                                .FontSize(14)
                                .Bold()
                                .FontColor("#1e293b");

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(1);
                                    c.RelativeColumn(1);
                                });

                                table.Header(h =>
                                {
                                    h.Cell().Element(HeaderCell).Text("Статус");
                                    h.Cell().Element(HeaderCell).Text("Кол-во");
                                    h.Cell().Element(HeaderCell).Text("Доля");
                                });

                                int total = statusDistribution.Sum(s => s.Count);

                                foreach (var status in statusDistribution)
                                {
                                    double pct = total > 0
                                        ? Math.Round((double)status.Count / total * 100, 1)
                                        : 0;

                                    table.Cell().Element(DataCell).Text(status.StatusName);
                                    table.Cell().Element(DataCell).Text(status.Count.ToString());
                                    table.Cell().Element(DataCell).Text(pct + "%");
                                }
                            });

                            col.Item().PaddingVertical(15);

                            // --- Блок: Задачи по сотрудникам ---
                            col.Item()
                                .PaddingBottom(10)
                                .Text("Задачи по сотрудникам")
                                .FontSize(14)
                                .Bold()
                                .FontColor("#1e293b");

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(3);
                                    c.RelativeColumn(1);
                                    c.RelativeColumn(1);
                                    c.RelativeColumn(1);
                                    c.RelativeColumn(1);
                                });

                                table.Header(h =>
                                {
                                    h.Cell().Element(HeaderCell).Text("Сотрудник");
                                    h.Cell().Element(HeaderCell).Text("Всего");
                                    h.Cell().Element(HeaderCell).Text("Выполнено");
                                    h.Cell().Element(HeaderCell).Text("Осталось");
                                    h.Cell().Element(HeaderCell).Text("Прогресс");
                                });

                                foreach (var emp in employeeTasks)
                                {
                                    double empProgress = emp.TotalTasks > 0
                                        ? Math.Round((double)emp.CompletedTasks / emp.TotalTasks * 100, 1)
                                        : 0;

                                    table.Cell().Element(DataCell).Text(emp.EmployeeName);
                                    table.Cell().Element(DataCell).Text(emp.TotalTasks.ToString());
                                    table.Cell().Element(DataCell).Text(emp.CompletedTasks.ToString());
                                    table.Cell().Element(DataCell).Text((emp.TotalTasks - emp.CompletedTasks).ToString());
                                    table.Cell().Element(DataCell).Text(empProgress + "%");
                                }
                            });
                        });

                        //  FOOTER 
                        page.Footer()
                            .AlignCenter()
                            .Text(txt =>
                            {
                                txt.Span("Сформировано: ").FontColor("#94a3b8");
                                txt.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).FontColor("#64748b");
                            });
                    });
                }).GeneratePdf(filePath);
            });
        }

        // ====== Вспомогательные методы для стилизации таблиц PDF ======
        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .Background("#3b82f6")
                .Padding(8)
                .DefaultTextStyle(x => x.FontColor("#ffffff").Bold().FontSize(11));
        }

        private static IContainer DataCell(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor("#e2e8f0")
                .Padding(8);
        }
    }
}
