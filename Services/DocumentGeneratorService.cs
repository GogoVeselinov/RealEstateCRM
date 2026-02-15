using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RealEstateCRM.Models.Entities;
using RealEstateCRM.Models.ViewModels;

namespace RealEstateCRM.Services;

public class DocumentGeneratorService : IDocumentGeneratorService
{
    private readonly IWebHostEnvironment _environment;

    public DocumentGeneratorService(IWebHostEnvironment environment)
    {
        _environment = environment;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> GeneratePdfAsync(DocumentTemplate template, string jsonData)
    {
        var values = JsonSerializer.Deserialize<Dictionary<string, object?>>(jsonData) ?? new();
        var schemaFields = ParseSchema(template.JsonSchema);
        
        Console.WriteLine($"[PDF Generator] Template: {template.Name}");
        Console.WriteLine($"[PDF Generator] Schema fields: {string.Join(", ", schemaFields.Select(f => f.Name))}");
        Console.WriteLine($"[PDF Generator] Data keys: {string.Join(", ", values.Keys)}");

        var now = DateTime.UtcNow;
        var folder = Path.Combine(_environment.WebRootPath, "documents", now.Year.ToString(), now.Month.ToString("00"));
        Directory.CreateDirectory(folder);

        var safeTemplateName = string.Join("_", template.Name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        var fileName = $"{safeTemplateName}_{Guid.NewGuid():N}.pdf";
        var fullPath = Path.Combine(folder, fileName);

        // Generate styled PDF
        GenerateStyledPdf(fullPath, template, schemaFields, values, now);

        var relativePath = $"/documents/{now.Year}/{now.Month:00}/{fileName}";
        return await Task.FromResult(relativePath);
    }

    private void GenerateStyledPdf(string fullPath, DocumentTemplate template, List<DocumentSchemaFieldViewModel> schemaFields, Dictionary<string, object?> values, DateTime generatedAt)
    {
        string GetValue(string key)
        {
            if (values.TryGetValue(key, out var val) && val != null)
            {
                var str = val.ToString();
                return string.IsNullOrWhiteSpace(str) ? "___________" : str;
            }
            return "___________";
        }

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(12).LineHeight(1.5f));

                // Header
                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text(template.Name.ToUpper())
                        .Bold().FontSize(16);
                    column.Item().PaddingTop(5).AlignCenter().Text($"Сключен на {DateTime.Today:dd.MM.yyyy} г.")
                        .FontSize(11).Italic();
                    column.Item().PaddingTop(10).LineHorizontal(2);
                });

                // Content
                page.Content().PaddingTop(20).Column(column =>
                {
                    // Section: Contract Parties
                    column.Item().Text("СТРАНИ ПО ДОГОВОРА").Bold().FontSize(13).Underline();
                    column.Item().PaddingTop(10);

                    // Data table
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(180);
                            columns.RelativeColumn();
                        });

                        foreach (var field in schemaFields)
                        {
                            var value = GetValue(field.Name);

                            table.Cell().Border(1).BorderColor("#000").Background("#f5f5f5")
                                .Padding(8).Text(field.Label).Bold();
                            
                            table.Cell().Border(1).BorderColor("#000")
                                .Padding(8).Text(value);
                        }
                    });

                    // Articles section (if Brokerage Contract)
                    if (template.Name.Contains("Brokerage") || template.Name.Contains("Посредничество"))
                    {
                        column.Item().PaddingTop(20).Text("ПРЕДМЕТ НА ДОГОВОРА").Bold().FontSize(13).Underline();
                        column.Item().PaddingTop(10);

                        column.Item().Text(text =>
                        {
                            text.Span("Член 1. ").Bold();
                            text.Span($"Клиентът възлага, а Агенцията приема да извърши посредничество при продажбата на недвижим имот, находящ се на адрес: ");
                            text.Span(GetValue("PropertyAddress")).Bold();
                            text.Span($", при посочена продажна цена от ");
                            text.Span(GetValue("ListingPrice") + " лв.").Bold();
                        });

                        column.Item().PaddingTop(10).Text(text =>
                        {
                            text.Span("Член 2. Задължения на Агенцията. ").Bold();
                            text.Span("Агенцията се задължава да рекламира имота, организира огледи, съдейства при преговори и сключване на договори.");
                        });

                        column.Item().PaddingTop(10).Text(text =>
                        {
                            text.Span("Член 3. Възнаграждение. ").Bold();
                            text.Span($"При успешно сключване на сделка, Клиентът заплаща комисионна в размер на ");
                            text.Span(GetValue("CommissionPercentage") + "%").Bold();
                            text.Span(" от продажната цена.");
                        });

                        column.Item().PaddingTop(10).Text(text =>
                        {
                            text.Span("Член 4. Срок на действие. ").Bold();
                            text.Span($"Договорът има срок на действие ");
                            text.Span(GetValue("ContractDurationMonths") + " месеца").Bold();
                            text.Span($" от ");
                            text.Span(GetValue("ContractStartDate")).Bold();
                            text.Span(".");
                        });
                    }

                    // Signatures
                    column.Item().PaddingTop(40).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Агенция:");
                            col.Item().PaddingTop(40).LineHorizontal(1);
                            col.Item().PaddingTop(4).Text(GetValue("AgencyName")).AlignCenter().FontSize(10);
                        });

                        row.ConstantItem(40);

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Клиент:");
                            col.Item().PaddingTop(40).LineHorizontal(1);
                            col.Item().PaddingTop(4).Text(GetValue("ClientName")).AlignCenter().FontSize(10);
                        });
                    });
                });

                // Footer
                page.Footer().AlignCenter().Text($"Генериран от RealEstate CRM на {generatedAt:dd.MM.yyyy HH:mm}")
                    .FontSize(9).FontColor("#999");
            });
        }).GeneratePdf(fullPath);
    }

    private static List<DocumentSchemaFieldViewModel> ParseSchema(string jsonSchema)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var fields = JsonSerializer.Deserialize<List<DocumentSchemaFieldViewModel>>(jsonSchema, options) ?? new List<DocumentSchemaFieldViewModel>();

            for (var index = 0; index < fields.Count; index++)
            {
                var field = fields[index];
                
                // Don't overwrite if name is already valid
                if (string.IsNullOrWhiteSpace(field.Name))
                {
                    field.Name = NormalizeFieldKey(field.Name, field.Label, index + 1);
                }
                field.Type = string.IsNullOrWhiteSpace(field.Type) ? "text" : field.Type.Trim().ToLowerInvariant();
                field.Label = string.IsNullOrWhiteSpace(field.Label) ? field.Name : field.Label.Trim();
            }

            return fields;
        }
        catch
        {
            return new List<DocumentSchemaFieldViewModel>();
        }
    }

    private static string NormalizeFieldKey(string? name, string? label, int index)
    {
        var candidate = string.IsNullOrWhiteSpace(name) ? label : name;
        if (string.IsNullOrWhiteSpace(candidate))
            candidate = $"Field{index}";

        var chars = candidate
            .Trim()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')
            .ToArray();

        var normalized = new string(chars).Trim('_');
        if (string.IsNullOrWhiteSpace(normalized))
            normalized = $"Field{index}";

        return normalized;
    }
}
