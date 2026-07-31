using HospitalManagement.Web.DTOs;
using HospitalManagement.Web.Models;

namespace HospitalManagement.Web.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
        Task<Appointment?> GetAppointmentByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAppointmentAsync(AppointmentDTO dto);
        Task<(bool Success, string Message)> UpdateAppointmentAsync(int id, AppointmentDTO dto);
        Task<(bool Success, string Message)> DeleteAppointmentAsync(int id);
        Task<IEnumerable<Appointment>> SearchAppointmentsAsync(string searchTerm);
        Task<int> GetTotalCountAsync();
        Task<int> GetTodayCountAsync();
    }
}
