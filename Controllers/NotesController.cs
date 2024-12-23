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
    [Authorize]
    [ApiController]
    [Route("api/notes")]
    [Produces("application/json")]
    public class NotesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<NoteMetadataResponse>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<NoteMetadataResponse>>> GetNotes()
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return Unauthorized();

            var notes = await _context.Notes
                .Where(n => n.UserId == userId)
                .Select(n => new NoteMetadataResponse
                {
                    Id = n.Id,
                    EncryptedMetadata = n.EncryptedMetadata,
                })
                .ToListAsync();

            return Ok(notes);
        }

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<object>> AddNote([FromBody] NoteDTO request)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return Unauthorized();

            // Check for duplicate title hash
            var existingNote = await _context.Notes
                .FirstOrDefaultAsync(n => n.UserId == userId && n.TitleHash == request.TitleHash);
            if (existingNote != null)
            {
                return Conflict(new { message = "A note with this title already exists." });
            }    

            var note = new Note
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EncryptedMetadata = request.EncryptedMetadata,
                EncryptedNote = request.EncryptedNote,
                EncryptedNoteKey = request.EncryptedNoteKey,
                HkdfSalt = request.HkdfSalt,
                TitleHash = request.TitleHash,
                Hmac = request.Hmac
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            var response = new { id = note.Id };
            return CreatedAtAction(nameof(AddNote), response, response);
        }

        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(NoteDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<NoteDTO>> GetNoteById([FromRoute] Guid id)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return Unauthorized();

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (note == null)
            {
                return NotFound(new { message = "Note not found." });
            }

            return Ok(new NoteDTO
            {
                EncryptedMetadata = note.EncryptedMetadata,
                EncryptedNote = note.EncryptedNote,
                EncryptedNoteKey = note.EncryptedNoteKey,
                HkdfSalt = note.HkdfSalt,
                TitleHash = note.TitleHash,
                Hmac = note.Hmac
            });
        }

        
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(NoteDTO), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> UpdateNote([FromRoute] Guid id, [FromBody] NoteDTO request)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return Unauthorized();

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (note == null)
            {
                return NotFound(new { message = "Note not found." });
            }

            // Check for duplicate title hash, excluding current note
            var existingNote = await _context.Notes
                .FirstOrDefaultAsync(n => n.UserId == userId && n.Id != id && n.TitleHash == request.TitleHash);
            if (existingNote != null)
            {
                return Conflict(new { message = "Another note with this title already exists." });
            }

            note.EncryptedMetadata = request.EncryptedMetadata!;
            note.EncryptedNote = request.EncryptedNote!;
            note.EncryptedNoteKey = request.EncryptedNoteKey!;
            note.HkdfSalt = request.HkdfSalt;
            note.TitleHash = request.TitleHash!;
            note.Hmac = request.Hmac!;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteNote([FromRoute] Guid id)
        {
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out Guid userId))
                return Unauthorized();

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (note == null)
            {
                return NotFound(new { message = "Note not found." });
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// A response DTO for note metadata.
        /// </summary>
        public class NoteMetadataResponse
        {
            [NotNull]
            public Guid Id { get; set; }
            [NotNull]
            [Base64String]
            public string? EncryptedMetadata { get; set; }
        }

        /// <summary>
        /// A request/response DTO for encrypted notes.
        /// </summary>
        public class NoteDTO
        {
            [NotNull]
            [Base64String]
            public string? EncryptedMetadata { get; set; }
            [NotNull]
            [Base64String]
            public string? EncryptedNote { get; set; }
            [NotNull]
            [Base64String]
            public string? EncryptedNoteKey { get; set; }
            [NotNull]
            [Base64String]
            public string? HkdfSalt { get; set; }
            [NotNull]
            [Base64String]
            public string? TitleHash { get; set; }
            [NotNull]
            [Base64String]
            public string? Hmac { get; set; }
        }
    }
}
