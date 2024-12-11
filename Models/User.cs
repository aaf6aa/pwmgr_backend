using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace pwmgr_backend.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [NotNull]
        [Required]
        [MaxLength(50)]
        public string? Username { get; set; }

        [NotNull]
        [Required]
        public string? PasswordHash { get; set; }

        [NotNull]
        [Required]

        public string? MasterSalt { get; set; } 

        // Navigation property
        public ICollection<PasswordEntry> PasswordEntries { get; set; } = Array.Empty<PasswordEntry>();
    }
}
