using RealEstateCRM.Models.Common;

namespace RealEstateCRM.Models.Entities;

public class DocumentTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DocumentTemplateType TemplateType { get; set; }
    public string JsonSchema { get; set; } = "[]";
    public string? FileTemplatePath { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<GeneratedDocument> GeneratedDocuments { get; set; } = new List<GeneratedDocument>();
}
