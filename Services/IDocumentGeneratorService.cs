using RealEstateCRM.Models.Entities;

namespace RealEstateCRM.Services;

public interface IDocumentGeneratorService
{
    Task<string> GeneratePdfAsync(DocumentTemplate template, string jsonData);
}
