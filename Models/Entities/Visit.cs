using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealEstateCRM.Models.Common;
using RealEstateCRM.Models.Identity;
using RealEstateCRM.Models.ViewModels;

namespace RealEstateCRM.Models.Entities
{
    public class Visit : BaseEntity
    {
        public Guid PropertyId { get; set; }
        public Property Property { get; set; } = default!;

        public Guid? ClientId { get; set; }
        public Client? Client { get; set; }

        public DateTime VisitAtLocal { get; set; }
        public int DurationMin { get; set; } = 45;

        public VisitStatus Status { get; set; } = VisitStatus.Scheduled;

        public string? Location { get; set; } // ако искаш различен адрес
        public string? Notes { get; set; }
        public string? Outcome { get; set; }
        public DateTime? NextActionAt { get; set; }

        public string OwnerUserId { get; set; } = default!;
        public ApplicationUser? OwnerUser { get; set; }

        public ICollection<VisitPayment> Payments { get; set; } = new List<VisitPayment>();
    }
}