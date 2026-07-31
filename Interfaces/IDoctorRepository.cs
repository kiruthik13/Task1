using HospitalManagement.Web.Models;

namespace HospitalManagement.Web.Interfaces
{
    public interface IDoctorRepository : IGenericRepository<Doctor>
    {
        Task<IEnumerable<Doctor>> GetAllWithUserAsync();
        Task<Doctor?> GetWithUserAsync(int id);
        Task<IEnumerable<Doctor>> SearchAsync(string searchTerm);
        Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync();
        Task<IEnumerable<Doctor>> GetBySpecializationAsync(string specialization);
    }
}
