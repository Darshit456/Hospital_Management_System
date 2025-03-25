using System;

namespace Hospital_Management_System.DTOs
{
    public class MedicalRecordDTO
    {
        public int RecordID { get; set; } // Unique identifier
        public int PatientID { get; set; } // Linked Patient
        public int DoctorID { get; set; } // Linked Doctor
        public int AppointmentID { get; set; } // Linked Appointment
        public string Diagnosis { get; set; } // Medical diagnosis
        public string Prescription { get; set; } // Medications/treatment
        public string? Notes { get; set; } // Additional doctor comments
        public DateTime RecordDate { get; set; } // Date of the report
    }
}
