using HospitalManagement.Web.DTOs;
using HospitalManagement.Web.Interfaces;
using HospitalManagement.Web.Models;

namespace HospitalManagement.Web.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(IAppointmentRepository appointmentRepo, ILogger<AppointmentService> logger)
        {
            _appointmentRepo = appointmentRepo;
            _logger          = logger;
        }

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
            => await _appointmentRepo.GetAllWithDetailsAsync();

        public async Task<Appointment?> GetAppointmentByIdAsync(int id)
            => await _appointmentRepo.GetWithDetailsAsync(id);

        public async Task<(bool Success, string Message)> CreateAppointmentAsync(AppointmentDTO dto)
        {
            try
            {
                var appointment = new Appointment
                {
                    DoctorId        = dto.DoctorId,
                    PatientId       = dto.PatientId,
                    AppointmentDate = dto.AppointmentDate,
                    Status          = dto.Status,
                    Type            = dto.Type,
                    Notes           = dto.Notes?.Trim(),
                    CreatedDate     = DateTime.UtcNow,
                    UpdatedDate     = DateTime.UtcNow
                };

                await _appointmentRepo.AddAsync(appointment);
                await _appointmentRepo.SaveChangesAsync();
                _logger.LogInformation("Appointment created for Doctor {DoctorId}, Patient {PatientId}.", dto.DoctorId, dto.PatientId);
                return (true, "Appointment created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment.");
                return (false, "Failed to create appointment.");
            }
        }

        public async Task<(bool Success, string Message)> UpdateAppointmentAsync(int id, AppointmentDTO dto)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(id);
            if (appointment == null) return (false, "Appointment not found.");

            appointment.DoctorId        = dto.DoctorId;
            appointment.PatientId       = dto.PatientId;
            appointment.AppointmentDate = dto.AppointmentDate;
            appointment.Status          = dto.Status;
            appointment.Type            = dto.Type;
            appointment.Notes           = dto.Notes?.Trim();
            appointment.UpdatedDate     = DateTime.UtcNow;

            _appointmentRepo.Update(appointment);
            await _appointmentRepo.SaveChangesAsync();
            _logger.LogInformation("Appointment {Id} updated.", id);
            return (true, "Appointment updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAppointmentAsync(int id)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(id);
            if (appointment == null) return (false, "Appointment not found.");

            _appointmentRepo.Remove(appointment);
            await _appointmentRepo.SaveChangesAsync();
            _logger.LogInformation("Appointment {Id} deleted.", id);
            return (true, "Appointment deleted successfully.");
        }

        public async Task<IEnumerable<Appointment>> SearchAppointmentsAsync(string searchTerm)
            => await _appointmentRepo.SearchAsync(searchTerm);

        public async Task<int> GetTotalCountAsync()
            => await _appointmentRepo.GetTotalCountAsync();

        public async Task<int> GetTodayCountAsync()
            => await _appointmentRepo.GetTodayCountAsync();
    }
}
