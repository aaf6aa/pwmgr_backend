using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace pwmgr_backend.Models
{
    public class PasswordEntry
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        // Encrypted Metadata
        [NotNull]
        [Required]
        [Base64String]
        public string? EncryptedMetadata { get; set; }

        // Encrypted Password
        [NotNull]
        [Required]
        [Base64String]
        public string? EncryptedPassword { get; set; }

        // Encrypted Password Key
        [NotNull]
        [Required]
        [Base64String]
        public string? EncryptedPasswordKey { get; set; }

        // HKDF Salt
        [NotNull]
        [Required]
        [Base64String]
        public string? HkdfSalt { get; set; }

        // Hash of the service and username
        [NotNull]
        [Required]
        [Base64String]
        public string? ServiceUsernameHash { get; set; }

        // Integrity HMAC
        [NotNull]
        [Required]
        [Base64String]
        public string? Hmac { get; set; }

        // Navigation property
        [NotNull]
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
