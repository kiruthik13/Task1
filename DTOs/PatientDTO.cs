using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Web.DTOs
{
    public class PatientDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(200)]
        [Display(Name = "Medical History")]
        public string? MedicalHistory { get; set; }
    }
}
