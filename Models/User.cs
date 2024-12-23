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
        [Base64String]
        public string? MasterSalt { get; set; } 

        // Navigation properties
        public ICollection<PasswordEntry> PasswordEntries { get; set; } = Array.Empty<PasswordEntry>();
        public ICollection<Note> Notes { get; set; } = Array.Empty<Note>();
    }
}
