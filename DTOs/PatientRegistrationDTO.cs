using System.ComponentModel.DataAnnotations;

namespace Hospital_Managemant_System.DTOs
{
    public class PatientRegistrationDTO
    {
        [Required, MaxLength(50)]
        public required string FirstName { get; set; }

        [Required, MaxLength(50)]
        public required string LastName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required, MaxLength(10)]
        public required string Gender { get; set; }

        [Required, MaxLength(15)]
        public required string Phone { get; set; }

        [Required, MaxLength(100)]
        [EmailAddress]
        public required string Email { get; set; }

        [Required, MaxLength(255)]
        public required string Address { get; set; }

        [Required, MinLength(6)]
        public required string Password { get; set; }
    }
}
