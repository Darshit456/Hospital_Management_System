using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Appointment
{
    [Key]
    public int AppointmentID { get; set; }  // Primary Key

    [Required]
    public int PatientID { get; set; }  // Foreign Key for Patient Table

    [Required]
    public int DoctorID { get; set; }  // Foreign Key for Doctor Table

    [Required]
    public DateTime AppointmentDate { get; set; }

    [MaxLength(255)]
    public string? Reason { get; set; }

    [Required, MaxLength(50)]
    public required string Status { get; set; }  // Added Status Column  

    // Foreign Key Relationships
    [ForeignKey("PatientID")]
    public Patient? Patient { get; set; }

    [ForeignKey("DoctorID")]
    public Doctor? Doctor { get; set; }
}
