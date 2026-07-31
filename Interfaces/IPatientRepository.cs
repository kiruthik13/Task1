using HospitalManagement.Web.Models;

namespace HospitalManagement.Web.Interfaces
{
    public interface IPatientRepository : IGenericRepository<Patient>
    {
        Task<IEnumerable<Patient>> GetAllWithUserAsync();
        Task<Patient?> GetWithAppointmentsAsync(int id);
        Task<IEnumerable<Patient>> SearchAsync(string searchTerm);
        Task<Patient?> GetByEmailAsync(string email);
    }
}
