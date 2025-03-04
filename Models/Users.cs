﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital_Management_System.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }  // Primary Key

        
        [MaxLength(100)]
        public required string Username { get; set; }  // User's Full Name

       
        [EmailAddress]
        public required string Email { get; set; }  // Unique Email for Login

       
        public required string PasswordHash { get; set; }  // Encrypted Password

        // Hash password before setting it
        public void SetPassword(string password)
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Verify password during login
        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }


        public required string Role { get; set; }  // Can be "Admin", "Doctor", "Patient"

        // Navigation Properties
        //public ICollection<Patient>? Patients { get; set; }  // Relationship with Patients
    }
}
