using HospitalManagement.Web.DTOs;
using HospitalManagement.Web.Models;

namespace HospitalManagement.Web.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<Doctor>> GetAllDoctorsAsync();
        Task<Doctor?> GetDoctorByIdAsync(int id);
        Task<(bool Success, string Message)> CreateDoctorAsync(DoctorDTO dto);
        Task<(bool Success, string Message)> UpdateDoctorAsync(int id, DoctorDTO dto);
        Task<(bool Success, string Message)> DeleteDoctorAsync(int id);
        Task<IEnumerable<Doctor>> SearchDoctorsAsync(string searchTerm);
        Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync();
        Task<int> GetTotalCountAsync();
    }
}
