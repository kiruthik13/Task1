using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Web.DTOs
{
    public class AppointmentDTO
    {
        [Required(ErrorMessage = "Doctor is required")]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Patient is required")]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Appointment date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Appointment Date & Time")]
        public DateTime AppointmentDate { get; set; } = DateTime.Now.AddDays(1);

        [StringLength(20)]
        public string Status { get; set; } = "Scheduled";

        [StringLength(20)]
        [Display(Name = "Type")]
        public string? Type { get; set; } = "General";

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
