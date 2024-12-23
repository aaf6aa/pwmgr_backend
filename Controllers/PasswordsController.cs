using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pwmgr_backend.Data;
using pwmgr_backend.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace pwmgr_backend.Controllers
{
    [ApiController]
    [Route("api/passwords")]
    [Authorize]
    public class PasswordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PasswordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all password entries for the authenticated user.
        /// </summary>
        /// <returns>
        /// - 200 OK: Returns a list of password metadata.
        /// - 401 Unauthorized: If the JWT token is missing, invalid, or expired.
        /// </returns>
        /// <example>
        /// Request:
        /// GET api/passwords
        /// Authorization: Bearer {jwtToken}
        /// Response:
        /// [
        ///     {
        ///         "id": "passwordGuid",
        ///         "encryptedMetadata": "base64EncodedEncryptedMetadata"
        ///     }
        /// ]
        /// </example>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<PasswordMetadataResponse>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<PasswordMetadataResponse>>> GetPasswords()
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return Unauthorized();

            var passwords = await _context.PasswordEntries
                .Where(pe => pe.UserId == userId)
                .Select(pe => new PasswordMetadataResponse
                {
                    Id = pe.Id,
                    EncryptedMetadata = pe.EncryptedMetadata
                })
                .ToListAsync();

            return Ok(passwords);
        }

        /// <summary>
        /// Adds a new password entry for the authenticated user.
        /// </summary>
        /// <param name="request">The password entry data.</param>
        /// <returns>
        /// - 201 Created: Returns the ID of the created password entry.
        /// - 401 Unauthorized: If the JWT token is missing, invalid, or expired.
        /// - 409 Conflict: If a password entry with the same service and username already exists.
        /// </returns>
        /// <example>
        /// Request:
        /// POST api/passwords
        /// Authorization: Bearer {jwtToken}
        /// {
        ///     "encryptedMetadata": "base64EncodedEncryptedMetadata",
        ///     "encryptedPassword": "base64EncodedEncryptedPassword",
        ///     "encryptedPasswordKey": "base64EncodedEncryptedKey",
        ///     "hkdfSalt": "base64EncodedHKDFSalt",
        ///     "serviceUsernameHash": "base64EncodedHMACHash",
        ///     "hmac": "base64EncodedHMACHash"
        /// }
        /// Response:
        /// {
        ///     "id": "passwordId"
        /// }
        /// </example>
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        public async Task<ActionResult> AddPassword([FromBody] PasswordDTO request)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return Unauthorized();

            // Check if a password entry with the same hash already exists for this user
            var existingEntry = await _context.PasswordEntries
                .FirstOrDefaultAsync(pe => pe.UserId == userId && pe.ServiceUsernameHash == request.ServiceUsernameHash);

            if (existingEntry != null)
            {
                // Return 409 Conflict if the hash already exists
                return Conflict(new { message = "An entry with the same service and username already exists." });
            }

            // Create a new password entry
            var passwordEntry = new PasswordEntry
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EncryptedMetadata = request.EncryptedMetadata,
                EncryptedPassword = request.EncryptedPassword,
                EncryptedPasswordKey = request.EncryptedPasswordKey,
                HkdfSalt = request.HkdfSalt,
                ServiceUsernameHash = request.ServiceUsernameHash,
                Hmac = request.Hmac
            };

            _context.PasswordEntries.Add(passwordEntry);
            await _context.SaveChangesAsync();

            var response = new { id = passwordEntry.Id };
            return CreatedAtAction(nameof(AddPassword), response, response);
        }

        /// <summary>
        /// Retrieves a specific password entry by ID for the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the password entry.</param>
        /// <returns>
        /// - 200 OK: Returns the password entry data.
        /// - 401 Unauthorized: If the JWT token is missing, invalid, or expired.
        /// - 404 Not Found: If the password entry does not exist or does not belong to the user.
        /// </returns>
        /// <example>
        /// Request:
        /// GET api/passwords/{id}
        /// Authorization: Bearer {jwtToken}
        /// Response:
        /// {
        ///     "encryptedMetadata": "base64EncodedEncryptedMetadata",
        ///     "encryptedPassword": "base64EncodedEncryptedPassword",
        ///     "encryptedPasswordKey": "base64EncodedEncryptedKey",
        ///     "hkdfSalt": "base64EncodedHKDFSalt",
        ///     "serviceUsernameHash": "base64EncodedHMACHash",
        ///     "hmac": "base64EncodedHMACHash"
        /// }
        /// </example>
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PasswordDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PasswordDTO>> GetPasswordById([FromRoute] Guid id)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return Unauthorized();

            var passwordEntry = await _context.PasswordEntries
                .FirstOrDefaultAsync(pe => pe.Id == id && pe.UserId == userId);

            if (passwordEntry == null)
            {
                return NotFound(new { message = "Password entry not found." });
            }

            return Ok(new PasswordDTO
            {
                EncryptedMetadata = passwordEntry.EncryptedMetadata,
                EncryptedPassword = passwordEntry.EncryptedPassword,
                EncryptedPasswordKey = passwordEntry.EncryptedPasswordKey,
                HkdfSalt = passwordEntry.HkdfSalt,
                ServiceUsernameHash = passwordEntry.ServiceUsernameHash,
                Hmac = passwordEntry.Hmac,
            });
        }

        /// <summary>
        /// Deletes a specific password entry by ID for the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the password entry.</param>
        /// <returns>
        /// - 204 No Content: If the password entry was successfully deleted.
        /// - 401 Unauthorized: If the JWT token is missing, invalid, or expired.
        /// - 404 Not Found: If the password entry does not exist or does not belong to the user.
        /// </returns>
        /// <example>
        /// Request:
        /// DELETE api/passwords/{id}
        /// Authorization: Bearer {jwtToken}
        /// </example>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> DeletePassword([FromRoute] Guid id)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return Unauthorized();

            var passwordEntry = await _context.PasswordEntries
                .FirstOrDefaultAsync(pe => pe.Id == id && pe.UserId == userId);

            if (passwordEntry == null)
            {
                return NotFound(new { message = "Password entry not found." });
            }

            _context.PasswordEntries.Remove(passwordEntry);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    /// <summary>
    /// A response DTO for password metadata.
    /// </summary>
    public class PasswordMetadataResponse
    {
        [NotNull]
        public Guid Id { get; set; }
        [NotNull]
        [Base64String]
        public string? EncryptedMetadata { get; set; }
    }

    /// <summary>
    /// A request/response DTO for encrypted passwords.
    /// </summary>
    public class PasswordDTO
    {
        [NotNull]
        [Base64String]
        public string? EncryptedMetadata { get; set; }
        [NotNull]
        [Base64String]
        public string? EncryptedPassword { get; set; }
        [NotNull]
        [Base64String]
        public string? EncryptedPasswordKey { get; set; }
        [NotNull]
        [Base64String]
        public string? HkdfSalt { get; set; }
        [NotNull]
        [Base64String]
        public string? ServiceUsernameHash { get; set; }
        [NotNull]
        [Base64String]
        public string? Hmac { get; set; }
    }
}