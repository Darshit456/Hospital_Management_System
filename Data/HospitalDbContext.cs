using Hospital_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Managemant_System.Data
{
    public class HospitalDbContext : DbContext
    {
        public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Configure Notification entity to match your existing table
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasDefaultValueSql("NEWID()"); // Match your existing default

                entity.Property(e => e.UserID)
                    .IsRequired();

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(500); // Match your existing nvarchar(500)

                entity.Property(e => e.IsRead)
                    .HasDefaultValue(false); // Match your existing default

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()") // Match your existing default
                    .HasColumnType("datetime2");

                // Foreign key relationship
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes for better performance
                entity.HasIndex(e => e.UserID);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsRead);
            });
        }
    }
}