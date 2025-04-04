﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hospital_Management_System.Models;

public class Doctor
{
    [Key]
    public int DoctorID { get; set; }  // Primary Key  

    [Required]
    public int UserID { get; set; }  // Foreign Key from User Table  

    [Required, MaxLength(50)]
    public required string FirstName { get; set; }

    [Required, MaxLength(50)]
    public required string LastName { get; set; }

    [Required, MaxLength(100)]
    public required string Specialization { get; set; }

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
