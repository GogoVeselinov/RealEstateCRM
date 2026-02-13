using RealEstateCRM.Models.Common;
using RealEstateCRM.Models.Identity;

namespace RealEstateCRM.Models.Entities
{
    public class AuditLog : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // Create, Update, Delete, Login, etc.
        public string EntityType { get; set; } = string.Empty; // Client, Property, User, etc.
        public string EntityId { get; set; } = string.Empty;
        public string? Details { get; set; } // JSON с промените
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public ApplicationUser? User { get; set; }
    }
}
