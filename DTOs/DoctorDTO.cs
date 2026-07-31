using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Web.DTOs
{
    public class DoctorDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Specialization is required")]
        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Qualification { get; set; }

        [StringLength(500)]
        public string? Biography { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;
    }
}
