using System.ComponentModel.DataAnnotations;
using RealEstateCRM.Models.Common;

namespace RealEstateCRM.Models.ViewModels;

public class VisitFormViewModel
{
    public Guid? Id { get; set; }

    [Display(Name = "Имот"), Required]
    public Guid PropertyId { get; set; }

    [Display(Name = "Клиент")]
    public Guid? ClientId { get; set; }

    [Display(Name = "Дата и час"), Required]
    public DateTime VisitAtLocal { get; set; } = DateTime.Now.AddHours(2);

    [Display(Name = "Продължителност (мин.)"), Range(10, 300)]
    public int DurationMin { get; set; } = 45;

    [Display(Name = "Статус"), Required]
    public VisitStatus Status { get; set; } = VisitStatus.Scheduled;

    [Display(Name = "Локация")]
    public string? Location { get; set; }

    [Display(Name = "Бележка")]
    public string? Notes { get; set; }

    [Display(Name = "Резултат")]
    public string? Outcome { get; set; }

    [Display(Name = "Следващо действие")]
    public DateTime? NextActionAt { get; set; }
}

public class VisitListItemViewModel
{
    public Guid Id { get; set; }
    public DateTime VisitAtLocal { get; set; }
    public int DurationMin { get; set; }
    public string PropertyTitle { get; set; } = "";
    public Guid PropertyId { get; set; }
    public string? ClientName { get; set; }
    public VisitStatus Status { get; set; }
    public string? Notes { get; set; }
}