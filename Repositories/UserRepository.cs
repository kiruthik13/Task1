using HospitalManagement.Web.Data;
using HospitalManagement.Web.Interfaces;
using HospitalManagement.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Web.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
            => await _dbSet.Include(u => u.Role)
                           .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        public async Task<User?> GetWithRoleAsync(int id)
            => await _dbSet.Include(u => u.Role)
                           .FirstOrDefaultAsync(u => u.Id == id);

        public async Task<IEnumerable<User>> GetAllWithRolesAsync()
            => await _dbSet.Include(u => u.Role)
                           .OrderBy(u => u.Name)
                           .ToListAsync();

        public async Task<bool> EmailExistsAsync(string email)
            => await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }
}
