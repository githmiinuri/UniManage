using System.ComponentModel.DataAnnotations;

namespace UniManage3.Models
{
    public class Lecturer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Required, StringLength(128)]
        public string FirstName { get; set; }

        [Required, StringLength(128)]
        public string LastName { get; set; }

        public int? ContactNumber { get; set; }

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
}
