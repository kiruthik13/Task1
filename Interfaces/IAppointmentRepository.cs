using HospitalManagement.Web.Models;

namespace HospitalManagement.Web.Interfaces
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetAllWithDetailsAsync();
        Task<Appointment?> GetWithDetailsAsync(int id);
        Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);
        Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId);
        Task<IEnumerable<Appointment>> SearchAsync(string searchTerm);
        Task<int> GetTotalCountAsync();
        Task<int> GetTodayCountAsync();
    }
}
