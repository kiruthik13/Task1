using HospitalManagement.Web.Data;
using HospitalManagement.Web.Interfaces;
using HospitalManagement.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Web.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Appointment>> GetAllWithDetailsAsync()
            => await _dbSet.Include(a => a.Doctor)
                           .Include(a => a.Patient)
                           .OrderByDescending(a => a.AppointmentDate)
                           .ToListAsync();

        public async Task<Appointment?> GetWithDetailsAsync(int id)
            => await _dbSet.Include(a => a.Doctor)
                           .Include(a => a.Patient)
                           .Include(a => a.Bill)
                           .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId)
            => await _dbSet.Include(a => a.Patient)
                           .Where(a => a.DoctorId == doctorId)
                           .OrderByDescending(a => a.AppointmentDate)
                           .ToListAsync();

        public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId)
            => await _dbSet.Include(a => a.Doctor)
                           .Where(a => a.PatientId == patientId)
                           .OrderByDescending(a => a.AppointmentDate)
                           .ToListAsync();

        public async Task<IEnumerable<Appointment>> SearchAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _dbSet
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Where(a => a.Doctor!.Name.ToLower().Contains(term) ||
                            a.Patient!.Name.ToLower().Contains(term) ||
                            a.Status.ToLower().Contains(term))
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
            => await _dbSet.CountAsync();

        public async Task<int> GetTodayCountAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _dbSet.CountAsync(a => a.AppointmentDate.Date == today);
        }
    }
}
