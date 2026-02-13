using Microsoft.Extensions.DependencyInjection;
using RealEstateCRM.Models.Entities;
using RealEstateCRM.Models.Common;

namespace RealEstateCRM.Data;

public static class DomainSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, string adminUserId, string managerUserId)
    {
        var db = serviceProvider.GetRequiredService<AppDbContext>();

        // Only seed if there's no existing data
        if (db.Clients.Any() || db.Properties.Any())
            return;

        // Create sample clients
        var client1 = new Client
        {
            FirstName = "John",
            LastName = "Smith", 
            Email = "john.smith@email.com",
            Phone = "+1234567890",
            Type = ClientType.Buyer,
            OwnerUserId = managerUserId
        };

        var client2 = new Client
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@email.com", 
            Phone = "+1987654321",
            Type = ClientType.Seller,
            OwnerUserId = managerUserId
        };

        db.Clients.AddRange(client1, client2);
        await db.SaveChangesAsync();

        // Create sample properties
        var property1 = new Property
        {
            Title = "Beautiful Downtown Apartment",
            Address = "123 Main Street, City Center",
            Type = PropertyType.Apartment,
            Price = 250000,
            Currency = "USD",
            AreaSqM = 85.5,
            Rooms = 3,
            Status = ListingStatus.Active,
            OwnerUserId = managerUserId,
            SellerClientId = client2.Id
        };

        var property2 = new Property
        {
            Title = "Spacious Family House",
            Address = "456 Oak Avenue, Suburbs",
            Type = PropertyType.House,
            Price = 450000,
            Currency = "USD",
            AreaSqM = 150.0,
            Rooms = 5,
            Status = ListingStatus.Active,
            OwnerUserId = adminUserId
        };

        db.Properties.AddRange(property1, property2);
        await db.SaveChangesAsync();

        // Create sample visits
        var visit1 = new Visit
        {
            PropertyId = property1.Id,
            ClientId = client1.Id,
            VisitAtLocal = DateTime.Now.AddDays(1),
            Notes = "First showing - client interested",
            OwnerUserId = managerUserId
        };

        var visit2 = new Visit
        {
            PropertyId = property2.Id,
            ClientId = client1.Id,
            VisitAtLocal = DateTime.Now.AddDays(2),
            Notes = "Second property viewing",
            OwnerUserId = managerUserId
        };

        db.Visits.AddRange(visit1, visit2);
        await db.SaveChangesAsync();

        // Create default app settings
        if (!db.AppSettings.Any())
        {
            var settings = new List<AppSetting>
            {
                new AppSetting
                {
                    Key = "Company.Name",
                    Value = "RealEstate CRM",
                    Description = "Име на компанията",
                    Category = "General"
                },
                new AppSetting
                {
                    Key = "Company.Email",
                    Value = "info@realestate.com",
                    Description = "Основен имейл адрес",
                    Category = "General"
                },
                new AppSetting
                {
                    Key = "Company.Phone",
                    Value = "+359 2 123 4567",
                    Description = "Телефон за контакт",
                    Category = "General"
                },
                new AppSetting
                {
                    Key = "System.DefaultCurrency",
                    Value = "USD",
                    Description = "Валута по подразбиране",
                    Category = "System"
                },
                new AppSetting
                {
                    Key = "System.ItemsPerPage",
                    Value = "20",
                    Description = "Брой записи на страница",
                    Category = "System"
                },
                new AppSetting
                {
                    Key = "Email.SmtpServer",
                    Value = "smtp.gmail.com",
                    Description = "SMTP сървър",
                    Category = "Email"
                },
                new AppSetting
                {
                    Key = "Email.SmtpPort",
                    Value = "587",
                    Description = "SMTP порт",
                    Category = "Email"
                },
                new AppSetting
                {
                    Key = "Notifications.EnableEmail",
                    Value = "false",
                    Description = "Включване на имейл известия",
                    Category = "Notifications"
                }
            };

            db.AppSettings.AddRange(settings);
            await db.SaveChangesAsync();
        }
    }
}