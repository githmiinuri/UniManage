namespace UniManage3.Models
{
    public class Lecturer
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int RoleId { get; set; }

        // Additional profile fields
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? ContactNumber { get; set; }
        public string NICNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public int? ZipCode { get; set; }
    }
}