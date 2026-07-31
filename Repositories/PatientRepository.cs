using HospitalManagement.Web.Data;
using HospitalManagement.Web.Interfaces;
using HospitalManagement.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Web.Repositories
{
    public class PatientRepository : GenericRepository<Patient>, IPatientRepository
    {
        public PatientRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Patient>> GetAllWithUserAsync()
            => await _dbSet.Include(p => p.User)
                           .OrderBy(p => p.Name)
                           .ToListAsync();

        public async Task<Patient?> GetWithAppointmentsAsync(int id)
            => await _dbSet.Include(p => p.Appointments)
                               .ThenInclude(a => a.Doctor)
                           .Include(p => p.Bills)
                           .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Patient>> SearchAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _dbSet
                .Where(p => p.Name.ToLower().Contains(term) ||
                            p.Email.ToLower().Contains(term) ||
                            (p.Phone != null && p.Phone.Contains(term)))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Patient?> GetByEmailAsync(string email)
            => await _dbSet.FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower());
    }
}
