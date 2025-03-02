using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Models;
using Hospital_Managemant_System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Hospital_Managemant_System.DTOs;

namespace Hospital_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly HospitalDbContext _context;

        public DoctorController(HospitalDbContext context)
        {
            _context = context;
        }

        // ✅ Get all doctors
        [HttpGet("All Doctors")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctors()
        {
            return await _context.Doctors.ToListAsync();
        }

        // ✅ Get doctor by ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Doctor>> GetDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound("Doctor not found");
            }
            return doctor;
        }

        // ✅ REGISTER DOCTOR API
        [HttpPost("register")]
        public IActionResult RegisterDoctor([FromBody] DoctorRegistrationDTO doctorDto)
        {
            // 🌟 Check if email already exists
            if (_context.Users.Any(u => u.Email == doctorDto.Email))
            {
                return BadRequest("Email already exists!");
            }

            // 🌟 Allow duplicate usernames but generate unique username if needed
            string baseUsername = doctorDto.FirstName.ToLower() + "_" + doctorDto.LastName.ToLower();
            string finalUsername = baseUsername;
            int counter = 1;

            while (_context.Users.Any(u => u.Username == finalUsername))
            {
                finalUsername = $"{baseUsername}{counter}"; // Append a number
                counter++;
            }

            // 🌟 Create User Entry
            var newUser = new User
            {
                Username = finalUsername,  // Use unique username
                Email = doctorDto.Email,
                Role = "Doctor",
                PasswordHash = ""
            };
            newUser.SetPassword(doctorDto.Password); // Hash password

            _context.Users.Add(newUser);
            _context.SaveChanges(); // Save User first to generate UserId

            // 🌟 Create Doctor Entry
            var newDoctor = new Doctor
            {
                FirstName = doctorDto.FirstName,
                LastName = doctorDto.LastName,
                Specialization = doctorDto.Specialization,
                Phone = doctorDto.Phone,
                Email = doctorDto.Email,
                Address = doctorDto.Address,
                UserID = newUser.UserID // Link Doctor to User
            };

            _context.Doctors.Add(newDoctor);
            _context.SaveChanges();

            return Ok(new { message = "Doctor registered successfully!", username = finalUsername });
        }

        // ✅ Update a doctor
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchDoctor(int id, [FromBody] JsonPatchDocument<Doctor> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest("Invalid patch data");
             }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
               return NotFound("Doctor not found");
            }

            patchDoc.ApplyTo(doctor);

            await _context.SaveChangesAsync();
            return Ok("Doctor updated successfully");
    }


    // ✅ Delete a doctor
    [HttpDelete("Delete/{id}")]
        
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound("Doctor not found");
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
