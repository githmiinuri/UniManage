using System.ComponentModel.DataAnnotations;

namespace UniManage3.Models.ViewModels
{
    public class ProfileUpdateViewModel
    {
        [Display(Name = "First Name")]
        [Required, StringLength(128)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required, StringLength(128)]
        public string LastName { get; set; }

        [Display(Name = "Contact Number")]
        [Phone]
        public string ContactNumber { get; set; }

        [Display(Name = "NIC Number")]
        [StringLength(64)]
        public string NICNumber { get; set; }

        [Display(Name = "Address Line 1")]
        [StringLength(256)]
        public string AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        [StringLength(256)]
        public string AddressLine2 { get; set; }

        [Display(Name = "Province")]
        [StringLength(128)]
        public string Province { get; set; }

        [Display(Name = "City")]
        [StringLength(128)]
        public string City { get; set; }

        [Display(Name = "ZIP Code")]
        public int? ZipCode { get; set; }
    }

    public class EmailUpdateViewModel
    {
        [Display(Name = "Current Email")]
        [Required, EmailAddress]
        public string CurrentEmail { get; set; }

        [Display(Name = "New Email")]
        [Required, EmailAddress]
        public string NewEmail { get; set; }
    }

    public class PasswordUpdateViewModel
    {
        [Display(Name = "Current Password")]
        [Required, DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Display(Name = "New Password")]
        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [Required, DataType(DataType.Password), Compare("NewPassword", ErrorMessage = "The new password and confirmation do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LecturerProfileIndexViewModel
    {
        [Display(Name = "Profile")]
        public ProfileUpdateViewModel Profile { get; set; } = new ProfileUpdateViewModel();

        [Display(Name = "Email")]
        public EmailUpdateViewModel Email { get; set; } = new EmailUpdateViewModel();

        [Display(Name = "Password")]
        public PasswordUpdateViewModel Password { get; set; } = new PasswordUpdateViewModel();
    }
}
