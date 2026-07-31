using HospitalManagement.Web.DTOs;
using HospitalManagement.Web.Models;

namespace HospitalManagement.Web.Interfaces
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<(bool Success, string Message)> RegisterAsync(RegisterDTO dto);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<(bool Success, string Message)> UpdateUserAsync(int id, string name, string email);
        Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> DeleteUserAsync(int id);
    }
}
