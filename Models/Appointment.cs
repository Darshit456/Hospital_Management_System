using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public class Appointment
{
   
    [Key]
    public int AppointmentID { get; set; }  // Primary Key

    [Required]
    public int PatientID { get; set; }  // Foreign Key for Patient Table

    [Required]
    public int DoctorID { get; set; }  // Foreign Key for Doctor Table

    [Required]
    public DateTime AppointmentDateTime { get; set; }

    [MaxLength(255)]
    public string? Reason { get; set; }
    
    [Required]
    [Column(TypeName = "nvarchar(50)")]
    public string Status { get; set; } = AppointmentStatus.Pending; // Default value




    // Foreign Key Relationships
    [ForeignKey("PatientID")]
    public virtual Patient? Patient { get; set; }

    [ForeignKey("DoctorID")]
    public virtual Doctor? Doctor { get; set; }
}
public static class AppointmentStatus
{
    public const string Pending = "Pending";
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
    public const string Completed = "Completed";

    public static readonly string[] AllStatuses = { Pending, Accepted, Rejected, Completed };
}
