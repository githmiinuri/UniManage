using System.ComponentModel.DataAnnotations;

namespace UniManage3.Models
{
    public class RegistrationViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "University Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; }

        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        public string AddressLine2 { get; set; }

        [Display(Name = "Province")]
        public string Province { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        [Display(Name = "Contact Number")]
        [RegularExpression(@"^\d{10,}$", ErrorMessage = "Contact number must contain only digits and be at least 10 characters long.")]
        public string ContactNumber { get; set; }

        [Display(Name = "NIC Number")]
        public string NICNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "The {0} must be at least {2} characters long.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}