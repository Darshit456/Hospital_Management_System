using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Management_System.Models
{
    public class MedicalRecord
    {
        [Key]
        public int RecordID { get; set; } // Primary Key

        [Required]
        [ForeignKey("Patient")]
        public int PatientID { get; set; } // Links to Patient

        [Required]
        [ForeignKey("Doctor")]
        public int DoctorID { get; set; } // Links to Doctor

        [Required]
        [ForeignKey("Appointment")]
        public int AppointmentID { get; set; } // Links to Appointment

        [Required]
        [MaxLength(255)]
        public string Diagnosis { get; set; } // Medical diagnosis

        [Required]
        [MaxLength(500)]
        public string Prescription { get; set; } // Medications/treatment

        [MaxLength(1000)]
        public string? Notes { get; set; } // Additional doctor comments

        [Required]
        public DateTime RecordDate { get; set; }        // Date of the report

        // Navigation Properties
        public virtual Patient Patient { get; set; }
        public virtual Doctor Doctor { get; set; }
        public virtual Appointment Appointment { get; set; }
    }
}
