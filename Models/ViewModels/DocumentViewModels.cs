using RealEstateCRM.Models.Common;
using RealEstateCRM.Models.Entities;

namespace RealEstateCRM.Models.ViewModels;

public class DocumentSchemaFieldViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public string Label { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string? HelpText { get; set; }
    public string? Placeholder { get; set; }
}

public class DocumentsIndexViewModel
{
    public List<DocumentTemplate> Templates { get; set; } = new();
    public List<GeneratedDocument> RecentDocuments { get; set; } = new();
    public Guid? SelectedTemplateId { get; set; }
}

public class DynamicFormViewModel
{
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public DocumentTemplateType TemplateType { get; set; }
    public List<DocumentSchemaFieldViewModel> Fields { get; set; } = new();
    public Dictionary<string, string> PrefillValues { get; set; } = new();
    public Guid? RelatedEntityId { get; set; }
    public DocumentRelatedEntityType? RelatedEntityType { get; set; }
}

public class DocumentTemplateEditViewModel
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DocumentTemplateType TemplateType { get; set; }
    public string JsonSchema { get; set; } = "[]";
    public string? FileTemplatePath { get; set; }
    public bool IsActive { get; set; } = true;
}
