using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Antiforgery;
using RealEstateCRM.Data;
using RealEstateCRM.Models.Identity;
using RealEstateCRM.Models.Entities;
using RealEstateCRM.Models.Common;
using RealEstateCRM.Models.ViewModels;

namespace RealEstateCRM.Controllers
{
    [Authorize] // виждат само логнати; Create/Edit/Delete ограничени допълнително по роли
    public class PropertiesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _um;
        private readonly IAntiforgery _antiforgery;

        public PropertiesController(AppDbContext db, UserManager<ApplicationUser> um, IAntiforgery antiforgery)
        {
            _db = db; _um = um; _antiforgery = antiforgery;
        }

        // List + филтри + partial reload
        public async Task<IActionResult> Index(string? q, PropertyType? type, ListingStatus? status)
        {
            var user = await _um.GetUserAsync(User);
            var isAdmin = await _um.IsInRoleAsync(user!, AppRoles.Admin);
            var userId = user!.Id;

            var props = _db.Properties.AsNoTracking();

            if (!isAdmin)
                props = props.Where(p => p.OwnerUserId == userId);

            if (!string.IsNullOrWhiteSpace(q))
                props = props.Where(p => p.Title.Contains(q) || p.Address.Contains(q));

            if (type.HasValue)
                props = props.Where(p => p.Type == type.Value);

            if (status.HasValue)
                props = props.Where(p => p.Status == status.Value);

            var list = await props
                .OrderByDescending(p => p.CreatedAtUtc)
                .Take(500)
                .Select(p => new PropertyListItemViewModel {
                    Id = p.Id, Title = p.Title, Address = p.Address, Type = p.Type,
                    Status = p.Status, Price = p.Price, Currency = p.Currency, CreatedAtUtc = p.CreatedAtUtc
                })
                .ToListAsync();

            ViewBag.CanEdit = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);
            
            if (Request.Headers["X-Partial"] == "true")
                return PartialView("_Table", list);

            return View(list);
        }

        // GET Create form (modal)
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_Form", new PropertyFormViewModel());
        }

        // POST Create
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyFormViewModel vm)
        {
            if (!ModelState.IsValid) return BadRequest(PartialView("_Form", vm));

            var user = await _um.GetUserAsync(User);
            var entity = new Property
            {
                Title = vm.Title.Trim(),
                Address = vm.Address.Trim(),
                Type = vm.Type,
                Status = vm.Status,
                Price = vm.Price,
                Currency = vm.Currency.Trim(),
                AreaSqM = vm.AreaSqM,
                Rooms = vm.Rooms,
                SellerClientId = vm.SellerClientId,
                OwnerUserId = user!.Id
            };
            _db.Properties.Add(entity);
            await _db.SaveChangesAsync();

            return Ok(new { ok = true });
        }

        // GET Edit form (modal)
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var p = await _db.Properties.FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();

            // Guard: Manager може да редактира само свои (Admin -> всичко)
            if (User.IsInRole(AppRoles.Manager))
            {
                var me = (await _um.GetUserAsync(User))!.Id;
                if (p.OwnerUserId != me) return Forbid();
            }

            var vm = new PropertyFormViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Address = p.Address,
                Type = p.Type,
                Status = p.Status,
                Price = p.Price,
                Currency = p.Currency,
                AreaSqM = p.AreaSqM,
                Rooms = p.Rooms,
                SellerClientId = p.SellerClientId
            };
            return PartialView("_Form", vm);
        }

        // POST Edit
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PropertyFormViewModel vm)
        {
            if (!ModelState.IsValid) return BadRequest(PartialView("_Form", vm));

            var p = await _db.Properties.FirstOrDefaultAsync(x => x.Id == vm.Id);
            if (p == null) return NotFound();

            if (User.IsInRole(AppRoles.Manager))
            {
                var me = (await _um.GetUserAsync(User))!.Id;
                if (p.OwnerUserId != me) return Forbid();
            }

            p.Title = vm.Title.Trim();
            p.Address = vm.Address.Trim();
            p.Type = vm.Type;
            p.Status = vm.Status;
            p.Price = vm.Price;
            p.Currency = vm.Currency.Trim();
            p.AreaSqM = vm.AreaSqM;
            p.Rooms = vm.Rooms;
            p.SellerClientId = vm.SellerClientId;

            await _db.SaveChangesAsync();
            return Ok(new {
                ok = true,
                id = p.Id,
                title = p.Title,
                address = p.Address,
                type = p.Type.ToString(),
                status = p.Status.ToString(),
                price = p.Price,
                currency = p.Currency
            });
        }

        // DELETE (soft delete през IsDeleted)
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
        [HttpDelete, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var p = await _db.Properties.FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();

            if (User.IsInRole(AppRoles.Manager))
            {
                var me = (await _um.GetUserAsync(User))!.Id;
                if (p.OwnerUserId != me) return Forbid();
            }

            p.IsDeleted = true;
            await _db.SaveChangesAsync();
            return Ok(new {
                ok = true,
                id = p.Id,
                title = p.Title,
                address = p.Address,
                type = p.Type.ToString(),
                status = p.Status.ToString(),
                price = p.Price,
                currency = p.Currency
            });
        }

        // AJAX inline edit (accepts JSON)
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
        [HttpPost]
        public async Task<IActionResult> AjaxEdit([FromBody] InlineEditDto dto)
        {
            // Quick diagnostics: ensure header is present
            var headerToken = Request.Headers["RequestVerificationToken"].ToString();
            if (string.IsNullOrWhiteSpace(headerToken))
            {
                return BadRequest("Missing RequestVerificationToken header");
            }

            // Validate antiforgery token from header (client sends RequestVerificationToken header)
            try
            {
                await _antiforgery.ValidateRequestAsync(HttpContext);
            }
            catch (AntiforgeryValidationException ex)
            {
                return BadRequest("Invalid antiforgery token: " + ex.Message);
            }

            if (!ModelState.IsValid)
            {
                var errs = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(s => !string.IsNullOrWhiteSpace(s)));
                if (string.IsNullOrWhiteSpace(errs)) errs = "Invalid payload";
                return BadRequest(errs);
            }

            if (dto == null) return BadRequest("Invalid payload: request body could not be parsed");
            if (dto.Id == Guid.Empty) return BadRequest("Missing or invalid Id");

            var p = await _db.Properties.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (p == null) return NotFound();

            if (User.IsInRole(AppRoles.Manager))
            {
                var me = (await _um.GetUserAsync(User))!.Id;
                if (p.OwnerUserId != me) return Forbid();
            }

            // Only update provided fields
            if (dto.Title != null) p.Title = dto.Title.Trim();
            if (dto.Address != null) p.Address = dto.Address.Trim();
            if (dto.Price.HasValue) p.Price = dto.Price.Value;
            if (dto.Currency != null) p.Currency = dto.Currency.Trim();
            if (dto.Type.HasValue) p.Type = dto.Type.Value;
            if (dto.Status.HasValue) p.Status = dto.Status.Value;

            await _db.SaveChangesAsync();
            return Ok(new {
                ok = true,
                id = p.Id,
                title = p.Title,
                address = p.Address,
                type = p.Type.ToString(),
                status = p.Status.ToString(),
                price = p.Price,
                currency = p.Currency
            });
        }

        public class InlineEditDto
        {
            public Guid Id { get; set; }
            public string? Title { get; set; }
            public string? Address { get; set; }
            public decimal? Price { get; set; }
            public string? Currency { get; set; }
            public PropertyType? Type { get; set; }
            public ListingStatus? Status { get; set; }
        }
    }
}
