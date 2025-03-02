namespace Hospital_Managemant_System.DTOs
{
    public class DoctorRegistrationDTO
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Specialization { get; set; }
        public required string Phone { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Address { get; set; }
    }
}
