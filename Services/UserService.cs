using HospitalManagement.Web.DTOs;
using HospitalManagement.Web.Interfaces;
using HospitalManagement.Web.Models;

namespace HospitalManagement.Web.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository    _userRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly IDoctorRepository  _doctorRepo;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepo,
            IPatientRepository patientRepo,
            IDoctorRepository doctorRepo,
            ILogger<UserService> logger)
        {
            _userRepo    = userRepo;
            _patientRepo = patientRepo;
            _doctorRepo  = doctorRepo;
            _logger      = logger;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            try
            {
                var user = await _userRepo.GetByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Login failed for {Email}: user not found or inactive.", email);
                    return null;
                }

                bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                if (!valid)
                {
                    _logger.LogWarning("Login failed for {Email}: invalid password.", email);
                    return null;
                }

                _logger.LogInformation("User {Email} authenticated successfully.", email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user {Email}.", email);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDTO dto)
        {
            try
            {
                if (await _userRepo.EmailExistsAsync(dto.Email))
                    return (false, "Email already registered.");

                var user = new User
                {
                    Name         = dto.Name.Trim(),
                    Email        = dto.Email.Trim().ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    RoleId       = dto.RoleId,
                    IsActive     = true,
                    CreatedDate  = DateTime.UtcNow,
                    UpdatedDate  = DateTime.UtcNow
                };

                await _userRepo.AddAsync(user);
                await _userRepo.SaveChangesAsync();

                // Automatically create corresponding Patient profile if RoleId is 3 (Patient)
                if (dto.RoleId == 3)
                {
                    var existingPatient = await _patientRepo.FindAsync(p => p.Email.ToLower() == user.Email);
                    if (!existingPatient.Any())
                    {
                        var patient = new Patient
                        {
                            Name        = user.Name,
                            Email       = user.Email,
                            UserId      = user.Id,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow
                        };
                        await _patientRepo.AddAsync(patient);
                        await _patientRepo.SaveChangesAsync();
                        _logger.LogInformation("Patient profile created for user {Email}", user.Email);
                    }
                }
                // Automatically create corresponding Doctor profile if RoleId is 2 (Doctor)
                else if (dto.RoleId == 2)
                {
                    var existingDoctor = await _doctorRepo.FindAsync(d => d.Email.ToLower() == user.Email);
                    if (!existingDoctor.Any())
                    {
                        var doctor = new Doctor
                        {
                            Name           = user.Name,
                            Email          = user.Email,
                            Specialization = "General Medicine",
                            IsAvailable    = true,
                            UserId         = user.Id,
                            CreatedDate    = DateTime.UtcNow,
                            UpdatedDate    = DateTime.UtcNow
                        };
                        await _doctorRepo.AddAsync(doctor);
                        await _doctorRepo.SaveChangesAsync();
                        _logger.LogInformation("Doctor profile created for user {Email}", user.Email);
                    }
                }

                _logger.LogInformation("New user registered: {Email}", dto.Email);
                return (true, "Registration successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Email}.", dto.Email);
                return (false, "Registration failed. Please try again.");
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
            => await _userRepo.GetAllWithRolesAsync();

        public async Task<User?> GetUserByIdAsync(int id)
            => await _userRepo.GetWithRoleAsync(id);

        public async Task<(bool Success, string Message)> UpdateUserAsync(int id, string name, string email)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return (false, "User not found.");

            if (await _userRepo.ExistsAsync(u => u.Email.ToLower() == email.ToLower() && u.Id != id))
                return (false, "Email already in use.");

            user.Name        = name.Trim();
            user.Email       = email.Trim().ToLower();
            user.UpdatedDate = DateTime.UtcNow;
            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();
            return (true, "User updated successfully.");
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return (false, "User not found.");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                return (false, "Incorrect current password.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedDate  = DateTime.UtcNow;
            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();
            return (true, "Password changed successfully.");
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return false;

            _userRepo.Remove(user);
            await _userRepo.SaveChangesAsync();
            return true;
        }
    }
}
