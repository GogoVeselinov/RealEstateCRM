using RealEstateCRM.Models.Common;

namespace RealEstateCRM.Models.Entities
{
    public class AppSetting : BaseEntity
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = "General"; // General, Email, Security, etc.
    }
}
