using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using Hospital_Managemant_System.Data;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Hospital_Managemant_System.DTOs;

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

        // ✅ GET ALL ADMINS (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet("all-admins")]
        public async Task<IActionResult> GetAllAdmins()
        {
            try
            {
                var admins = await _context.Users
                    .Where(u => u.Role == "Admin")
                    .Select(u => new
                    {
                        UserID = u.UserID,
                        Username = u.Username,
                        Email = u.Email,
                        Role = u.Role
                    })
                    .ToListAsync();

                return Ok(admins);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching admins", error = ex.Message });
            }
        }

        // ✅ UPDATE ADMIN (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPut("update-admin/{id}")]
        public async Task<IActionResult> UpdateAdmin(int id, [FromBody] UpdateAdminDTO updateDto)
        {
            try
            {
                var admin = await _context.Users.FindAsync(id);

                if (admin == null)
                {
                    return NotFound("Admin not found");
                }

                if (admin.Role != "Admin")
                {
                    return BadRequest("User is not an admin");
                }

                // Update fields if provided
                if (!string.IsNullOrWhiteSpace(updateDto.Username))
                {
                    // Check if username already exists
                    var existingUser = await _context.Users
                        .Where(u => u.Username == updateDto.Username && u.UserID != id)
                        .FirstOrDefaultAsync();

                    if (existingUser != null)
                    {
                        return BadRequest("Username already exists");
                    }

                    admin.Username = updateDto.Username;
                }

                if (!string.IsNullOrWhiteSpace(updateDto.Email))
                {
                    // Check if email already exists
                    var existingEmail = await _context.Users
                        .Where(u => u.Email == updateDto.Email && u.UserID != id)
                        .FirstOrDefaultAsync();

                    if (existingEmail != null)
                    {
                        return BadRequest("Email already exists");
                    }

                    admin.Email = updateDto.Email;
                }

                if (!string.IsNullOrWhiteSpace(updateDto.Password))
                {
                    admin.SetPassword(updateDto.Password);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Admin updated successfully",
                    admin = new
                    {
                        UserID = admin.UserID,
                        Username = admin.Username,
                        Email = admin.Email,
                        Role = admin.Role
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating admin", error = ex.Message });
            }
        }

        // ✅ DELETE ADMIN (Admin only) - Enhanced version
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-admin/{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            try
            {
                // Get current user ID from token
                var currentUserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                if (currentUserId != null && int.Parse(currentUserId) == id)
                {
                    return BadRequest("You cannot delete your own admin account");
                }

                var admin = await _context.Users.FindAsync(id);

                if (admin == null)
                {
                    return NotFound("Admin not found");
                }

                if (admin.Role != "Admin")
                {
                    return BadRequest("User is not an admin");
                }

                // Check if this is the last admin
                var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
                if (adminCount <= 1)
                {
                    return BadRequest("Cannot delete the last admin in the system");
                }

                // Use raw SQL to bypass database trigger issues
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Users WHERE UserID = {0}", id);

                return Ok(new { message = "Admin deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting admin", error = ex.Message });
            }
        }

        // ✅ ADMIN REGISTRATION (Only for Existing Admins)
        [Authorize(Roles = "Admin")]
        [HttpPost("create-admin")]
        public IActionResult CreateAdmin([FromBody] AdminRegistrationDTO adminDto)
        {
            if (string.IsNullOrWhiteSpace(adminDto.Email) || string.IsNullOrWhiteSpace(adminDto.Password))
            {
                return BadRequest("Email and Password are required!");
            }

            // Check if email already exists
            if (_context.Users.Any(u => u.Email == adminDto.Email))
            {
                return BadRequest("Email already exists!");
            }

            // Check if username already exists
            if (_context.Users.Any(u => u.Username == adminDto.Username))
            {
                return BadRequest("Username already exists!");
            }

            // Create User object
            var newAdmin = new User
            {
                Username = adminDto.Username,
                Email = adminDto.Email,
                Role = "Admin",  // 🔥 Assign Role explicitly
                PasswordHash = ""
            };

            // Hash password
            newAdmin.SetPassword(adminDto.Password);

            // Save admin user
            _context.Users.Add(newAdmin);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Admin registered successfully!",
                admin = new
                {
                    UserID = newAdmin.UserID,
                    Username = newAdmin.Username,
                    Email = newAdmin.Email,
                    Role = newAdmin.Role
                }
            });
        }

        // ✅ FIXED Delete User - Handles Database Triggers
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Use raw SQL to bypass database trigger issues
                // The CASCADE DELETE will handle all related records automatically
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Users WHERE UserID = {0}", id);

                return Ok(new { message = "User and all related data deleted successfully" });
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error deleting user {id}: {ex.Message}");

                return StatusCode(500, new
                {
                    message = "Error deleting user",
                    error = ex.Message
                });
            }
        }

        // ✅ LOGIN API (Updated for Role-Based Redirection)
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null || !user.VerifyPassword(request.Password))
            {
                return Unauthorized("Invalid credentials!");
            }

            // 🌟 Fetch role-specific details & assign dashboard URL
            object userDetails = null;
            string dashboardUrl = "";

            if (user.Role == "Doctor")
            {
                userDetails = _context.Doctors.FirstOrDefault(d => d.UserID == user.UserID);
                dashboardUrl = "/doctor/dashboard";
            }
            else if (user.Role == "Patient")
            {
                userDetails = _context.Patients.FirstOrDefault(p => p.UserID == user.UserID);
                dashboardUrl = "/patient/dashboard";
            }
            else if (user.Role == "Admin")
            {
                // ✅ FIXED: Now returning admin details instead of null
                userDetails = new
                {
                    userID = user.UserID,
                    username = user.Username,
                    email = user.Email,
                    role = user.Role
                };
                dashboardUrl = "/admin/dashboard";
            }

            // 🌟 Generate JWT Token with Role
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)  // 🔥 Add Role to Token
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // 🌟 Return token + user info + dashboard URL
            return Ok(new
            {
                Token = tokenString,
                Role = user.Role,
                DashboardUrl = dashboardUrl,
                UserDetails = userDetails  // Now includes admin details
            });
        }

        // ✅ PROTECTED PROFILE ENDPOINT (INSIDE THE CLASS)
        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            return Ok(new { message = "You have accessed a protected route!" });
        }
    }

    // ✅ Login Request Model
    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    // ✅ Update Admin DTO
    public class UpdateAdminDTO
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}