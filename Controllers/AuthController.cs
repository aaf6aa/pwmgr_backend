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

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <remarks>
        /// This endpoint creates a new user account with a unique username and hashed password.
        /// </remarks>
        /// <param name="request">The registration request containing the username, password, and master salt.</param>
        /// <returns>
        /// - 200 OK: Returns a JWT token for the newly created user.
        /// - 400 Bad Request: If the username is already taken.
        /// </returns>
        /// <example>
        /// Request:
        /// POST api/register
        /// {
        ///     "username": "exampleUser",
        ///     "password": "examplePassword",
        ///     "masterSalt": "base64EncodedMasterSalt"
        /// }
        /// Response:
        /// {
        ///     "token": "jwtToken"
        /// }
        /// </example>
        [HttpPost("register")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
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

            var token = JwtHelper.GenerateJwtToken(user, _configuration);
            return Ok(new { token });
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token.
        /// </summary>
        /// <remarks>
        /// This endpoint verifies the user's credentials and returns a JWT token if successful.
        /// </remarks>
        /// <param name="request">The login request containing the username and password.</param>
        /// <returns>
        /// - 200 OK: Returns a JWT token and the user's master salt.
        /// - 401 Unauthorized: If the password is incorrect.
        /// - 404 Not Found: If the username is not found.
        /// </returns>
        /// <example>
        /// Request:
        /// POST api/login
        /// {
        ///     "username": "exampleUser",
        ///     "password": "examplePassword"
        /// }
        /// Response:
        /// {
        ///     "token": "jwtToken",
        ///     "masterSalt": "base64EncodedMasterSalt"
        /// }
        /// </example>
        [HttpPost("login")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
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