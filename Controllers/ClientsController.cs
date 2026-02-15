using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateCRM.Data;
using RealEstateCRM.Models.Clients;
using RealEstateCRM.Models.Identity;
using RealEstateCRM.Models.Common;
using RealEstateCRM.Models.Entities;
namespace RealEstateCRM.Controllers;
using RealEstateCRM.Models.ViewModels;
using System.Text.Json;

[Authorize]
public class ClientsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _um;

    public ClientsController(AppDbContext db, UserManager<ApplicationUser> um)
    {
        _db = db;
        _um = um;
    }

    public async Task<IActionResult> Index(string? q, ClientType? type)
    {
        var user = await _um.GetUserAsync(User);
        var isAdmin = await _um.IsInRoleAsync(user!, AppRoles.Admin);
        var userId = user!.Id;

        var clients = _db.Clients.AsNoTracking();

        if (!isAdmin)
            clients = clients.Where(c => c.OwnerUserId == userId);

        if (!string.IsNullOrWhiteSpace(q))
            clients = clients.Where(c => c.FirstName.Contains(q) || c.LastName.Contains(q) || (c.Email != null && c.Email.Contains(q)));

        if (type.HasValue)
            clients = clients.Where(c => c.Type == type.Value);

        var list = await clients
            .OrderBy(c => c.LastName)
            .Select(c => new ClientListItemViewModel
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                Type = c.Type,
                Comments = c.Comments
            }).ToListAsync();

        ViewBag.CanEdit = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);
        
        if (Request.Headers["X-Partial"] == "true")
            return PartialView("_Table", list);

        return View(list);
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    [HttpGet]
    public IActionResult Create() => PartialView("_Form", new ClientFormViewModel());

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return BadRequest(PartialView("_Form", vm));

        var user = await _um.GetUserAsync(User);
        var entity = new Client
        {
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            Email = vm.Email ?? "",
            Phone = vm.Phone ?? "",
            Type = vm.Type,
            Comments = vm.Comments,
            OwnerUserId = user!.Id
        };

        _db.Clients.Add(entity);

        // Audit Log
        CreateAuditLogEntry(user.Id, user.Email ?? "", "Create", "Client", entity.Id.ToString(), 
            JsonSerializer.Serialize(new { entity.FirstName, entity.LastName, entity.Email, entity.Phone, entity.Type }));

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var c = await _db.Clients.FindAsync(id);
        if (c == null) return NotFound();

        if (User.IsInRole(AppRoles.Manager))
        {
            var me = (await _um.GetUserAsync(User))!.Id;
            if (c.OwnerUserId != me) return Forbid();
        }

        var vm = new ClientFormViewModel
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            Phone = c.Phone,
            Type = c.Type,
            Comments = c.Comments
        };

        return PartialView("_Form", vm);
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ClientFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return BadRequest(PartialView("_Form", vm));

        var c = await _db.Clients.FindAsync(vm.Id);
        if (c == null) return NotFound();

        if (User.IsInRole(AppRoles.Manager))
        {
            var me = (await _um.GetUserAsync(User))!.Id;
            if (c.OwnerUserId != me) return Forbid();
        }

        var oldData = JsonSerializer.Serialize(new { c.FirstName, c.LastName, c.Email, c.Phone, c.Type, c.Comments });

        c.FirstName = vm.FirstName;
        c.LastName = vm.LastName;
        c.Email = vm.Email ?? "";
        c.Phone = vm.Phone ?? "";
        c.Type = vm.Type;
        c.Comments = vm.Comments;

        // Audit Log
        var user = await _um.GetUserAsync(User);
        var newData = JsonSerializer.Serialize(new { c.FirstName, c.LastName, c.Email, c.Phone, c.Type, c.Comments });
        CreateAuditLogEntry(user!.Id, user.Email ?? "", "Update", "Client", c.Id.ToString(), 
            $"Old: {oldData} | New: {newData}");

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    [HttpDelete, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var c = await _db.Clients.FindAsync(id);
        if (c == null) return NotFound();

        if (User.IsInRole(AppRoles.Manager))
        {
            var me = (await _um.GetUserAsync(User))!.Id;
            if (c.OwnerUserId != me) return Forbid();
        }

        c.IsDeleted = true;

        // Audit Log
        var user = await _um.GetUserAsync(User);
        CreateAuditLogEntry(user!.Id, user.Email ?? "", "Delete", "Client", c.Id.ToString(), 
            JsonSerializer.Serialize(new { c.FirstName, c.LastName, c.Email }));

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    private void CreateAuditLogEntry(string userId, string userEmail, string action, string entityType, string entityId, string? details)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            Timestamp = DateTime.UtcNow
        };
        _db.AuditLogs.Add(auditLog);
    }
}
