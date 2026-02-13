using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateCRM.Models.Identity;
using RealEstateCRM.Models.Entities;
using RealEstateCRM.Models.ViewModels;
using RealEstateCRM.Data;

namespace RealEstateCRM.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            AppDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    Roles = roles.ToList(),
                    IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                    LockoutEnd = user.LockoutEnd,
                    EmailConfirmed = user.EmailConfirmed
                });
            }

            return View(userViewModels);
        }

        // User Management
        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var model = new UserFormViewModel
            {
                AvailableRoles = roles.Where(r => r != null).Cast<string>().ToList()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetCreateUserForm()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var model = new UserFormViewModel
            {
                AvailableRoles = roles.Where(r => r != null).Cast<string>().ToList()
            };
            return PartialView("Form/_CreateUser", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).Where(r => r != null).Cast<string>().ToListAsync();
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = $"{model.FirstName} {model.LastName}",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password ?? "TempPass123!");

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.SelectedRole))
                {
                    await _userManager.AddToRoleAsync(user, model.SelectedRole);
                }

                // Log audit
                await LogAuditAsync("Create", "User", user.Id, $"Created user: {user.Email}");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Потребителят е създаден успешно!" });
                }

                TempData["Success"] = "Потребителят е създаден успешно!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).Where(r => r != null).Cast<string>().ToListAsync();
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, errors = result.Errors.Select(e => e.Description) });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).Where(r => r != null).Cast<string>().ToListAsync();

            var nameParts = user.FullName?.Split(' ', 2) ?? new string[] { "", "" };

            var model = new UserFormViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = nameParts.Length > 0 ? nameParts[0] : string.Empty,
                LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
                SelectedRole = roles.FirstOrDefault() ?? string.Empty,
                AvailableRoles = allRoles
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetEditUserForm(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).Where(r => r != null).Cast<string>().ToListAsync();

            var nameParts = user.FullName?.Split(' ', 2) ?? new string[] { "", "" };

            var model = new UserFormViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = nameParts.Length > 0 ? nameParts[0] : string.Empty,
                LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
                SelectedRole = roles.FirstOrDefault() ?? string.Empty,
                AvailableRoles = allRoles
            };

            return PartialView("Form/_EditUser", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).Where(r => r != null).Cast<string>().ToListAsync();
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id!);
            if (user == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, errors = new[] { "Потребителят не е намерен." } });
                }
                return NotFound();
            }

            user.Email = model.Email;
            user.UserName = model.Email;
            user.FullName = $"{model.FirstName} {model.LastName}";

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Update password if provided
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, model.Password);
                }

                // Update role
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!string.IsNullOrEmpty(model.SelectedRole))
                {
                    await _userManager.AddToRoleAsync(user, model.SelectedRole);
                }

                // Log audit
                await LogAuditAsync("Update", "User", user.Id, $"Updated user: {user.Email}");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Потребителят е обновен успешно!" });
                }

                TempData["Success"] = "Потребителят е обновен успешно!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).Where(r => r != null).Cast<string>().ToListAsync();
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, errors = result.Errors.Select(e => e.Description) });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["Error"] = "Не можете да изтриете собствения си акаунт!";
                return RedirectToAction(nameof(Index));
            }

            // Check if user owns any entities
            var hasClients = await _db.Clients.AnyAsync(c => c.OwnerUserId == id && !c.IsDeleted);
            var hasProperties = await _db.Properties.AnyAsync(p => p.OwnerUserId == id && !p.IsDeleted);
            var hasVisits = await _db.Visits.AnyAsync(v => v.OwnerUserId == id && !v.IsDeleted);

            if (hasClients || hasProperties || hasVisits)
            {
                var entities = new List<string>();
                if (hasClients) entities.Add("клиенти");
                if (hasProperties) entities.Add("имоти");
                if (hasVisits) entities.Add("посещения");
                
                TempData["Error"] = $"Не можете да изтриете потребител, който има свързани {string.Join(", ", entities)}. Моля, преназначете или изтрийте тези записи първо.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                // Log audit
                await LogAuditAsync("Delete", "User", id, $"Deleted user: {user.Email}");

                TempData["Success"] = "Потребителят е изтрит успешно!";
            }
            else
            {
                TempData["Error"] = "Грешка при изтриване на потребител!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLockout(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
            {
                // Unlock
                await _userManager.SetLockoutEndDateAsync(user, null);
                await LogAuditAsync("Unlock", "User", id, $"Unlocked user: {user.Email}");
                TempData["Success"] = "Потребителят е отключен!";
            }
            else
            {
                // Lock
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                await LogAuditAsync("Lock", "User", id, $"Locked user: {user.Email}");
                TempData["Success"] = "Потребителят е заключен!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Audit Logs
        public async Task<IActionResult> AuditLogs(int page = 1, string? action = null, string? entityType = null)
        {
            var pageSize = 50;
            var query = _db.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            var total = await query.CountAsync();
            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogViewModel
                {
                    Id = a.Id.ToString(),
                    UserEmail = a.UserEmail,
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    Details = a.Details,
                    Timestamp = a.Timestamp
                })
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Action = action;
            ViewBag.EntityType = entityType;

            return View(logs);
        }

        // App Settings
        public async Task<IActionResult> Settings()
        {
            var settings = await _db.AppSettings
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Key)
                .Select(s => new AppSettingViewModel
                {
                    Id = s.Id.ToString(),
                    Key = s.Key,
                    Value = s.Value,
                    Description = s.Description,
                    Category = s.Category
                })
                .ToListAsync();

            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSetting(string id, string value)
        {
            var setting = await _db.AppSettings.FindAsync(Guid.Parse(id));
            if (setting == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Настройката не е намерена." });
                }
                return NotFound();
            }

            var oldValue = setting.Value;
            setting.Value = value;
            await _db.SaveChangesAsync();

            // Log audit
            await LogAuditAsync("Update", "AppSetting", id, $"Changed {setting.Key} from '{oldValue}' to '{value}'");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Настройката е обновена!" });
            }

            TempData["Success"] = "Настройката е обновена!";
            return RedirectToAction(nameof(Settings));
        }

        [HttpGet]
        public async Task<IActionResult> GetEditSettingForm(string id)
        {
            var setting = await _db.AppSettings.FindAsync(Guid.Parse(id));
            if (setting == null)
            {
                return NotFound();
            }

            var model = new AppSettingViewModel
            {
                Id = setting.Id.ToString(),
                Key = setting.Key,
                Value = setting.Value,
                Description = setting.Description,
                Category = setting.Category
            };

            return PartialView("Form/_EditSetting", model);
        }

        // Helper method for audit logging
        private async Task LogAuditAsync(string action, string entityType, string entityId, string? details = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return;

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = currentUser.Id,
                UserEmail = currentUser.Email ?? string.Empty,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                Timestamp = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.AuditLogs.Add(auditLog);
            await _db.SaveChangesAsync();
        }
    }
}