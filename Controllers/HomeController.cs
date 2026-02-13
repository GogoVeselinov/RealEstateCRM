using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateCRM.Data;
using RealEstateCRM.Models.ViewModels;
using RealEstateCRM.Models.Identity;
using RealEstateCRM.Models.Common;

namespace RealEstateCRM.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }
        
        var userId = user.Id;
        var isAdmin = await _userManager.IsInRoleAsync(user, AppRoles.Admin);

        // Guard за филтриране по собственик (админ вижда всичко)
        IQueryable<T> OwnOrAll<T>(IQueryable<T> q) where T : class
        {
            // динамичен филтър според тип (Property/Client/Visit) за OwnerUserId
            if (isAdmin) return q;

            if (typeof(T).Name == "Property")
                return (IQueryable<T>)(object)_db.Properties.Where(p => p.OwnerUserId == userId);
            if (typeof(T).Name == "Client")
                return (IQueryable<T>)(object)_db.Clients.Where(c => c.OwnerUserId == userId);
            if (typeof(T).Name == "Visit")
                return (IQueryable<T>)(object)_db.Visits.Where(v => v.OwnerUserId == userId);

            return q;
        }

        // Totals
        var propsQ = OwnOrAll(_db.Properties.AsNoTracking());
        var clientsQ = OwnOrAll(_db.Clients.AsNoTracking());
        var visitsQ = OwnOrAll(_db.Visits.AsNoTracking());

        // За feed-а взимаме последни 5 имота, последни 5 клиента и следващи 10 огледа
        var recentProps = await propsQ
            .OrderByDescending(p => p.CreatedAtUtc)
            .Take(5)
            .Select(p => new SidebarItem
            {
                Kind = "Property",
                Title = p.Title,
                Subtitle = p.Address,
                When = p.CreatedAtUtc,
                LinkController = "Properties",
                LinkId = p.Id
            }).ToListAsync();

        var recentClients = await clientsQ
            .OrderByDescending(c => c.CreatedAtUtc)
            .Take(5)
            .Select(c => new SidebarItem
            {
                Kind = "Client",
                Title = c.FirstName + " " + c.LastName,
                Subtitle = string.IsNullOrWhiteSpace(c.Email) ? c.Phone : c.Email,
                When = c.CreatedAtUtc,
                LinkController = null, // ако имаш Clients/Edit, сложи "Clients" и LinkId=c.Id
                LinkId = null
            }).ToListAsync();

        var upcomingVisits = await visitsQ
            .Where(v => v.VisitAtLocal >= DateTime.Today)
            .OrderBy(v => v.VisitAtLocal)
            .Include(v => v.Property)
            .Include(v => v.Client)
            .Take(10)
            .Select(v => new SidebarItem
            {
                Kind = "Visit",
                Title = v.Property != null ? v.Property.Title : "Visit",
                Subtitle = v.Client != null ? (v.Client.FirstName + " " + v.Client.LastName) : v.Notes,
                When = v.VisitAtLocal,
                LinkController = "Properties",
                LinkId = v.PropertyId
            }).ToListAsync();

        var activeQ = propsQ.Where(p => p.Status == ListingStatus.Active);
        var activePrices = await activeQ.Select(p => p.Price).ToListAsync();

        var byStatus = await propsQ
            .GroupBy(p => p.Status)
            .Select(g => new { Name = g.Key.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();


        // Подреди комбинирано (по време, най-новите/скорошните отгоре)
        var sidebar = recentProps
            .Concat(recentClients)
            .Concat(upcomingVisits)
            .OrderByDescending(x => x.When)
            .Take(12)
            .ToList();


        var model = new DashboardViewModel
        {
            GreetingName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? user.UserName ?? "User" : user.FullName,
            PropertiesCount = await propsQ.CountAsync(),
            ClientsCount = await clientsQ.CountAsync(),
            VisitsCount = await visitsQ.CountAsync(),
            RecentProperties = await _db.Properties.AsNoTracking()
                                    .Where(p => isAdmin || p.OwnerUserId == userId)
                                    .OrderByDescending(p => p.CreatedAtUtc)
                                    .Take(5)
                                    .ToListAsync(),
            UpcomingVisits = await _db.Visits.AsNoTracking()
                                    .Where(v => (isAdmin || v.OwnerUserId == userId) && v.VisitAtLocal >= DateTime.Today)
                                    .OrderBy(v => v.VisitAtLocal)
                                    .Take(5)
                                    .Include(v => v.Property)
                                    .Include(v => v.Client)
                                    .ToListAsync(),
            ActivePropertiesCount = await activeQ.CountAsync(),
            ActivePropertiesValueSum = activePrices.Sum(),
            ActivePropertiesAvgPrice = activePrices.Count > 0 ? activePrices.Average() : 0,
            PropertiesByStatus = byStatus.Select(x => (x.Name, x.Count)).ToList()
        };
        model.SidebarFeed = sidebar;
        return View(model);
    }
}
