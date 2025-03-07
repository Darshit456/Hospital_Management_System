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

            var appointment = new Appointment
            {
                PatientID = dto.PatientID,
                DoctorID = dto.DoctorID,
                AppointmentDateTime = dto.AppointmentDateTime,
                Reason = dto.Reason,
                Status = AppointmentStatus.Pending
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { Token = appointment.AppointmentID, Message = "Appointment booked successfully." });
        }

        // ✅ Update Appointment Status (Only Doctors & Admins)
        [HttpPut("{appointmentId}/UpdateStatus")]
        public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId, [FromBody] string newStatus)
        {
            if (!AppointmentStatus.AllStatuses.Contains(newStatus))
                return BadRequest("Invalid appointment status.");

            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return NotFound("Appointment not found.");

            appointment.Status = newStatus;
            await _context.SaveChangesAsync();
            return Ok("Appointment status updated successfully.");
        }

        // ✅ Delete Appointment (Only Admins & Patients)
        [HttpDelete("{appointmentId}/Delete")]
        public async Task<IActionResult> DeleteAppointment(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return NotFound("Appointment not found.");

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return Ok("Appointment deleted successfully.");
        }

        // ✅ Get Doctor’s Appointments
        [HttpGet("Doctor/{doctorId}")]
        public async Task<IActionResult> GetDoctorAppointments(int doctorId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.DoctorID == doctorId)
                .Select(a => new
                {
                    Token = a.AppointmentID,
                    PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                    DateTime = a.AppointmentDateTime,
                    Status = a.Status
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // ✅ Get Patient’s Appointments (Token-Based Retrieval)
        [HttpGet("Patient/{patientId}")]
        public async Task<IActionResult> GetPatientAppointments(int patientId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.PatientID == patientId)
                .Select(a => new
                {
                    Token = a.AppointmentID,
                    DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                    DateTime = a.AppointmentDateTime,
                    Status = a.Status
                })
                .ToListAsync();

            return Ok(appointments);
        }
    }
}
