using RealEstateCRM.Models.Common;
using RealEstateCRM.Models.Identity;

namespace RealEstateCRM.Models.Entities;

public enum ExpenseCategory { Office, Marketing, Utilities, SalaryBonus, Other }

public class Expense : BaseEntity
{
    public ExpenseCategory Category { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
}

public class PayrollSettings : BaseEntity
{
    public decimal BaseSalary { get; set; }
    public decimal CommissionPercentage { get; set; }
    public decimal VisitBonus { get; set; }
    public bool IsGlobal { get; set; } = true;
}

public class ManagerPayrollOverride : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal CommissionPercentage { get; set; }
    public decimal VisitBonus { get; set; }
}
