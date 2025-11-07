using RealEstateCRM.Models.Common;

namespace RealEstateCRM.Models.ViewModels
{
    public class PropertyListItemViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Address { get; set; } = "";
        public PropertyType Type { get; set; }
        public ListingStatus Status { get; set; } = ListingStatus.Draft;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "EUR";
        public DateTime CreatedAtUtc { get; set; }
    }
}
