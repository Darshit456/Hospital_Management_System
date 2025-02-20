using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using Hospital_Managemant_System.Data;
using Hospital_Management_System.Models;

namespace Hospital_Management_System.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly HospitalDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(HospitalDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ✅ REGISTER API
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
                return BadRequest("User already exists!");

            // Hash Password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully!" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null || user.PasswordHash != request.Password) // Compare plain text passwords
            {
                return Unauthorized("Invalid credentials!");
            }

            // Generate JWT Token (Keep this part as it is)
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),  // Ensure UserID is used
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }


        // ✅ JWT Token Generation
        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing"));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // ✅ Login Request Model
    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
