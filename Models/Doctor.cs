using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Web.Models
{
    public class Doctor : BaseEntity
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Qualification { get; set; }

        [StringLength(500)]
        public string? Biography { get; set; }

        public bool IsAvailable { get; set; } = true;

        // FK
        public int? UserId { get; set; }

        // Navigation
        public User? User { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
