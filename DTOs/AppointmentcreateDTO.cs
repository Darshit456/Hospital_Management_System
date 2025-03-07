namespace Hospital_Managemant_System.DTOs
{
    public class AppointmentCreateDTO
    {
        public int PatientID { get; set; }
        public int DoctorID { get; set; }
        public string Reason { get; set; }
        public DateTime AppointmentDateTime { get; set; }
    }

    public class AppointmentDTO
    {
        public int AppointmentID { get; set; }
        public int PatientID { get; set; }
        public int DoctorID { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public DateTime AppointmentDateTime { get; set; }

        // Include related data
        public PatientDTO Patient { get; set; }
        public DoctorDTO Doctor { get; set; }
    }

    public class PatientDTO
    {
        public int PatientID { get; set; }
        public string Name { get; set; }
    }

    public class DoctorDTO
    {
        public int DoctorID { get; set; }
        public string Name { get; set; }
    }

}
