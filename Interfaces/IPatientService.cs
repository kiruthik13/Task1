using HospitalManagement.Web.DTOs;
using HospitalManagement.Web.Models;

namespace HospitalManagement.Web.Interfaces
{
    public interface IPatientService
    {
        Task<IEnumerable<Patient>> GetAllPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(int id);
        Task<(bool Success, string Message)> CreatePatientAsync(PatientDTO dto);
        Task<(bool Success, string Message)> UpdatePatientAsync(int id, PatientDTO dto);
        Task<(bool Success, string Message)> DeletePatientAsync(int id);
        Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm);
        Task<int> GetTotalCountAsync();
    }
}
