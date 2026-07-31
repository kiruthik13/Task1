using HospitalManagement.Web.Models;

namespace HospitalManagement.Web.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetWithRoleAsync(int id);
        Task<IEnumerable<User>> GetAllWithRolesAsync();
        Task<bool> EmailExistsAsync(string email);
    }
}
