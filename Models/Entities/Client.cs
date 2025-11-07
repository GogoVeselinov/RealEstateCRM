using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealEstateCRM.Models.Common;
using RealEstateCRM.Models.Identity;

namespace RealEstateCRM.Models.Entities
{
    // Domain/Clients/Client.cs
    public class Client : BaseEntity
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public ClientType Type { get; set; }
        public string? Comments { get; set; } // Коментари за комуникация между мениджърите

        public string OwnerUserId { get; set; } = default!;
        public ApplicationUser? OwnerUser { get; set; }

        // навигации
        public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    }

}