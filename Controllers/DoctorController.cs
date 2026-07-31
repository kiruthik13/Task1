using HospitalManagement.Web.DTOs;
using HospitalManagement.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,Doctor")]
    public class DoctorController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly ILogger<DoctorController> _logger;

        public DoctorController(IDoctorService doctorService, ILogger<DoctorController> logger)
        {
            _doctorService = doctorService;
            _logger        = logger;
        }

        // GET /Doctor
        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
        {
            var doctors = string.IsNullOrWhiteSpace(search)
                ? await _doctorService.GetAllDoctorsAsync()
                : await _doctorService.SearchDoctorsAsync(search);

            // Pagination
            var total   = doctors.Count();
            var paged   = doctors.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Search     = search;
            ViewBag.Page       = page;
            ViewBag.PageSize   = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Total      = total;

            return View(paged);
        }

        // GET /Doctor/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null)
            {
                TempData["Error"] = "Doctor not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        // GET /Doctor/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View(new DoctorDTO());

        // POST /Doctor/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var (success, message) = await _doctorService.CreateDoctorAsync(dto);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(dto);
            }

            _logger.LogInformation("Doctor {Name} created by {User}", dto.Name, User.Identity?.Name);
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET /Doctor/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null)
            {
                TempData["Error"] = "Doctor not found.";
                return RedirectToAction(nameof(Index));
            }

            var dto = new DoctorDTO
            {
                Name           = doctor.Name,
                Email          = doctor.Email,
                Specialization = doctor.Specialization,
                Phone          = doctor.Phone,
                Qualification  = doctor.Qualification,
                Biography      = doctor.Biography,
                IsAvailable    = doctor.IsAvailable
            };
            ViewBag.DoctorId = id;
            return View(dto);
        }

        // POST /Doctor/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DoctorDTO dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.DoctorId = id;
                return View(dto);
            }

            var (success, message) = await _doctorService.UpdateDoctorAsync(id, dto);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.DoctorId = id;
                return View(dto);
            }

            _logger.LogInformation("Doctor {Id} updated by {User}", id, User.Identity?.Name);
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET /Doctor/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null)
            {
                TempData["Error"] = "Doctor not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        // POST /Doctor/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (success, message) = await _doctorService.DeleteDoctorAsync(id);
            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("Doctor {Id} deleted by {User}", id, User.Identity?.Name);
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET /Doctor/MyProfile
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out int userId);

            var doctors = await _doctorService.GetAllDoctorsAsync();
            var doctor  = doctors.FirstOrDefault(d => d.UserId == userId || (userEmail != null && d.Email.ToLower() == userEmail.ToLower()));

            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile not found.";
                return RedirectToAction("Index", "Home");
            }

            var dto = new DoctorDTO
            {
                Name           = doctor.Name,
                Email          = doctor.Email,
                Specialization = doctor.Specialization,
                Phone          = doctor.Phone,
                Qualification  = doctor.Qualification,
                Biography      = doctor.Biography,
                IsAvailable    = doctor.IsAvailable
            };
            ViewBag.DoctorId = doctor.Id;
            return View(dto);
        }

        // POST /Doctor/MyProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyProfile(DoctorDTO dto)
        {
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out int userId);

            var doctors = await _doctorService.GetAllDoctorsAsync();
            var doctor  = doctors.FirstOrDefault(d => d.UserId == userId || (userEmail != null && d.Email.ToLower() == userEmail.ToLower()));

            if (doctor == null)
            {
                TempData["Error"] = "Doctor profile not found.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DoctorId = doctor.Id;
                return View(dto);
            }

            var (success, message) = await _doctorService.UpdateDoctorAsync(doctor.Id, dto);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.DoctorId = doctor.Id;
                return View(dto);
            }

            _logger.LogInformation("Doctor profile updated for {Email}", userEmail);
            TempData["Success"] = "Your doctor profile has been updated successfully.";
            return RedirectToAction(nameof(MyProfile));
        }
    }
}
