using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateCRM.Models.Common
{
    // Domain/Common/BaseEntity.cs
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedByUserId { get; set; }
        public DateTime? ModifiedAtUtc { get; set; }
        public string? ModifiedByUserId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}