using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RealEstateCRM.Data;
using RealEstateCRM.Models.Common;
using RealEstateCRM.Models.Entities;
using RealEstateCRM.Models.Identity;
using RealEstateCRM.Models.ViewModels;
using System.Text.Json;

namespace RealEstateCRM.Controllers;

[Authorize]
public class VisitsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _um;

    public VisitsController(AppDbContext db, UserManager<ApplicationUser> um)
    {
        _db = db; _um = um;
    }

    public async Task<IActionResult> Index(DateTime? from, DateTime? to, VisitStatus? status, Guid? propertyId, Guid? clientId, string? q)
    {
        var user = await _um.GetUserAsync(User);
        var isAdmin = await _um.IsInRoleAsync(user!, AppRoles.Admin);
        var userId = user!.Id;

        var visits = _db.Visits
            .AsNoTracking()
            .Include(v => v.Property)
            .Include(v => v.Client)
            .Where(v => !v.IsDeleted);

        if (!isAdmin)
            visits = visits.Where(v => v.OwnerUserId == userId);

        if (from.HasValue) visits = visits.Where(v => v.VisitAtLocal >= from.Value);
        if (to.HasValue)   visits = visits.Where(v => v.VisitAtLocal <= to.Value);
        if (status.HasValue) visits = visits.Where(v => v.Status == status.Value);
        if (propertyId.HasValue) visits = visits.Where(v => v.PropertyId == propertyId.Value);
        if (clientId.HasValue)   visits = visits.Where(v => v.ClientId == clientId.Value);
        if (!string.IsNullOrWhiteSpace(q))
            visits = visits.Where(v => (v.Notes ?? "").Contains(q) || (v.Outcome ?? "").Contains(q));

        var list = await visits
            .OrderBy(v => v.VisitAtLocal)
            .Take(500)
            .Select(v => new VisitListItemViewModel
            {
                Id = v.Id,
                VisitAtLocal = v.VisitAtLocal,
                DurationMin = v.DurationMin,
                PropertyTitle = v.Property.Title,
                PropertyId = v.PropertyId,
                ClientName = v.Client == null ? null : (v.Client.FirstName + " " + v.Client.LastName),
                Status = v.Status,
                Notes = v.Notes
            })
            .ToListAsync();

        ViewBag.CanEdit = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);

        if (Request.Headers["X-Partial"] == "true")
            return PartialView("_Table", list);

        // dropdown-и за филтри и форми
        var propsQ = _db.Properties.AsNoTracking();
        var clientsQ = _db.Clients.AsNoTracking();
        if (!isAdmin)
        {
            propsQ = propsQ.Where(p => p.OwnerUserId == userId);
            clientsQ = clientsQ.Where(c => c.OwnerUserId == userId);
        }

        ViewBag.Properties = new SelectList(await propsQ.OrderBy(p => p.Title).Select(p => new { p.Id, p.Title }).ToListAsync(), "Id", "Title");
        ViewBag.Clients = new SelectList(await clientsQ.OrderBy(c => c.LastName).Select(c => new { c.Id, Name = c.FirstName + " " + c.LastName }).ToListAsync(), "Id", "Name");

        return View(list);
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await FillSelectsAsync();
        return PartialView("_Form", new VisitFormViewModel { VisitAtLocal = DateTime.Now.AddHours(2) });
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VisitFormViewModel vm)
    {
        await ValidateOverlap(vm);
        if (!ModelState.IsValid)
        {
            await FillSelectsAsync(vm.PropertyId, vm.ClientId);
            return BadRequest(PartialView("_Form", vm));
        }

        var me = (await _um.GetUserAsync(User))!.Id;

        var entity = new Models.Entities.Visit
        {
            PropertyId = vm.PropertyId,
            ClientId = vm.ClientId,
            VisitAtLocal = vm.VisitAtLocal,
            DurationMin = vm.DurationMin,
            Status = vm.Status,
            Location = vm.Location,
            Notes = vm.Notes,
            Outcome = vm.Outcome,
            NextActionAt = vm.NextActionAt,
            OwnerUserId = me
        };
        _db.Visits.Add(entity);

        // Audit Log
        CreateAuditLogEntry(me, (await _um.FindByIdAsync(me))?.Email ?? "", "Create", "Visit", entity.Id.ToString(), 
            JsonSerializer.Serialize(new { entity.PropertyId, entity.ClientId, entity.VisitAtLocal, entity.DurationMin, entity.Status }));

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var v = await _db.Visits.FirstOrDefaultAsync(x => x.Id == id);
        if (v == null) return NotFound();

        if (User.IsInRole(AppRoles.Manager))
        {
            var me = (await _um.GetUserAsync(User))!.Id;
            if (v.OwnerUserId != me) return Forbid();
        }

        var vm = new VisitFormViewModel
        {
            Id = v.Id,
            PropertyId = v.PropertyId,
            ClientId = v.ClientId,
            VisitAtLocal = v.VisitAtLocal,
            DurationMin = v.DurationMin,
            Status = v.Status,
            Location = v.Location,
            Notes = v.Notes,
            Outcome = v.Outcome,
            NextActionAt = v.NextActionAt
        };

        await FillSelectsAsync(vm.PropertyId, vm.ClientId);
        return PartialView("_Form", vm);
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VisitFormViewModel vm)
    {
        await ValidateOverlap(vm);
        if (!ModelState.IsValid)
        {
            await FillSelectsAsync(vm.PropertyId, vm.ClientId);
            return BadRequest(PartialView("_Form", vm));
        }

        var v = await _db.Visits.FirstOrDefaultAsync(x => x.Id == vm.Id);
        if (v == null) return NotFound();

        if (User.IsInRole(AppRoles.Manager))
        {
            var me = (await _um.GetUserAsync(User))!.Id;
            if (v.OwnerUserId != me) return Forbid();
        }

        var oldData = JsonSerializer.Serialize(new { v.PropertyId, v.ClientId, v.VisitAtLocal, v.DurationMin, v.Status, v.Notes, v.Outcome });

        v.PropertyId = vm.PropertyId;
        v.ClientId = vm.ClientId;
        v.VisitAtLocal = vm.VisitAtLocal;
        v.DurationMin = vm.DurationMin;
        v.Status = vm.Status;
        v.Location = vm.Location;
        v.Notes = vm.Notes;
        v.Outcome = vm.Outcome;
        v.NextActionAt = vm.NextActionAt;

        // Audit Log
        var user = await _um.GetUserAsync(User);
        var newData = JsonSerializer.Serialize(new { v.PropertyId, v.ClientId, v.VisitAtLocal, v.DurationMin, v.Status, v.Notes, v.Outcome });
        CreateAuditLogEntry(user!.Id, user.Email ?? "", "Update", "Visit", v.Id.ToString(), 
            $"Old: {oldData} | New: {newData}");

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    [HttpDelete, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var v = await _db.Visits.FirstOrDefaultAsync(x => x.Id == id);
        if (v == null) return NotFound();

        if (User.IsInRole(AppRoles.Manager))
        {
            var me = (await _um.GetUserAsync(User))!.Id;
            if (v.OwnerUserId != me) return Forbid();
        }

        v.IsDeleted = true;

        // Audit Log
        var user = await _um.GetUserAsync(User);
        CreateAuditLogEntry(user!.Id, user.Email ?? "", "Delete", "Visit", v.Id.ToString(), 
            JsonSerializer.Serialize(new { v.PropertyId, v.ClientId, v.VisitAtLocal }));

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    // Бърза смяна на статус
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(Guid id, VisitStatus status)
    {
        var v = await _db.Visits.FirstOrDefaultAsync(x => x.Id == id);
        if (v == null) return NotFound();

        if (User.IsInRole(AppRoles.Manager))
        {
            var me = (await _um.GetUserAsync(User))!.Id;
            if (v.OwnerUserId != me) return Forbid();
        }

        var oldStatus = v.Status;
        v.Status = status;
        if (status == VisitStatus.Completed && v.Outcome == null)
            v.Outcome = "Проведен оглед.";

        // Audit Log
        var user = await _um.GetUserAsync(User);
        CreateAuditLogEntry(user!.Id, user.Email ?? "", "Update", "Visit", v.Id.ToString(), 
            $"Status changed from {oldStatus} to {status}");

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    // Details page for visit with payments
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var v = await _db.Visits
            .Include(x => x.Property)
            .Include(x => x.Client)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        
        if (v == null) return NotFound();

        if (User.IsInRole(AppRoles.Manager))
        {
            var me = (await _um.GetUserAsync(User))!.Id;
            if (v.OwnerUserId != me) return Forbid();
        }

        return View(v);
    }

    // Payment form partial
    [HttpGet]
    public async Task<IActionResult> PaymentForm(Guid visitId = default)
    {
        var visit = await _db.Visits.FirstOrDefaultAsync(x => x.Id == visitId);
        return PartialView("_PaymentForm", visit ?? new Visit { Id = visitId });
    }

    // Payment endpoints
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    [HttpPost]
    public async Task<IActionResult> AddPayment([FromForm] Guid visitId, [FromForm] decimal amount, [FromForm] PaymentType type, [FromForm] PaymentMethod method, [FromForm] string? notes, [FromForm] IFormFile? file)
    {
        var visit = await _db.Visits.FirstOrDefaultAsync(x => x.Id == visitId && !x.IsDeleted);
        if (visit == null) return BadRequest("Visit not found");

        if (User.IsInRole(AppRoles.Manager))
        {
            var me = (await _um.GetUserAsync(User))!.Id;
            if (visit.OwnerUserId != me) return Forbid();
        }

        string? filePath = null;
        if (file != null && file.Length > 0)
        {
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "payments");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var fullPath = Path.Combine(uploadsDir, fileName);
            
            using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);

            filePath = $"/uploads/payments/{fileName}";
        }

        var me2 = (await _um.GetUserAsync(User))!.Id;
        var payment = new VisitPayment
        {
            VisitId = visitId,
            Amount = amount,
            PaymentType = type,
            PaymentMethod = method,
            Notes = notes,
            FilePath = filePath,
            OwnerUserId = me2
        };

        _db.VisitPayments.Add(payment);

        // Audit Log
        CreateAuditLogEntry(me2, (await _um.FindByIdAsync(me2))?.Email ?? "", "Create", "VisitPayment", payment.Id.ToString(), 
            JsonSerializer.Serialize(new { payment.VisitId, payment.Amount, payment.PaymentType, payment.PaymentMethod }));

        await _db.SaveChangesAsync();

        var payments = await _db.VisitPayments
            .Where(p => p.VisitId == visitId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync();

        return PartialView("_PaymentsTable", payments);
    }

    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
    [HttpDelete]
    public async Task<IActionResult> DeletePayment(Guid paymentId)
    {
        var payment = await _db.VisitPayments.FirstOrDefaultAsync(x => x.Id == paymentId);
        if (payment == null) return NotFound();

        if (User.IsInRole(AppRoles.Manager))
        {
            var me = (await _um.GetUserAsync(User))!.Id;
            if (payment.OwnerUserId != me) return Forbid();
        }

        payment.IsDeleted = true;

        // Audit Log
        var user = await _um.GetUserAsync(User);
        CreateAuditLogEntry(user!.Id, user.Email ?? "", "Delete", "VisitPayment", payment.Id.ToString(), 
            JsonSerializer.Serialize(new { payment.VisitId, payment.Amount }));

        await _db.SaveChangesAsync();

        var payments = await _db.VisitPayments
            .Where(p => p.VisitId == payment.VisitId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync();

        return PartialView("_PaymentsTable", payments);
    }

    // ICS export (календър)
    [HttpGet]
    public async Task<IActionResult> Ics(Guid id)
    {
        var v = await _db.Visits.Include(x => x.Property).Include(x => x.Client).FirstOrDefaultAsync(x => x.Id == id);
        if (v == null) return NotFound();

        var dtStart = v.VisitAtLocal.ToString("yyyyMMdd\\THHmmss");
        var dtEnd = v.VisitAtLocal.AddMinutes(v.DurationMin).ToString("yyyyMMdd\\THHmmss");
        var title = $"Оглед: {v.Property.Title}";
        var location = v.Location ?? v.Property.Address;
        var desc = (v.Client != null ? $"Клиент: {v.Client.FirstName} {v.Client.LastName}\\n" : "") + (v.Notes ?? "");

        var ics = $"BEGIN:VCALENDAR\r\nVERSION:2.0\r\nBEGIN:VEVENT\r\nDTSTART:{dtStart}\r\nDTEND:{dtEnd}\r\nSUMMARY:{title}\r\nLOCATION:{location}\r\nDESCRIPTION:{desc}\r\nEND:VEVENT\r\nEND:VCALENDAR";
        var bytes = System.Text.Encoding.UTF8.GetBytes(ics);
        return File(bytes, "text/calendar", "visit.ics");
    }

    // Helpers
    private async Task FillSelectsAsync(Guid? propertyId = null, Guid? clientId = null)
    {
        var user = await _um.GetUserAsync(User);
        var isAdmin = await _um.IsInRoleAsync(user!, AppRoles.Admin);
        var userId = user!.Id;

        var propsQ = _db.Properties.AsNoTracking();
        var clientsQ = _db.Clients.AsNoTracking();
        if (!isAdmin)
        {
            propsQ = propsQ.Where(p => p.OwnerUserId == userId);
            clientsQ = clientsQ.Where(c => c.OwnerUserId == userId);
        }

        ViewBag.Properties = new SelectList(await propsQ.OrderBy(p => p.Title).Select(p => new { p.Id, p.Title }).ToListAsync(), "Id", "Title", propertyId);
        ViewBag.Clients = new SelectList(await clientsQ.OrderBy(c => c.LastName).Select(c => new { c.Id, Name = c.FirstName + " " + c.LastName }).ToListAsync(), "Id", "Name", clientId);
    }

    private async Task ValidateOverlap(VisitFormViewModel vm)
    {
        var me = (await _um.GetUserAsync(User))!.Id;
        var end = vm.VisitAtLocal.AddMinutes(vm.DurationMin);
        var overlaps = await _db.Visits.AnyAsync(v =>
            v.OwnerUserId == me &&
            v.Id != vm.Id &&
            !v.IsDeleted &&
            v.VisitAtLocal < end &&
            v.VisitAtLocal.AddMinutes(v.DurationMin) > vm.VisitAtLocal);

        if (overlaps)
            ModelState.AddModelError(nameof(vm.VisitAtLocal), "Имате друг оглед в този интервал.");
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
