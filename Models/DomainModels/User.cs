using System;
using System.ComponentModel.DataAnnotations;

namespace AuthManagerEnterprise.Models.DomainModels
{
    public class User
    {
        public int Id { get; set; }

        [Required] // IMPORTANT: Required field
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required] // NOTE: Plain password is allowed for this task
        public string Password { get; set; } = string.Empty;

        // unverified | active | blocked
        public string Status { get; set; } = "unverified";

        public DateTime? LastLoginTime { get; set; }

        public DateTime RegistrationTime { get; set; } = DateTime.Now;
    }
}
