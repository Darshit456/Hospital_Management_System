using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class MedicalReport
{
    [Key]
    public int ReportID { get; set; }  // Primary Key

    [Required]
    public int PatientID { get; set; }  // Foreign Key for Patient Table

    [Required]
    public int DoctorID { get; set; }  // Foreign Key for Doctor Table

    [Required]
    public int AppointmentID { get; set; }  // Foreign Key for Appointment Table

    [Required, MaxLength(255)]
    public required string Diagnosis { get; set; }

    [Required, MaxLength(500)]
    public required string Prescription { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public DateTime ReportDate { get; set; }

    // Foreign Key Relationships
    [ForeignKey("PatientID")]
    public Patient? Patient { get; set; }

    [ForeignKey("DoctorID")]
    public Doctor? Doctor { get; set; }

    [ForeignKey("AppointmentID")]
    public Appointment? Appointment { get; set; }
}
