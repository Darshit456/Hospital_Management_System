using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Hospital_Managemant_System.Data;
using Microsoft.AspNetCore.JsonPatch;

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

        [HttpPost("create")]
        [Authorize]  // Ensure only authenticated users can create patients
        public async Task<IActionResult> CreatePatient([FromBody] Patient patient)
        {
            if (patient == null)
            {
                return BadRequest("Invalid patient data.");
            }

            // Ensure the UserID exists in the Users table
            var userExists = await _context.Users.AnyAsync(u => u.UserID == patient.UserID);
            if (!userExists)
            {
                return BadRequest("Invalid UserID. The user does not exist.");
            }

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPatientById), new { id = patient.PatientID }, patient);
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

        [HttpGet]
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



    }
}
