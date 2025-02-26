using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hospital_Management_System.Models;

public class Patient
{
    [Key]
    public int PatientID { get; set; }  // Primary Key  

    [Required]
    public int UserID { get; set; }  // Foreign Key from User Table  

    [Required, MaxLength(50)]
    [Column("First Name")]
    public required string FirstName { get; set; }

    [Required, MaxLength(50)]
    [Column("Last Name")]
    public required string LastName { get; set; }

    [Required]
    [Column("Date Of Birth")]
    public DateTime DateOfBirth { get; set; }

    [Required, MaxLength(10)]
    public required string Gender { get; set; }

    [Required, MaxLength(15)]
    public required string Phone { get; set; }

    [Required, MaxLength(100)]
    public required string Email { get; set; }

    [Required, MaxLength(255)]
    public required string Address { get; set; }

    // Foreign Key Relationship
    [ForeignKey("UserID")]
    public User? User { get; set; }
}
