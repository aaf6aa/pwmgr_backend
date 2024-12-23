using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace pwmgr_backend.Models
{
    public class Note
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        // Encrypted Metadata
        [NotNull]
        [Required]
        [Base64String]
        public string? EncryptedMetadata { get; set; }

        // Encrypted Body
        [NotNull]
        [Required]
        [Base64String]
        public string? EncryptedNote { get; set; }

        // Encrypted Body Key
        [NotNull]
        [Required]
        [Base64String]
        public string? EncryptedNoteKey { get; set; }

        // HKDF Salt
        [NotNull]
        [Required]
        [Base64String]
        public string? HkdfSalt { get; set; }

        // Hash of the title
        [NotNull]
        [Required]
        [Base64String]
        public string? TitleHash { get; set; }

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
