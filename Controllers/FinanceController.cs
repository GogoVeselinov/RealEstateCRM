using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateCRM.Data;
using RealEstateCRM.Models.Common;
using RealEstateCRM.Models.Entities;
using RealEstateCRM.Models.Identity;

namespace RealEstateCRM.Controllers;

[Authorize]
public class FinanceController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _um;

    public FinanceController(AppDbContext db, UserManager<ApplicationUser> um)
    {
        _db = db;
        _um = um;
    }

    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var user = await _um.GetUserAsync(User);
        var isAdmin = await _um.IsInRoleAsync(user!, AppRoles.Admin);

        // Default to current month if not specified
        if (!from.HasValue) from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        if (!to.HasValue) to = from.Value.AddMonths(1).AddSeconds(-1);

        // Get income data (VisitPayments in period)
        var payments = await _db.VisitPayments
            .Where(p => !p.IsDeleted && p.CreatedAtUtc >= from && p.CreatedAtUtc <= to)
            .Include(p => p.Visit)
            .ThenInclude(v => v.Property)
            .Include(p => p.Visit)
            .ThenInclude(v => v.OwnerUser)
            .ToListAsync();

        var totalIncome = payments.Sum(p => p.Amount);

        // Get expenses in period
        var expenses = await _db.Expenses
            .Where(e => !e.IsDeleted && e.CreatedAtUtc >= from && e.CreatedAtUtc <= to)
            .ToListAsync();

        var totalExpenses = expenses.Sum(e => e.Amount);
        var netProfit = totalIncome - totalExpenses;

        // Get global payroll settings
        var payrollSettings = await _db.PayrollSettings
            .Where(p => p.IsGlobal && !p.IsDeleted)
            .FirstOrDefaultAsync() ?? new PayrollSettings
            {
                BaseSalary = 2000,
                CommissionPercentage = 5,
                VisitBonus = 50,
                IsGlobal = true
            };

        // Get all managers (users with Manager role)
        var managers = await _um.GetUsersInRoleAsync(AppRoles.Manager);
        var payrollData = new List<dynamic>();

        foreach (var manager in managers)
        {
            // Get manager's payroll override or use global
            var override_ = await _db.ManagerPayrollOverrides
                .Where(m => m.UserId == manager.Id && !m.IsDeleted)
                .FirstOrDefaultAsync();

            var baseSalary = override_?.BaseSalary ?? payrollSettings.BaseSalary;
            var commPercentage = override_?.CommissionPercentage ?? payrollSettings.CommissionPercentage;
            var visitBonus = override_?.VisitBonus ?? payrollSettings.VisitBonus;

            // Calculate commission and bonus for period
            var managerPayments = payments.Where(p => p.Visit.OwnerUserId == manager.Id).Sum(p => p.Amount);
            var commissionAmount = (managerPayments * commPercentage) / 100;

            var completedVisits = await _db.Visits
                .Where(v => !v.IsDeleted && v.OwnerUserId == manager.Id && v.Status == VisitStatus.Completed && v.CreatedAtUtc >= from && v.CreatedAtUtc <= to)
                .CountAsync();

            var visitBonusAmount = completedVisits * visitBonus;
            var finalSalary = baseSalary + commissionAmount + visitBonusAmount;

            payrollData.Add(new
            {
                ManagerName = manager.UserName,
                BaseSalary = baseSalary,
                CommissionPercentage = commPercentage,
                CommissionAmount = commissionAmount,
                VisitBonus = visitBonus,
                CompletedVisits = completedVisits,
                VisitBonusAmount = visitBonusAmount,
                FinalSalary = finalSalary
            });
        }

        ViewBag.From = from;
        ViewBag.To = to;
        ViewBag.TotalIncome = totalIncome;
        ViewBag.TotalExpenses = totalExpenses;
        ViewBag.NetProfit = netProfit;
        ViewBag.Payments = payments;
        ViewBag.Expenses = expenses;
        ViewBag.PayrollData = payrollData;
        ViewBag.IsAdmin = isAdmin;

        return View();
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpGet]
    public IActionResult AddExpense()
    {
        return PartialView("_ExpenseForm");
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> AddExpense(ExpenseCategory category, decimal amount, string description)
    {
        if (amount <= 0)
            return BadRequest("Amount must be greater than 0");

        var user = await _um.GetUserAsync(User);
        var expense = new Expense
        {
            Category = category,
            Amount = amount,
            Description = description.Trim(),
            CreatedByUserId = user!.Id
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();

        return Ok(new { ok = true, id = expense.Id });
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id);
        if (expense == null) return NotFound();

        expense.IsDeleted = true;
        await _db.SaveChangesAsync();

        return Ok(new { ok = true });
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> EditPayrollSettings()
    {
        var settings = await _db.PayrollSettings
            .Where(p => p.IsGlobal && !p.IsDeleted)
            .FirstOrDefaultAsync() ?? new PayrollSettings { IsGlobal = true };

        return PartialView("_PayrollSettingsForm", settings);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> SavePayrollSettings(decimal baseSalary, decimal commissionPercentage, decimal visitBonus)
    {
        if (baseSalary < 0 || commissionPercentage < 0 || visitBonus < 0)
            return BadRequest("Values cannot be negative");

        var settings = await _db.PayrollSettings
            .Where(p => p.IsGlobal && !p.IsDeleted)
            .FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new PayrollSettings
            {
                BaseSalary = baseSalary,
                CommissionPercentage = commissionPercentage,
                VisitBonus = visitBonus,
                IsGlobal = true
            };
            _db.PayrollSettings.Add(settings);
        }
        else
        {
            settings.BaseSalary = baseSalary;
            settings.CommissionPercentage = commissionPercentage;
            settings.VisitBonus = visitBonus;
        }

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }
}
