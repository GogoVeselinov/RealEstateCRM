using RealEstateCRM.Models.Entities;
using RealEstateCRM.Models.ViewModels;


namespace RealEstateCRM.Models.ViewModels
{

    public class DashboardViewModel
    {
        public string GreetingName { get; set; } = string.Empty;
        public int PropertiesCount { get; set; }
        public int ClientsCount { get; set; }
        public int VisitsCount { get; set; }
        public List<Property> RecentProperties { get; set; } = new();
        public List<Visit> UpcomingVisits { get; set; } = new();
        
        // Property statistics
        public int ActivePropertiesCount { get; set; }
        public decimal ActivePropertiesValueSum { get; set; }
        public decimal ActivePropertiesAvgPrice { get; set; }
        public List<(string Name, int Count)> PropertiesByStatus { get; set; } = new();

        public List<SidebarItem> SidebarFeed { get; set; } = new();
    }
    public class SidebarItem
    {
        public string Kind { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Subtitle { get; set; }
        public DateTime When { get; set; }
        public string? LinkController { get; set; }
        public Guid? LinkId { get; set; }
    }

    public class PropertyStatusStats
    {
        public int ActivePropertiesCount { get; set; }
        public decimal ActivePropertiesValueSum { get; set; }
        public decimal ActivePropertiesAvgPrice { get; set; }

        public List<(string Name, int Count)> PropertiesByStatus { get; set; } = new();
    }
}
