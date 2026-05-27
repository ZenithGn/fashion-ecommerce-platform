using FashionEcommerce.API.Models.Auth;
using FashionEcommerce.API.Services.Email;
using FashionEcommerce.Core.Entities;
using FashionEcommerce.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FashionEcommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FashionEcommerceDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthController(
            FashionEcommerceDbContext context,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IEmailSender emailSender)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.FirstName) ||
                    string.IsNullOrWhiteSpace(request.LastName) ||
                    string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest("First name, last name, email and password are required.");
                }

                if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
                {
                    return BadRequest("Password and confirm password do not match.");
                }

                var normalizedEmail = request.Email.Trim().ToLowerInvariant();
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && !u.IsDeleted);
                if (existingUser != null)
                {
                    return Conflict("Email is already registered.");
                }

                var user = new User
                {
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = normalizedEmail,
                    PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
                    RoleId = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var cart = new Cart
                {
                    UserId = user.Id,
                    ItemCount = 0,
                    TotalPrice = 0m,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                var createdUser = await _context.Users
                    .Include(u => u.Role)
                    .FirstAsync(u => u.Id == user.Id);

                var token = GenerateJwtToken(createdUser, out var expiresAtUtc);

                return Ok(new AuthResponse
                {
                    UserId = createdUser.Id,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    Email = createdUser.Email,
                    Role = createdUser.Role.RoleName,
                    Token = token,
                    ExpiresAtUtc = expiresAtUtc
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest("Email and password are required.");
                }

                var normalizedEmail = request.Email.Trim().ToLowerInvariant();
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && !u.IsDeleted);

                if (user == null || !user.IsActive)
                {
                    return Unauthorized("Invalid email or password.");
                }

                PasswordVerificationResult verificationResult;

                try
                {
                    verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
                }
                catch (FormatException)
                {
                    _logger.LogWarning("Stored password hash for user {UserId} is invalid", user.Id);
                    return Unauthorized("Invalid email or password.");
                }

                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    return Unauthorized("Invalid email or password.");
                }

                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user, out var expiresAtUtc);

                return Ok(new AuthResponse
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role.RoleName,
                    Token = token,
                    ExpiresAtUtc = expiresAtUtc
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while logging in user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("request-password-reset")]
        public async Task<ActionResult<PasswordResetResponse>> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest("Email is required.");
                }

                var normalizedEmail = request.Email.Trim().ToLowerInvariant();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && !u.IsDeleted);
                if (user == null)
                {
                    return Ok(new PasswordResetResponse
                    {
                        Message = "If the email exists, a reset token has been generated."
                    });
                }

                var resetToken = GenerateResetToken();
                user.PasswordResetToken = resetToken;
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var subject = "Reset your password";
                var body = $"Hello {user.FirstName},\n\nYour password reset token is: {resetToken}\n\nThis token expires at {user.PasswordResetTokenExpiry:O} UTC.\n\nIf you did not request this, please ignore this email.";
                try
                {
                    await _emailSender.SendAsync(user.Email, subject, body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
                    // Do not expose SMTP errors to the caller — respond as if token was generated.
                }

                return Ok(new PasswordResetResponse
                {
                    Message = "If the email exists, a reset token has been generated.",
                    ExpiresAtUtc = user.PasswordResetTokenExpiry
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while generating password reset token");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Token) ||
                    string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return BadRequest("Email, token and new password are required.");
                }

                if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
                {
                    return BadRequest("New password and confirm password do not match.");
                }

                var normalizedEmail = request.Email.Trim().ToLowerInvariant();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && !u.IsDeleted);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                if (string.IsNullOrWhiteSpace(user.PasswordResetToken) ||
                    user.PasswordResetTokenExpiry == null ||
                    user.PasswordResetTokenExpiry < DateTime.UtcNow)
                {
                    return BadRequest("Reset token is invalid or expired.");
                }

                if (!string.Equals(user.PasswordResetToken, request.Token.Trim(), StringComparison.Ordinal))
                {
                    return BadRequest("Reset token is invalid or expired.");
                }

                user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Password reset successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while resetting password");
                return StatusCode(500, "Internal server error");
            }
        }

        private string GenerateJwtToken(User user, out DateTime expiresAtUtc)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing");
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JwtSettings:Issuer is missing");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JwtSettings:Audience is missing");
            var expirationMinutes = int.TryParse(jwtSettings["ExpirationMinutes"], out var minutes) ? minutes : 60;

            expiresAtUtc = DateTime.UtcNow.AddMinutes(expirationMinutes);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new(ClaimTypes.Role, user.Role?.RoleName ?? "Customer")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateResetToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        }
    }
}
