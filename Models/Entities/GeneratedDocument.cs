using RealEstateCRM.Models.Common;

namespace RealEstateCRM.Models.Entities;

public class GeneratedDocument : BaseEntity
{
    public Guid TemplateId { get; set; }
    public DocumentTemplate? DocumentTemplate { get; set; }

    public Guid? RelatedEntityId { get; set; }
    public DocumentRelatedEntityType? RelatedEntityType { get; set; }

    public string JsonData { get; set; } = "{}";
    public string FilePath { get; set; } = string.Empty;
}
