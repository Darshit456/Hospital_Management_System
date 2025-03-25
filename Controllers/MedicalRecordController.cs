using Microsoft.AspNetCore.Mvc;
using Hospital_Management_System.Models;
using Hospital_Management_System.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hospital_Managemant_System.Data;

namespace Hospital_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly HospitalDbContext _context;

        public MedicalRecordsController(HospitalDbContext context)
        {
            _context = context;
        }

        // ✅ Create Medical Record (Only Doctors)
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public IActionResult CreateMedicalRecord([FromBody] MedicalRecordDTO recordDto)
        {
            if (recordDto == null)
                return BadRequest("Invalid data.");

            var medicalRecord = new MedicalRecord
            {
                PatientID = recordDto.PatientID,
                DoctorID = recordDto.DoctorID,
                AppointmentID = recordDto.AppointmentID,
                Diagnosis = recordDto.Diagnosis,
                Prescription = recordDto.Prescription,
                Notes = recordDto.Notes,
                RecordDate = recordDto.RecordDate < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue ? DateTime.Now : recordDto.RecordDate
            };

            _context.MedicalRecords.Add(medicalRecord);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetMedicalRecord), new { id = medicalRecord.RecordID }, medicalRecord);
        }

        // ✅ Get Medical Record by ID (Role-Based Access)
        [HttpGet("{id}")]
        [Authorize(Roles = "Doctor,Admin,Patient")]
        public async Task<IActionResult> GetMedicalRecord(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;

            int? patientId = null;
            int? doctorId = null;

            if (userRole == "Patient")
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userId);
                if (patient == null) return Forbid();
                patientId = patient.PatientID;
            }
            else if (userRole == "Doctor")
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserID == userId);
                if (doctor == null) return Forbid();
                doctorId = doctor.DoctorID;
            }

            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null)
                return NotFound("Medical record not found.");

            if (userRole == "Patient" && record.PatientID != patientId)
                return Forbid("Access denied.");

            return Ok(record);
        }

        // ✅ Get All Medical Records for a Patient
        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Doctor,Admin,Patient")]
        public async Task<IActionResult> GetRecordsForPatient(int patientId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;

            if (userRole == "Patient")
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userId);
                if (patient == null || patient.PatientID != patientId) return Forbid();
            }

            var records = await _context.MedicalRecords.Where(r => r.PatientID == patientId).ToListAsync();
            return Ok(records);
        }

        // ✅ Update Medical Record (Only Doctors)
        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor")]
        public IActionResult UpdateMedicalRecord(int id, [FromBody] MedicalRecordDTO recordDto)
        {
            var record = _context.MedicalRecords.FirstOrDefault(r => r.RecordID == id);
            if (record == null)
                return NotFound("Medical record not found.");

            record.Diagnosis = recordDto.Diagnosis;
            record.Prescription = recordDto.Prescription;
            record.Notes = recordDto.Notes;
            record.RecordDate = recordDto.RecordDate;

            _context.SaveChanges();
            return Ok(record);
        }

        // ✅ Delete Medical Record (Only Admins)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteMedicalRecord(int id)
        {
            var record = _context.MedicalRecords.FirstOrDefault(r => r.RecordID == id);
            if (record == null)
                return NotFound("Medical record not found.");

            _context.MedicalRecords.Remove(record);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
