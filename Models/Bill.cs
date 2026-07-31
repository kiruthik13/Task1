using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Web.Models
{
    public class Bill : BaseEntity
    {
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Cancelled

        [StringLength(500)]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        // FKs
        public int PatientId { get; set; }
        public int AppointmentId { get; set; }

        // Navigation
        public Patient? Patient { get; set; }
        public Appointment? Appointment { get; set; }
    }
}
