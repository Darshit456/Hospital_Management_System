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
        private readonly HttpClient _httpClient;

        public AppointmentController(HospitalDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
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

            // ✅ NEW: Send notification to doctor about new appointment
            await SendAppointmentNotification(appointment.AppointmentID);

            return Ok(new { Token = appointment.AppointmentID, Message = "Appointment booked successfully." });
        }

        // ✅ Update Appointment Status (Only Doctors & Admins)
        [HttpPut("{appointmentId}/UpdateStatus")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId, [FromBody] string newStatus)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userRoleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");

                if (userIdClaim == null || userRoleClaim == null)
                    return Unauthorized(new { Message = "Invalid token. Missing user information." });

                int userId = int.Parse(userIdClaim.Value);
                string userRole = userRoleClaim.Value;

                int? doctorId = null;
                if (userRole == "Doctor")
                {
                    var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserID == userId);
                    if (doctor != null)
                        doctorId = doctor.DoctorID;
                }

                Console.WriteLine($"User ID: {userId}, Role: {userRole}, Mapped DoctorID: {doctorId}");

                if (!AppointmentStatus.AllStatuses.Contains(newStatus))
                    return BadRequest(new { Message = "Invalid appointment status." });

                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null)
                    return NotFound(new { Message = "Appointment not found." });

                Console.WriteLine($"Appointment DoctorID: {appointment.DoctorID}");

                if (userRole == "Doctor" && (doctorId == null || appointment.DoctorID != doctorId))
                    return Forbid();

                var oldStatus = appointment.Status;
                appointment.Status = newStatus;
                await _context.SaveChangesAsync();

                // ✅ NEW: Send status change notification to patient
                await SendStatusChangeNotification(appointmentId, newStatus, oldStatus);

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

            if (userRole == "Patient")
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userId);
                if (patient == null || appointment.PatientID != patient.PatientID)
                    return Forbid();
            }

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

            // 🔹 FIXED: Fetch correct PatientID for Patients
            if (userRole == "Patient")
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserID == userId);
                if (patient == null) return Forbid();
                patientId = patient.PatientID; // Assign correct PatientID
            }
            else if (userRole == "Doctor")
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserID == userId);
                if (doctor == null) return Forbid();
                doctorId = doctor.DoctorID;
            }

            var query = _context.Appointments.AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .AsQueryable();

            if (doctorId.HasValue) query = query.Where(a => a.DoctorID == doctorId);
            if (patientId.HasValue) query = query.Where(a => a.PatientID == patientId);
            if (date.HasValue) query = query.Where(a => a.AppointmentDateTime.Date == date.Value.Date);
            if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.Status == status);

            var appointments = await query.Select(a => new
            {
                token = a.AppointmentID.ToString(), // Changed to lowercase to match your frontend
                patientID = a.PatientID,
                patientName = a.Patient.FirstName + " " + a.Patient.LastName,
                patientPhone = a.Patient.Phone,
                patientEmail = a.Patient.Email,
                doctorID = a.DoctorID,
                dateTime = a.AppointmentDateTime,
                reason = a.Reason,
                status = a.Status
            }).ToListAsync();

            return Ok(appointments);
        }

        // ✅ NEW: Get appointments for a specific doctor (used by frontend)
        [HttpGet("doctor/{doctorId}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> GetDoctorAppointments(int doctorId)
        {
            try
            {
                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Where(a => a.DoctorID == doctorId)
                    .Select(a => new
                    {
                        token = a.AppointmentID.ToString(),
                        patientID = a.PatientID,
                        patientName = a.Patient.FirstName + " " + a.Patient.LastName,
                        patientPhone = a.Patient.Phone,
                        patientEmail = a.Patient.Email,
                        doctorID = a.DoctorID,
                        dateTime = a.AppointmentDateTime,
                        reason = a.Reason,
                        status = a.Status
                    })
                    .ToListAsync();

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving appointments", Error = ex.Message });
            }
        }

        // ✅ NEW: Helper method to send appointment notification
        private async Task SendAppointmentNotification(int appointmentId)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "https://localhost:7195/api/Notification/send-appointment-notification",
                    appointmentId);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to send appointment notification: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending appointment notification: {ex.Message}");
            }
        }

        // ✅ NEW: Helper method to send status change notification
        private async Task SendStatusChangeNotification(int appointmentId, string newStatus, string oldStatus)
        {
            try
            {
                var payload = new { appointmentId, newStatus, oldStatus };
                var response = await _httpClient.PostAsJsonAsync(
                    "https://localhost:7195/api/Notification/send-status-change",
                    payload);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to send status change notification: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending status change notification: {ex.Message}");
            }
        }
    }
}