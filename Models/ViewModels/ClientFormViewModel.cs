using System.ComponentModel.DataAnnotations;
using RealEstateCRM.Models.Common;
using RealEstateCRM.Models.Clients;


namespace RealEstateCRM.Models.Clients
{
    public class ClientFormViewModel
    {
        public Guid? Id { get; set; }

        [Required, StringLength(60)]
        [Display(Name = "Име")]
        public string FirstName { get; set; } = "";

        [Required, StringLength(60)]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = "";

        [EmailAddress]
        [Display(Name = "Имейл")]
        public string? Email { get; set; }

        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        [Display(Name = "Тип клиент")]
        public ClientType Type { get; set; }

        [StringLength(1000)]
        [Display(Name = "Коментари")]
        public string? Comments { get; set; }
    }

    public class ClientListItemViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string? Email { get; set; } = "";
        public string? Phone { get; set; } = "";
        public ClientType Type { get; set; }
        public string? Comments { get; set; }
    }
}


