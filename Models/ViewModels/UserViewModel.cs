using System.ComponentModel.DataAnnotations;

namespace RealEstateCRM.Models.ViewModels
{
    public class UserListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }

    public class UserFormViewModel
    {
        public string? Id { get; set; }
        
        [Required(ErrorMessage = "Имейл адресът е задължителен")]
        [EmailAddress(ErrorMessage = "Въведете валиден имейл адрес")]
        [Display(Name = "Имейл")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Името е задължително")]
        [Display(Name = "Име")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилията е задължителна")]
        [Display(Name = "Фамилия")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Парола")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Паролата трябва да бъде между {2} и {1} символа")]
        public string? Password { get; set; }

        [Display(Name = "Потвърди парола")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Роля")]
        public string SelectedRole { get; set; } = string.Empty;

        public List<string> AvailableRoles { get; set; } = new();
    }

    public class ChangeRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string NewRole { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        public string UserId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Новата парола е задължителна")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Паролата трябва да бъде между {2} и {1} символа")]
        [Display(Name = "Нова парола")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Паролите не съвпадат")]
        [Display(Name = "Потвърди парола")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
