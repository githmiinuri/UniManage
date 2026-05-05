using System.ComponentModel.DataAnnotations;

namespace UniManage3.Models.ViewModels
{
    public class ProfileUpdateViewModel
    {
        [Required, StringLength(128)]
        public string FirstName { get; set; }

        [Required, StringLength(128)]
        public string LastName { get; set; }

        [Phone]
        public string ContactNumber { get; set; }

        [StringLength(64)]
        public string NICNumber { get; set; }

        [StringLength(256)]
        public string AddressLine1 { get; set; }

        [StringLength(256)]
        public string AddressLine2 { get; set; }

        [StringLength(128)]
        public string Province { get; set; }

        [StringLength(128)]
        public string City { get; set; }

        public int? ZipCode { get; set; }
    }

    public class EmailUpdateViewModel
    {
        [Required, EmailAddress]
        public string CurrentEmail { get; set; }

        [Required, EmailAddress]
        public string NewEmail { get; set; }
    }

    public class PasswordUpdateViewModel
    {
        [Required, DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Required, DataType(DataType.Password), Compare("NewPassword", ErrorMessage = "The new password and confirmation do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LecturerProfileIndexViewModel
    {
        public ProfileUpdateViewModel Profile { get; set; } = new ProfileUpdateViewModel();
        public EmailUpdateViewModel Email { get; set; } = new EmailUpdateViewModel();
        public PasswordUpdateViewModel Password { get; set; } = new PasswordUpdateViewModel();
    }
}
