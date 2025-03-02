using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Hospital_Managemant_System.Data;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Http.HttpResults;
using Hospital_Managemant_System.DTOs;

//"Username" : "Darshit Gohil",
//    "passwordHash": "Darshit123",
//    "email": "Darshitgohil123@gmail.com",
//    "Role":"Patient"

namespace Hospital_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly HospitalDbContext _context;

        public PatientController(HospitalDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
                public IActionResult RegisterPatient([FromBody] PatientRegistrationDTO patientDto)
                {
                    // 🌟 Check if email already exists
                    if (_context.Users.Any(u => u.Email == patientDto.Email))
                    {
                        return BadRequest("Email already exists!");
                    }

                    // 🌟 Generate unique username
                    string baseUsername = patientDto.FirstName.ToLower() + "_" + patientDto.LastName.ToLower();
                    string finalUsername = baseUsername;
                    int counter = 1;

                    while (_context.Users.Any(u => u.Username == finalUsername))
                    {
                        finalUsername = $"{baseUsername}{counter}"; // Append number if duplicate
                        counter++;
                    }

                    // 🌟 Create User Entry
                    var newUser = new User
                    {
                        Username = finalUsername,
                        Email = patientDto.Email,
                        Role = "Patient",
                        PasswordHash = ""
                    };
                    newUser.SetPassword(patientDto.Password); // Hash password

                    _context.Users.Add(newUser);
                    _context.SaveChanges(); // Save User first to generate UserId

                    // 🌟 Create Patient Entry
                    var newPatient = new Patient
                    {
                        FirstName = patientDto.FirstName,
                        LastName = patientDto.LastName,
                        DateOfBirth = patientDto.DateOfBirth,
                        Phone = patientDto.Phone,
                        Gender = patientDto.Gender,
                        Email = patientDto.Email,
                        Address = patientDto.Address,
                        UserID = newUser.UserID // Link Patient to User
                    };

                    _context.Patients.Add(newPatient);
                    _context.SaveChanges();

                    return Ok(new { message = "Patient registered successfully!", username = finalUsername });
                }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var patient = await _context.Patients
                                        .Include(p => p.User) // Include User details if needed
                                        .FirstOrDefaultAsync(p => p.PatientID == id);

            if (patient == null)
            {
                return NotFound("Patient not found.");
            } 

            return Ok(patient);
        }

        [HttpGet("All")]
        [Authorize]
        public async Task<IActionResult> GetAllPatients()
        {
            var patientList = await _context.Patients.Include(p => p.User).ToListAsync();
            return Ok(patientList);
        }

        [HttpPatch("update/{id}")]
        [Authorize] // Ensure only authenticated users can update patient details
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] JsonPatchDocument<Patient> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest("Invalid update request.");
            }

            var patient = await _context.Patients.Include(p => p.User).FirstOrDefaultAsync(p => p.PatientID == id);
            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            patchDoc.ApplyTo(patient, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Patient details updated successfully." });
        }

        [HttpDelete("Delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientID == id);
            if (patient == null)
            {
                return NotFound("Patient is not Found!");
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Patient has been Removed." });
        }
    }
}
