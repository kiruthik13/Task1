using HospitalManagement.Web.DTOs;
using HospitalManagement.Web.Interfaces;
using HospitalManagement.Web.Models;

namespace HospitalManagement.Web.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepo;
        private readonly ILogger<PatientService> _logger;

        public PatientService(IPatientRepository patientRepo, ILogger<PatientService> logger)
        {
            _patientRepo = patientRepo;
            _logger      = logger;
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
            => await _patientRepo.GetAllWithUserAsync();

        public async Task<Patient?> GetPatientByIdAsync(int id)
            => await _patientRepo.GetWithAppointmentsAsync(id);

        public async Task<(bool Success, string Message)> CreatePatientAsync(PatientDTO dto)
        {
            try
            {
                var patient = new Patient
                {
                    Name          = dto.Name.Trim(),
                    Email         = dto.Email.Trim().ToLower(),
                    DateOfBirth   = dto.DateOfBirth,
                    Phone         = dto.Phone?.Trim(),
                    Address       = dto.Address?.Trim(),
                    Gender        = dto.Gender?.Trim(),
                    MedicalHistory = dto.MedicalHistory?.Trim(),
                    CreatedDate   = DateTime.UtcNow,
                    UpdatedDate   = DateTime.UtcNow
                };

                await _patientRepo.AddAsync(patient);
                await _patientRepo.SaveChangesAsync();
                _logger.LogInformation("Patient created: {Name}", dto.Name);
                return (true, "Patient created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient {Name}.", dto.Name);
                return (false, "Failed to create patient.");
            }
        }

        public async Task<(bool Success, string Message)> UpdatePatientAsync(int id, PatientDTO dto)
        {
            var patient = await _patientRepo.GetByIdAsync(id);
            if (patient == null) return (false, "Patient not found.");

            patient.Name           = dto.Name.Trim();
            patient.Email          = dto.Email.Trim().ToLower();
            patient.DateOfBirth    = dto.DateOfBirth;
            patient.Phone          = dto.Phone?.Trim();
            patient.Address        = dto.Address?.Trim();
            patient.Gender         = dto.Gender?.Trim();
            patient.MedicalHistory = dto.MedicalHistory?.Trim();
            patient.UpdatedDate    = DateTime.UtcNow;

            _patientRepo.Update(patient);
            await _patientRepo.SaveChangesAsync();
            _logger.LogInformation("Patient {Id} updated.", id);
            return (true, "Patient updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeletePatientAsync(int id)
        {
            var patient = await _patientRepo.GetByIdAsync(id);
            if (patient == null) return (false, "Patient not found.");

            _patientRepo.Remove(patient);
            await _patientRepo.SaveChangesAsync();
            _logger.LogInformation("Patient {Id} deleted.", id);
            return (true, "Patient deleted successfully.");
        }

        public async Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm)
            => await _patientRepo.SearchAsync(searchTerm);

        public async Task<int> GetTotalCountAsync()
            => await _patientRepo.CountAsync();
    }
}
