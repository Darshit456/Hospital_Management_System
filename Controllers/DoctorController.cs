using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hospital_Management_System.Models;
using Hospital_Managemant_System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;

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

        // ✅ Create a new doctor
        [HttpPost("Register")]
       
        public async Task<ActionResult<Doctor>> CreateDoctor(Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.DoctorID }, doctor);
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
