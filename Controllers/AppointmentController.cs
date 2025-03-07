using Hospital_Managemant_System.Data;
using Hospital_Managemant_System.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace Hospital_Managemant_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly HospitalDbContext _context;
        private const int DefaultMaxAppointmentsPerDay = 30; // Default limit

        public AppointmentController(HospitalDbContext context)
        {
            _context = context;
        }

        // ✅ Create Appointment (Token-Based System)
        [HttpPost("Create")]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentCreateDTO dto)
        {
            if (!await _context.Patients.AnyAsync(p => p.PatientID == dto.PatientID))
                return BadRequest("Invalid Patient ID.");

            if (!await _context.Doctors.AnyAsync(d => d.DoctorID == dto.DoctorID))
                return BadRequest("Invalid Doctor ID.");

            // Get doctor's daily appointment limit (default 30)
            int doctorDailyLimit = await _context.Doctors
                .Where(d => d.DoctorID == dto.DoctorID)
                .Select(d => d.DailyAppointmentLimit ?? DefaultMaxAppointmentsPerDay)
                .FirstOrDefaultAsync();

            // Count existing appointments for the doctor on the selected date
            int bookedAppointments = await _context.Appointments
                .Where(a => a.DoctorID == dto.DoctorID && a.AppointmentDate == dto.AppointmentDate)
                .CountAsync();

            if (bookedAppointments >= doctorDailyLimit)
                return BadRequest("Doctor has reached the maximum appointments for this day.");

            var appointment = new Appointment
            {
                PatientID = dto.PatientID,
                DoctorID = dto.DoctorID,
                AppointmentDate = dto.AppointmentDate,
                Status = AppointmentStatus.Pending
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { Token = appointment.AppointmentID, Message = "Appointment booked successfully." });
        }

        // ✅ Allow Admins/Doctors to Update Daily Appointment Limit
        [HttpPut("Doctor/{doctorId}/SetLimit")]
        public async Task<IActionResult> SetDoctorDailyLimit(int doctorId, [FromBody] int newLimit)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null)
                return NotFound("Doctor not found.");

            if (newLimit < 1)
                return BadRequest("Daily limit must be at least 1.");

            doctor.DailyAppointmentLimit = newLimit;
            await _context.SaveChangesAsync();
            return Ok("Doctor's daily appointment limit updated successfully.");
        }

        // ✅ Get Patient's Appointments (Token-Based Retrieval)
        [HttpGet("Patient/{patientId}")]
        public async Task<IActionResult> GetPatientAppointments(int patientId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.PatientID == patientId)
                .Select(a => new { Token = a.AppointmentID, Date = a.AppointmentDate, Status = a.Status })
                .ToListAsync();

            return Ok(appointments);
        }
    }
}
