using HospitalManagement.Web.DTOs;
using HospitalManagement.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<PatientController> _logger;

        public PatientController(IPatientService patientService, ILogger<PatientController> logger)
        {
            _patientService = patientService;
            _logger         = logger;
        }

        // GET /Patient
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
        {
            var patients = string.IsNullOrWhiteSpace(search)
                ? await _patientService.GetAllPatientsAsync()
                : await _patientService.SearchPatientsAsync(search);

            var total = patients.Count();
            var paged = patients.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Search     = search;
            ViewBag.Page       = page;
            ViewBag.PageSize   = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Total      = total;

            return View(paged);
        }

        // GET /Patient/Details/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Details(int id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET /Patient/Create
        [Authorize(Roles = "Admin,Doctor")]
        public IActionResult Create() => View(new PatientDTO());

        // POST /Patient/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var (success, message) = await _patientService.CreatePatientAsync(dto);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(dto);
            }

            _logger.LogInformation("Patient {Name} created by {User}", dto.Name, User.Identity?.Name);
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET /Patient/Edit/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }

            var dto = new PatientDTO
            {
                Name           = patient.Name,
                Email          = patient.Email,
                DateOfBirth    = patient.DateOfBirth,
                Phone          = patient.Phone,
                Address        = patient.Address,
                Gender         = patient.Gender,
                MedicalHistory = patient.MedicalHistory
            };
            ViewBag.PatientId = id;
            return View(dto);
        }

        // POST /Patient/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PatientDTO dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PatientId = id;
                return View(dto);
            }

            var (success, message) = await _patientService.UpdatePatientAsync(id, dto);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.PatientId = id;
                return View(dto);
            }

            _logger.LogInformation("Patient {Id} updated by {User}", id, User.Identity?.Name);
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET /Patient/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // POST /Patient/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (success, message) = await _patientService.DeletePatientAsync(id);
            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("Patient {Id} deleted by {User}", id, User.Identity?.Name);
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET /Patient/MyProfile
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out int userId);

            var patients = await _patientService.GetAllPatientsAsync();
            var patient  = patients.FirstOrDefault(p => p.UserId == userId || (userEmail != null && p.Email.ToLower() == userEmail.ToLower()));

            if (patient == null)
            {
                TempData["Error"] = "Patient profile not found.";
                return RedirectToAction("Index", "Home");
            }

            var dto = new PatientDTO
            {
                Name           = patient.Name,
                Email          = patient.Email,
                DateOfBirth    = patient.DateOfBirth,
                Phone          = patient.Phone,
                Address        = patient.Address,
                Gender         = patient.Gender,
                MedicalHistory = patient.MedicalHistory
            };
            ViewBag.PatientId = patient.Id;
            return View(dto);
        }

        // POST /Patient/MyProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyProfile(PatientDTO dto)
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out int userId);

            var patients = await _patientService.GetAllPatientsAsync();
            var patient  = patients.FirstOrDefault(p => p.UserId == userId || (userEmail != null && p.Email.ToLower() == userEmail.ToLower()));

            if (patient == null)
            {
                TempData["Error"] = "Patient profile not found.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PatientId = patient.Id;
                return View(dto);
            }

            var (success, message) = await _patientService.UpdatePatientAsync(patient.Id, dto);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.PatientId = patient.Id;
                return View(dto);
            }

            _logger.LogInformation("Patient profile updated for {Email}", patient.Email);
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(MyProfile));
        }
    }
}
