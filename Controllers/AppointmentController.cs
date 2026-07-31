using HospitalManagement.Web.DTOs;
using HospitalManagement.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalManagement.Web.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorService      _doctorService;
        private readonly IPatientService     _patientService;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(
            IAppointmentService appointmentService,
            IDoctorService      doctorService,
            IPatientService     patientService,
            ILogger<AppointmentController> logger)
        {
            _appointmentService = appointmentService;
            _doctorService      = doctorService;
            _patientService     = patientService;
            _logger             = logger;
        }

        // GET /Appointment
        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
        {
            var appointments = string.IsNullOrWhiteSpace(search)
                ? await _appointmentService.GetAllAppointmentsAsync()
                : await _appointmentService.SearchAppointmentsAsync(search);

            // Filter for Patient role: only view their own appointments
            if (User.IsInRole("Patient"))
            {
                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int.TryParse(userIdStr, out int userId);

                appointments = appointments.Where(a => a.Patient?.UserId == userId || 
                    (userEmail != null && a.Patient?.Email.ToLower() == userEmail.ToLower())).ToList();
            }

            var total = appointments.Count();
            var paged = appointments.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Search     = search;
            ViewBag.Page       = page;
            ViewBag.PageSize   = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Total      = total;

            return View(paged);
        }

        // GET /Appointment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            // Security check for Patients
            if (User.IsInRole("Patient"))
            {
                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int.TryParse(userIdStr, out int userId);

                if (appointment.Patient?.UserId != userId && (userEmail == null || appointment.Patient?.Email.ToLower() != userEmail.ToLower()))
                {
                    TempData["Error"] = "Access denied.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(appointment);
        }

        // GET /Appointment/Create
        public async Task<IActionResult> Create()
        {
            var dto = new AppointmentDTO { AppointmentDate = DateTime.Now.AddDays(1) };

            if (User.IsInRole("Patient"))
            {
                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int.TryParse(userIdStr, out int userId);

                var patients = await _patientService.GetAllPatientsAsync();
                var currentPatient = patients.FirstOrDefault(p => p.UserId == userId || (userEmail != null && p.Email.ToLower() == userEmail.ToLower()));

                if (currentPatient != null)
                {
                    dto.PatientId = currentPatient.Id;
                    ViewBag.IsPatient = true;
                    ViewBag.PatientName = currentPatient.Name;
                }
            }

            await PopulateDropdownsAsync(selectedPatient: dto.PatientId);
            return View(dto);
        }

        // POST /Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentDTO dto)
        {
            if (User.IsInRole("Patient"))
            {
                var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int.TryParse(userIdStr, out int userId);

                var patients = await _patientService.GetAllPatientsAsync();
                var currentPatient = patients.FirstOrDefault(p => p.UserId == userId || (userEmail != null && p.Email.ToLower() == userEmail.ToLower()));

                if (currentPatient != null)
                {
                    dto.PatientId = currentPatient.Id;
                    ViewBag.IsPatient = true;
                    ViewBag.PatientName = currentPatient.Name;
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(selectedDoctor: dto.DoctorId, selectedPatient: dto.PatientId);
                return View(dto);
            }

            var (success, message) = await _appointmentService.CreateAppointmentAsync(dto);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                await PopulateDropdownsAsync(selectedDoctor: dto.DoctorId, selectedPatient: dto.PatientId);
                return View(dto);
            }

            _logger.LogInformation("Appointment created by {User}", User.Identity?.Name);
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET /Appointment/Edit/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }

            var dto = new AppointmentDTO
            {
                DoctorId        = appointment.DoctorId,
                PatientId       = appointment.PatientId,
                AppointmentDate = appointment.AppointmentDate,
                Status          = appointment.Status,
                Type            = appointment.Type,
                Notes           = appointment.Notes
            };

            ViewBag.AppointmentId = id;
            await PopulateDropdownsAsync(dto.DoctorId, dto.PatientId);
            return View(dto);
        }

        // POST /Appointment/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentDTO dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AppointmentId = id;
                await PopulateDropdownsAsync(dto.DoctorId, dto.PatientId);
                return View(dto);
            }

            var (success, message) = await _appointmentService.UpdateAppointmentAsync(id, dto);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.AppointmentId = id;
                await PopulateDropdownsAsync(dto.DoctorId, dto.PatientId);
                return View(dto);
            }

            _logger.LogInformation("Appointment {Id} updated by {User}", id, User.Identity?.Name);
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET /Appointment/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(appointment);
        }

        // POST /Appointment/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (success, message) = await _appointmentService.DeleteAppointmentAsync(id);
            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("Appointment {Id} deleted by {User}", id, User.Identity?.Name);
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // Helper
        private async Task PopulateDropdownsAsync(int selectedDoctor = 0, int selectedPatient = 0)
        {
            var doctors  = await _doctorService.GetAvailableDoctorsAsync();
            var patients = await _patientService.GetAllPatientsAsync();

            ViewBag.Doctors  = new SelectList(doctors,  "Id", "Name", selectedDoctor);
            ViewBag.Patients = new SelectList(patients, "Id", "Name", selectedPatient);

            ViewBag.Statuses = new SelectList(new[]
            {
                new { Value = "Scheduled",  Text = "Scheduled"  },
                new { Value = "Completed",  Text = "Completed"  },
                new { Value = "Cancelled",  Text = "Cancelled"  }
            }, "Value", "Text");

            ViewBag.Types = new SelectList(new[]
            {
                new { Value = "General",    Text = "General Consultation" },
                new { Value = "Follow-up",  Text = "Follow-up"            },
                new { Value = "Emergency",  Text = "Emergency"            },
                new { Value = "Specialist", Text = "Specialist"           }
            }, "Value", "Text");
        }
    }
}
