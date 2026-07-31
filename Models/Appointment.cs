using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Web.Models
{
    public class Appointment : BaseEntity
    {
        [Required]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(20)]
        public string? Type { get; set; } = "General"; // General, Follow-up, Emergency

        // FKs
        public int DoctorId { get; set; }
        public int PatientId { get; set; }

        // Navigation
        public Doctor? Doctor { get; set; }
        public Patient? Patient { get; set; }
        public Bill? Bill { get; set; }
    }
}
