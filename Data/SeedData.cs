using HospitalManagement.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Web.Data
{
    /// <summary>
    /// Seeds default data (Admin user) at application startup if not already present.
    /// </summary>
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                // Apply any pending migrations
                await context.Database.MigrateAsync();

                // Seed Admin user if not exists
                if (!await context.Users.AnyAsync(u => u.Email == "admin@hospital.com"))
                {
                    var adminUser = new User
                    {
                        Name         = "System Admin",
                        Email        = "admin@hospital.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        RoleId       = 1, // Admin
                        IsActive     = true,
                        CreatedDate  = DateTime.UtcNow,
                        UpdatedDate  = DateTime.UtcNow
                    };

                    context.Users.Add(adminUser);
                    await context.SaveChangesAsync();
                    logger.LogInformation("✅ Admin user seeded: admin@hospital.com / Admin@123");
                }

                // Seed sample doctor
                if (!await context.Doctors.AnyAsync())
                {
                    var sampleDoctors = new List<Doctor>
                    {
                        new Doctor { Name = "Dr. Sarah Johnson", Email = "sarah.johnson@hospital.com", Specialization = "Cardiology",    Phone = "+1-555-0101", Qualification = "MD, FACC",   IsAvailable = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                        new Doctor { Name = "Dr. Michael Chen",  Email = "michael.chen@hospital.com",  Specialization = "Neurology",     Phone = "+1-555-0102", Qualification = "MD, PhD",    IsAvailable = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                        new Doctor { Name = "Dr. Emily Davis",   Email = "emily.davis@hospital.com",   Specialization = "Pediatrics",    Phone = "+1-555-0103", Qualification = "MD, FAAP",   IsAvailable = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                        new Doctor { Name = "Dr. James Wilson",  Email = "james.wilson@hospital.com",  Specialization = "Orthopedics",   Phone = "+1-555-0104", Qualification = "MD, FAAOS",  IsAvailable = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                        new Doctor { Name = "Dr. Lisa Anderson", Email = "lisa.anderson@hospital.com", Specialization = "Dermatology",   Phone = "+1-555-0105", Qualification = "MD, FAAD",   IsAvailable = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                    };
                    context.Doctors.AddRange(sampleDoctors);
                    await context.SaveChangesAsync();
                    logger.LogInformation("✅ Sample doctors seeded.");
                }

                // Seed sample patients
                if (!await context.Patients.AnyAsync())
                {
                    static DateTime Utc(int y, int m, int d) =>
                        DateTime.SpecifyKind(new DateTime(y, m, d), DateTimeKind.Utc);

                    var samplePatients = new List<Patient>
                    {
                        new Patient { Name = "John Smith",   Email = "john.smith@email.com",   Phone = "+1-555-1001", Gender = "Male",   DateOfBirth = Utc(1985, 3, 15), Address = "123 Main St, New York", CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                        new Patient { Name = "Mary Johnson", Email = "mary.johnson@email.com",  Phone = "+1-555-1002", Gender = "Female", DateOfBirth = Utc(1990, 7, 22), Address = "456 Oak Ave, Boston",   CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                        new Patient { Name = "Robert Brown", Email = "robert.brown@email.com",  Phone = "+1-555-1003", Gender = "Male",   DateOfBirth = Utc(1978, 11, 8), Address = "789 Pine Rd, Chicago",  CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                        new Patient { Name = "Emma Davis",   Email = "emma.davis@email.com",    Phone = "+1-555-1004", Gender = "Female", DateOfBirth = Utc(1995, 5, 30), Address = "321 Elm St, Houston",   CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                        new Patient { Name = "William Lee",  Email = "william.lee@email.com",   Phone = "+1-555-1005", Gender = "Male",   DateOfBirth = Utc(1967, 9, 12), Address = "654 Maple Dr, Phoenix", CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow },
                    };
                    context.Patients.AddRange(samplePatients);
                    await context.SaveChangesAsync();
                    logger.LogInformation("✅ Sample patients seeded.");
                }

                // Sync any registered users with RoleId == 3 (Patient) who lack a Patient record
                var patientUsers = await context.Users.Where(u => u.RoleId == 3).ToListAsync();
                foreach (var u in patientUsers)
                {
                    if (!await context.Patients.AnyAsync(p => p.Email.ToLower() == u.Email.ToLower() || p.UserId == u.Id))
                    {
                        context.Patients.Add(new Patient
                        {
                            Name = u.Name,
                            Email = u.Email,
                            UserId = u.Id,
                            CreatedDate = u.CreatedDate,
                            UpdatedDate = DateTime.UtcNow
                        });
                        logger.LogInformation("✅ Synced registered user {Email} into Patients table.", u.Email);
                    }
                }
                await context.SaveChangesAsync();

                // Sync any registered users with RoleId == 2 (Doctor) who lack a Doctor record
                var doctorUsers = await context.Users.Where(u => u.RoleId == 2).ToListAsync();
                foreach (var u in doctorUsers)
                {
                    if (!await context.Doctors.AnyAsync(d => d.Email.ToLower() == u.Email.ToLower() || d.UserId == u.Id))
                    {
                        context.Doctors.Add(new Doctor
                        {
                            Name = u.Name,
                            Email = u.Email,
                            Specialization = "General Medicine",
                            IsAvailable = true,
                            UserId = u.Id,
                            CreatedDate = u.CreatedDate,
                            UpdatedDate = DateTime.UtcNow
                        });
                        logger.LogInformation("✅ Synced registered user {Email} into Doctors table.", u.Email);
                    }
                }
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ An error occurred while seeding the database.");
                throw;
            }
        }
    }
}
