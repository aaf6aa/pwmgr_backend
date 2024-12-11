using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pwmgr_backend.Data;
using pwmgr_backend.Helpers;
using pwmgr_backend.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace pwmgr_backend.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/register
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            // Check if the username already exists
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
            {
                return BadRequest(new { message = "User already exists." });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = ArgonHelper.HashPassword(request.Username.ToLower(), request.Password, _configuration),
                MasterSalt = request.MasterSalt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate JWT Token
            var token = JwtHelper.GenerateJwtToken(user, _configuration);

            return Ok(new { token });
        }

        // POST: api/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var hashResult = ArgonHelper.VerifyHashedPassword(user.Username.ToLower(), user.PasswordHash, request.Password, _configuration);
            if (hashResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }
            else if (hashResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = ArgonHelper.HashPassword(user.Username.ToLower(), request.Password, _configuration);
                await _context.SaveChangesAsync();
            }

            // Generate JWT Token
            var token = JwtHelper.GenerateJwtToken(user, _configuration);

            return Ok(new { token, masterSalt = user.MasterSalt });
        }
    }

    // DTOs for requests
    public class RegisterRequest
    {
        [NotNull]
        public string? Username { get; set; }
        [NotNull]
        [PasswordPropertyText]
        public string? Password { get; set; }
        [NotNull]
        [Base64String]
        public string? MasterSalt { get; set; }
    }

    public class LoginRequest
    {
        [NotNull]
        public string? Username { get; set; }
        [NotNull]
        [PasswordPropertyText]
        public string? Password { get; set; }
    }
}
