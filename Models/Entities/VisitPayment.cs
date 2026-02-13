using RealEstateCRM.Models.Common;

namespace RealEstateCRM.Models.Entities;

public enum PaymentType { ViewingFee, Commission, Deposit, Other }
public enum PaymentMethod { Cash, BankTransfer, POS, Other }

public class VisitPayment : BaseEntity
{
    public Guid VisitId { get; set; }
    public Visit Visit { get; set; } = null!;

    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    public string? Notes { get; set; }
    public string? FilePath { get; set; }

    public string OwnerUserId { get; set; } = string.Empty;
}
