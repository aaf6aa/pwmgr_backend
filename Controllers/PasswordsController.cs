using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pwmgr_backend.Data;
using pwmgr_backend.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

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

        // GET: api/passwords
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<PasswordMetadataResponse>), 200)]
        public async Task<ActionResult<IEnumerable<PasswordMetadataResponse>>> GetPasswords()
        {
            var userId = GetUserId();

            var passwordEntries = await _context.PasswordEntries
                .Where(pe => pe.UserId == userId)
                .ToListAsync();

            var response = passwordEntries.Select(pe => new PasswordMetadataResponse
            {
                Id = pe.Id,
                EncryptedMetadata = pe.EncryptedMetadata
            });

            return Ok(response);
        }

        // POST: api/passwords
        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(201)]
        public async Task<ActionResult> AddPassword([FromBody] PasswordDTO request)
        {
            var userId = GetUserId();

            var passwordEntry = new PasswordEntry
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EncryptedMetadata = request.EncryptedMetadata,
                EncryptedPassword = request.EncryptedPassword,
                EncryptedPasswordKey = request.EncryptedPasswordKey,
                HkdfSalt = request.HkdfSalt
            };

            _context.PasswordEntries.Add(passwordEntry);
            await _context.SaveChangesAsync();

            var response = new { id = passwordEntry.Id };
      
            return CreatedAtAction(nameof(GetPasswordById), response, response);
        }

        // GET: api/passwords/{id}
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PasswordDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PasswordDTO>> GetPasswordById(Guid id)
        {
            var userId = GetUserId();

            var passwordEntry = await _context.PasswordEntries
                .FirstOrDefaultAsync(pe => pe.Id == id && pe.UserId == userId);

            if (passwordEntry == null)
            {
                return NotFound(new { message = "Password entry not found." });
            }

            var response = new PasswordDTO
            {
                EncryptedMetadata = passwordEntry.EncryptedMetadata,
                EncryptedPassword = passwordEntry.EncryptedPassword,
                EncryptedPasswordKey = passwordEntry.EncryptedPasswordKey,
                HkdfSalt = passwordEntry.HkdfSalt
            };

            return Ok(response);
        }

        // DELETE: api/passwords/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> DeletePassword(Guid id)
        {
            var userId = GetUserId();

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

        // Helper method to extract UserId from JWT
        private Guid GetUserId()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("An authorized invalid GUID encountered!! This is not possible!");
            // User ID cannot be null because the user is authenticated
            return Guid.Parse(userIdString);
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
    }
}
