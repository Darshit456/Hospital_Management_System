using Hospital_Managemant_System.Data;
using Hospital_Managemant_System.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Hospital_Managemant_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly HospitalDbContext _context;

        public AppointmentController(HospitalDbContext context)
        {
            _context = context;
        }

        // ✅ Create Appointment (Patients & Admins)
        [HttpPost("Create")]
        [Authorize(Roles = "Patient,Admin")]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentCreateDTO dto)
        {
            if (dto.AppointmentDateTime < DateTime.UtcNow)
                return BadRequest(new { Message = "Cannot book an appointment in the past." });

            int existingCount = await _context.Appointments
                .Where(a => a.DoctorID == dto.DoctorID && a.AppointmentDateTime == dto.AppointmentDateTime)
                .CountAsync();

            if (existingCount >= 2)
                return BadRequest(new { Message = "This time slot is fully booked." });

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
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId, [FromBody] string newStatus)
        {
            try
            {
                // ✅ Extract user details from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userRoleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");

                if (userIdClaim == null || userRoleClaim == null)
                    return Unauthorized(new { Message = "Invalid token. Missing user information." });

                int userId = int.Parse(userIdClaim.Value);
                string userRole = userRoleClaim.Value;

                // ✅ Fetch DoctorID using UserID (For Doctors)
                int? doctorId = null;
                if (userRole == "Doctor")
                {
                    var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserID == userId);
                    if (doctor != null)
                        doctorId = doctor.DoctorID; // Convert UserID to DoctorID
                }

                Console.WriteLine($"User ID: {userId}, Role: {userRole}, Mapped DoctorID: {doctorId}");

                // ✅ Validate Status
                if (!AppointmentStatus.AllStatuses.Contains(newStatus))
                    return BadRequest(new { Message = "Invalid appointment status." });

                // ✅ Fetch Appointment
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null)
                    return NotFound(new { Message = "Appointment not found." });

                Console.WriteLine($"Appointment DoctorID: {appointment.DoctorID}");

                // ✅ Allow only respective doctors or admins
                if (userRole == "Doctor" && (doctorId == null || appointment.DoctorID != doctorId))
                    return Forbid();

                // ✅ Update and Save
                appointment.Status = newStatus;
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Appointment status updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }




        // ✅ Delete Appointment (Only Admins & Patients)
        [HttpDelete("{appointmentId}/Delete")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> DeleteAppointment(int appointmentId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst("role")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;

            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return NotFound(new { Message = "Appointment not found." });

            if (userRole == "Patient" && appointment.PatientID != userId)
                return Forbid();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Appointment deleted successfully." });
        }
        // ✅ Get Appointments (Role-based Access)
        [HttpGet]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        public async Task<IActionResult> GetAppointments(int? doctorId = null, int? patientId = null, DateTime? date = null, string status = null)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;

            // Role-based filtering
            if (userRole == "Patient")
            {
                patientId = userId; // Patient can only see their own data
            }
            else if (userRole == "Doctor")
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserID == userId);
                if (doctor == null) return Forbid();
                doctorId = doctor.DoctorID; // Doctor can only see their own appointments
            }

            var query = _context.Appointments.AsNoTracking().Include(a => a.Patient).Include(a => a.Doctor).AsQueryable();

            if (doctorId.HasValue) query = query.Where(a => a.DoctorID == doctorId);
            if (patientId.HasValue) query = query.Where(a => a.PatientID == patientId);
            if (date.HasValue) query = query.Where(a => a.AppointmentDateTime.Date == date.Value.Date);
            if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.Status == status);

            var appointments = await query.Select(a => new
            {
                Token = a.AppointmentID,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                DateTime = a.AppointmentDateTime,
                Status = a.Status
            }).ToListAsync();

            return Ok(appointments);
        }

    }
}
