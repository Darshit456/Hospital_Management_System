using Microsoft.AspNetCore.Mvc;
using Hospital_Management_System.Models;
using Hospital_Management_System.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
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

        // POST: api/MedicalRecords (Create Medical Record)
        [HttpPost]
        [Authorize(Roles = "Doctor")] // Only doctors can create records
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
                RecordDate = recordDto.RecordDate
            };

            _context.MedicalRecords.Add(medicalRecord);
            if (medicalRecord.RecordDate < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue)
            {
                medicalRecord.RecordDate = DateTime.Now; // Or some default valid date
            }

            _context.SaveChanges();

            return CreatedAtAction(nameof(GetMedicalRecord), new { id = medicalRecord.RecordID }, medicalRecord);
        }

        // GET: api/MedicalRecords/{id} (Get Medical Record by ID)
        [HttpGet("{id}")]
        [Authorize(Roles = "Doctor,Admin,Patient")] // Doctors/Admins can access any record, Patients only their own
        public IActionResult GetMedicalRecord(int id)
        {
            var record = _context.MedicalRecords.FirstOrDefault(r => r.RecordID == id);
            if (record == null)
                return NotFound("Medical record not found.");

            // Get logged-in user info
            var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);

            if (userRole == "Patient" && record.PatientID != userId)
                return Forbid("Access denied.");

            var recordDto = new MedicalRecordDTO
            {
                RecordID = record.RecordID,
                PatientID = record.PatientID,
                DoctorID = record.DoctorID,
                AppointmentID = record.AppointmentID,
                Diagnosis = record.Diagnosis,
                Prescription = record.Prescription,
                Notes = record.Notes,
                RecordDate = record.RecordDate
            };

            return Ok(recordDto);
        }

        // GET: api/MedicalRecords/patient/{patientId} (Get All Records for a Patient)
        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Doctor,Admin,Patient")] // Doctor/Admin can view any patient’s records, Patient can view their own
        public IActionResult GetRecordsForPatient(int patientId)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value);

            if (userRole == "Patient" && userId != patientId)
                return Forbid("Access denied.");

            var records = _context.MedicalRecords.Where(r => r.PatientID == patientId).ToList();
            return Ok(records);
        }

        // PUT: api/MedicalRecords/{id} (Update Medical Record)
        [HttpPut("{id}")]
        [Authorize(Roles = "Doctor")] // Only doctors can update records
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

        // DELETE: api/MedicalRecords/{id} (Delete Medical Record)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only admins can delete records
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
