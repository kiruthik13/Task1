using HospitalManagement.Web.DTOs;
using HospitalManagement.Web.Interfaces;
using HospitalManagement.Web.Models;

namespace HospitalManagement.Web.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepo;
        private readonly ILogger<DoctorService> _logger;

        public DoctorService(IDoctorRepository doctorRepo, ILogger<DoctorService> logger)
        {
            _doctorRepo = doctorRepo;
            _logger     = logger;
        }

        public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync()
            => await _doctorRepo.GetAllWithUserAsync();

        public async Task<Doctor?> GetDoctorByIdAsync(int id)
            => await _doctorRepo.GetWithUserAsync(id);

        public async Task<(bool Success, string Message)> CreateDoctorAsync(DoctorDTO dto)
        {
            try
            {
                var doctor = new Doctor
                {
                    Name           = dto.Name.Trim(),
                    Email          = dto.Email.Trim().ToLower(),
                    Specialization = dto.Specialization.Trim(),
                    Phone          = dto.Phone?.Trim(),
                    Qualification  = dto.Qualification?.Trim(),
                    Biography      = dto.Biography?.Trim(),
                    IsAvailable    = dto.IsAvailable,
                    CreatedDate    = DateTime.UtcNow,
                    UpdatedDate    = DateTime.UtcNow
                };

                await _doctorRepo.AddAsync(doctor);
                await _doctorRepo.SaveChangesAsync();
                _logger.LogInformation("Doctor created: {Name}", dto.Name);
                return (true, "Doctor created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating doctor {Name}.", dto.Name);
                return (false, "Failed to create doctor.");
            }
        }

        public async Task<(bool Success, string Message)> UpdateDoctorAsync(int id, DoctorDTO dto)
        {
            var doctor = await _doctorRepo.GetByIdAsync(id);
            if (doctor == null) return (false, "Doctor not found.");

            doctor.Name           = dto.Name.Trim();
            doctor.Email          = dto.Email.Trim().ToLower();
            doctor.Specialization = dto.Specialization.Trim();
            doctor.Phone          = dto.Phone?.Trim();
            doctor.Qualification  = dto.Qualification?.Trim();
            doctor.Biography      = dto.Biography?.Trim();
            doctor.IsAvailable    = dto.IsAvailable;
            doctor.UpdatedDate    = DateTime.UtcNow;

            _doctorRepo.Update(doctor);
            await _doctorRepo.SaveChangesAsync();
            _logger.LogInformation("Doctor {Id} updated.", id);
            return (true, "Doctor updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteDoctorAsync(int id)
        {
            var doctor = await _doctorRepo.GetByIdAsync(id);
            if (doctor == null) return (false, "Doctor not found.");

            _doctorRepo.Remove(doctor);
            await _doctorRepo.SaveChangesAsync();
            _logger.LogInformation("Doctor {Id} deleted.", id);
            return (true, "Doctor deleted successfully.");
        }

        public async Task<IEnumerable<Doctor>> SearchDoctorsAsync(string searchTerm)
            => await _doctorRepo.SearchAsync(searchTerm);

        public async Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync()
            => await _doctorRepo.GetAvailableDoctorsAsync();

        public async Task<int> GetTotalCountAsync()
            => await _doctorRepo.CountAsync();
    }
}
