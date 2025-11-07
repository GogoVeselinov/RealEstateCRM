using System.ComponentModel.DataAnnotations;
using RealEstateCRM.Models.Common;

namespace RealEstateCRM.Models.ViewModels
{
    public class PropertyFormViewModel
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";
        
        [Required]
        [StringLength(500)]
        public string Address { get; set; } = "";
        
        public PropertyType Type { get; set; }
        
        public ListingStatus Status { get; set; } = ListingStatus.Draft;
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "EUR";
        
        [Range(0.1, double.MaxValue, ErrorMessage = "Area must be greater than 0")]
        public double? AreaSqM { get; set; }
        
        [Range(1, 50, ErrorMessage = "Rooms must be between 1 and 50")]
        public int? Rooms { get; set; }
        
        public Guid? SellerClientId { get; set; }
    }
}