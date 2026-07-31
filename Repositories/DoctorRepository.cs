using HospitalManagement.Web.Data;
using HospitalManagement.Web.Interfaces;
using HospitalManagement.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Web.Repositories
{
    public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Doctor>> GetAllWithUserAsync()
            => await _dbSet.Include(d => d.User)
                           .OrderBy(d => d.Name)
                           .ToListAsync();

        public async Task<Doctor?> GetWithUserAsync(int id)
            => await _dbSet.Include(d => d.User)
                           .Include(d => d.Appointments)
                           .FirstOrDefaultAsync(d => d.Id == id);

        public async Task<IEnumerable<Doctor>> SearchAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _dbSet
                .Where(d => d.Name.ToLower().Contains(term) ||
                            d.Email.ToLower().Contains(term) ||
                            d.Specialization.ToLower().Contains(term))
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync()
            => await _dbSet.Where(d => d.IsAvailable)
                           .OrderBy(d => d.Name)
                           .ToListAsync();

        public async Task<IEnumerable<Doctor>> GetBySpecializationAsync(string specialization)
            => await _dbSet.Where(d => d.Specialization.ToLower() == specialization.ToLower())
                           .OrderBy(d => d.Name)
                           .ToListAsync();
    }
}
