using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealEstateCRM.Models.Common;
using RealEstateCRM.Models.Identity;

namespace RealEstateCRM.Models.Entities
{
    // Domain/Properties/Property.cs
    public class Property : BaseEntity
    {
        public string Title { get; set; } = default!;
        public string Address { get; set; } = default!;
        public PropertyType Type { get; set; }

        public decimal Price { get; set; }            // офертна цена
        public string Currency { get; set; } = "EUR"; // прост стринг за кратко

        public double? AreaSqM { get; set; }          // квадратура
        public int? Rooms { get; set; }

        public ListingStatus Status { get; set; } = ListingStatus.Draft;

        // Отговорен брокер
        public string OwnerUserId { get; set; } = default!;
        public ApplicationUser? OwnerUser { get; set; }

        // по желание: Seller клиент (ако има)
        public Guid? SellerClientId { get; set; }
        public Client? SellerClient { get; set; }

        public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    }

}