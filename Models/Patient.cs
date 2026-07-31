using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Web.Models
{
    public class Patient : BaseEntity
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(200)]
        [Display(Name = "Medical History")]
        public string? MedicalHistory { get; set; }

        // FK
        public int? UserId { get; set; }

        // Navigation
        public User? User { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    }
}
