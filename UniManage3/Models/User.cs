using System;

namespace UniManage3.Models
{
    public class User
    {
        public int Id { get; set; }

        // Full name (First + Last)
        public string FullName { get; set; }

        public string Email { get; set; }

        // password (hashed)
        public string Password { get; set; }

        public int RoleId { get; set; }

        public bool IsActive { get; set; }

        public bool IsApproved { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLogin { get; set; }
    }
}
