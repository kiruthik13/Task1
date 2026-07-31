using HospitalManagement.Web.Data;
using HospitalManagement.Web.Helpers;
using HospitalManagement.Web.Interfaces;
using HospitalManagement.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HospitalManagement.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDoctorService      _doctorService;
        private readonly IPatientService     _patientService;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ApplicationDbContext context,
            IDoctorService      doctorService,
            IPatientService     patientService,
            IAppointmentService appointmentService,
            ILogger<HomeController> logger)
        {
            _context            = context;
            _doctorService      = doctorService;
            _patientService     = patientService;
            _appointmentService = appointmentService;
            _logger             = logger;
        }

        public async Task<IActionResult> Index()
        {
            var now   = DateTime.UtcNow;
            var today = now.Date;
            var yesterday = today.AddDays(-1);

            var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var previousMonthStart = currentMonthStart.AddMonths(-1);

            // 1. Live Database Counts
            var totalDoctors      = await _context.Doctors.CountAsync();
            var totalPatients     = await _context.Patients.CountAsync();
            var totalAppointments = await _context.Appointments.CountAsync();
            var todayAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == today);
            var completedToday    = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == today && a.Status == "Completed");

            ViewBag.TotalDoctors      = totalDoctors;
            ViewBag.TotalPatients     = totalPatients;
            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.TodayAppointments = todayAppointments;
            ViewBag.CompletedToday    = completedToday;

            // 2. Dynamic Logical Trend Percentages
            int currentMonthPatients = await _context.Patients.CountAsync(p => p.CreatedDate >= currentMonthStart);
            int previousMonthPatients = await _context.Patients.CountAsync(p => p.CreatedDate >= previousMonthStart && p.CreatedDate < currentMonthStart);
            ViewBag.PatientTrendText = CalculateTrendText(currentMonthPatients, previousMonthPatients, "this month");

            int currentMonthDoctors = await _context.Doctors.CountAsync(d => d.CreatedDate >= currentMonthStart);
            int previousMonthDoctors = await _context.Doctors.CountAsync(d => d.CreatedDate >= previousMonthStart && d.CreatedDate < currentMonthStart);
            ViewBag.DoctorTrendText = CalculateTrendText(currentMonthDoctors, previousMonthDoctors, "this month");

            int yesterdayApts = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == yesterday);
            ViewBag.TodayAppointmentsTrendText = CalculateTrendText(todayAppointments, yesterdayApts, "vs yesterday");

            int yesterdayCompleted = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == yesterday && a.Status == "Completed");
            ViewBag.CompletedTodayTrendText = CalculateTrendText(completedToday, yesterdayCompleted, "vs yesterday");

            // 3. Department / Specialization Distribution
            var specData = await _context.Doctors
                .Where(d => !string.IsNullOrEmpty(d.Specialization))
                .GroupBy(d => d.Specialization)
                .Select(g => new { Specialization = g.Key, Count = g.Count() })
                .ToListAsync();

            var specLabels = specData.Select(s => s.Specialization).ToList();
            var specCounts = specData.Select(s => s.Count).ToList();
            if (!specLabels.Any())
            {
                specLabels = new List<string> { "Cardiology", "Neurology", "Orthopedics", "Pediatrics", "General Medicine" };
                specCounts = new List<int> { 5, 3, 2, 4, 6 };
            }
            ViewBag.SpecializationLabels = specLabels;
            ViewBag.SpecializationCounts = specCounts;
            ViewBag.TotalDepartments    = specLabels.Count;

            // 4. Weekly Appointments Trend (Current Week Mon-Sun)
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var monday = today.AddDays(-1 * diff);
            var sunday = monday.AddDays(7);

            var weekAppointments = await _context.Appointments
                .Where(a => a.AppointmentDate >= monday && a.AppointmentDate < sunday)
                .ToListAsync();

            var scheduledCounts = new int[7];
            var completedCounts = new int[7];
            var cancelledCounts = new int[7];

            for (int i = 0; i < 7; i++)
            {
                var day = monday.AddDays(i);
                scheduledCounts[i] = weekAppointments.Count(a => a.AppointmentDate.Date == day && a.Status == "Scheduled");
                completedCounts[i] = weekAppointments.Count(a => a.AppointmentDate.Date == day && a.Status == "Completed");
                cancelledCounts[i] = weekAppointments.Count(a => a.AppointmentDate.Date == day && a.Status == "Cancelled");
            }

            ViewBag.WeeklyScheduled = scheduledCounts;
            ViewBag.WeeklyCompleted = completedCounts;
            ViewBag.WeeklyCancelled = cancelledCounts;

            // 5. Recent Appointments
            var recentAppointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .OrderByDescending(a => a.CreatedDate)
                .ThenByDescending(a => a.AppointmentDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentAppointments = recentAppointments;

            // 6. Dynamic Notifications Stream
            var notifications = new List<object>();

            var latestPatient = await _context.Patients.OrderByDescending(p => p.CreatedDate).FirstOrDefaultAsync();
            if (latestPatient != null)
            {
                notifications.Add(new {
                    Icon = "bi-person-plus",
                    ColorClass = "text-primary",
                    Message = $"New patient {latestPatient.Name} registered",
                    TimeAgo = GetTimeAgo(latestPatient.CreatedDate)
                });
            }

            var latestApt = await _context.Appointments.Include(a => a.Patient).OrderByDescending(a => a.CreatedDate).FirstOrDefaultAsync();
            if (latestApt != null)
            {
                notifications.Add(new {
                    Icon = "bi-calendar-event",
                    ColorClass = "text-info",
                    Message = $"Appointment #APT-{latestApt.Id} for {latestApt.Patient?.Name ?? "Patient"} ({latestApt.Status})",
                    TimeAgo = GetTimeAgo(latestApt.CreatedDate)
                });
            }

            var latestDoc = await _context.Doctors.OrderByDescending(d => d.UpdatedDate).FirstOrDefaultAsync();
            if (latestDoc != null)
            {
                notifications.Add(new {
                    Icon = "bi-person-badge",
                    ColorClass = "text-warning",
                    Message = $"Doctor {latestDoc.Name} profile updated",
                    TimeAgo = GetTimeAgo(latestDoc.UpdatedDate)
                });
            }

            notifications.Add(new {
                Icon = "bi-check-circle",
                ColorClass = "text-success",
                Message = "PostgreSQL Database connected & synchronized",
                TimeAgo = "Just now"
            });

            ViewBag.Notifications = notifications;

            _logger.LogInformation("Dashboard real-time data retrieved for {User}", User.Identity?.Name);
            return View();
        }

        /// <summary>
        /// JSON API endpoint for dynamic auto-refreshing dashboard metrics
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            var now   = DateTime.UtcNow;
            var today = now.Date;
            var yesterday = today.AddDays(-1);

            var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var previousMonthStart = currentMonthStart.AddMonths(-1);

            var totalDoctors      = await _context.Doctors.CountAsync();
            var totalPatients     = await _context.Patients.CountAsync();
            var totalAppointments = await _context.Appointments.CountAsync();
            var todayAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == today);
            var completedToday    = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == today && a.Status == "Completed");

            int currentMonthPatients = await _context.Patients.CountAsync(p => p.CreatedDate >= currentMonthStart);
            int previousMonthPatients = await _context.Patients.CountAsync(p => p.CreatedDate >= previousMonthStart && p.CreatedDate < currentMonthStart);
            string patientTrendText = CalculateTrendText(currentMonthPatients, previousMonthPatients, "this month");

            int currentMonthDoctors = await _context.Doctors.CountAsync(d => d.CreatedDate >= currentMonthStart);
            int previousMonthDoctors = await _context.Doctors.CountAsync(d => d.CreatedDate >= previousMonthStart && d.CreatedDate < currentMonthStart);
            string doctorTrendText = CalculateTrendText(currentMonthDoctors, previousMonthDoctors, "this month");

            int yesterdayApts = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == yesterday);
            string todayAppointmentsTrendText = CalculateTrendText(todayAppointments, yesterdayApts, "vs yesterday");

            int yesterdayCompleted = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == yesterday && a.Status == "Completed");
            string completedTodayTrendText = CalculateTrendText(completedToday, yesterdayCompleted, "vs yesterday");

            var specData = await _context.Doctors
                .Where(d => !string.IsNullOrEmpty(d.Specialization))
                .GroupBy(d => d.Specialization)
                .Select(g => new { Specialization = g.Key, Count = g.Count() })
                .ToListAsync();

            var specLabels = specData.Select(s => s.Specialization).ToList();
            var specCounts = specData.Select(s => s.Count).ToList();

            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var monday = today.AddDays(-1 * diff);
            var sunday = monday.AddDays(7);

            var weekAppointments = await _context.Appointments
                .Where(a => a.AppointmentDate >= monday && a.AppointmentDate < sunday)
                .ToListAsync();

            var scheduledCounts = new int[7];
            var completedCounts = new int[7];
            var cancelledCounts = new int[7];

            for (int i = 0; i < 7; i++)
            {
                var day = monday.AddDays(i);
                scheduledCounts[i] = weekAppointments.Count(a => a.AppointmentDate.Date == day && a.Status == "Scheduled");
                completedCounts[i] = weekAppointments.Count(a => a.AppointmentDate.Date == day && a.Status == "Completed");
                cancelledCounts[i] = weekAppointments.Count(a => a.AppointmentDate.Date == day && a.Status == "Cancelled");
            }

            var recentAppointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .OrderByDescending(a => a.CreatedDate)
                .ThenByDescending(a => a.AppointmentDate)
                .Take(5)
                .Select(a => new {
                    id = a.Id,
                    time = a.AppointmentDate.ToString("hh:mm tt"),
                    date = a.AppointmentDate.ToString("MMM dd, yyyy"),
                    patientName = a.Patient != null ? a.Patient.Name : "—",
                    doctorName = a.Doctor != null ? a.Doctor.Name : "—",
                    status = a.Status
                })
                .ToListAsync();

            return Json(new {
                totalDoctors,
                totalPatients,
                totalAppointments,
                todayAppointments,
                completedToday,
                patientTrendText,
                doctorTrendText,
                todayAppointmentsTrendText,
                completedTodayTrendText,
                specLabels,
                specCounts,
                weeklyScheduled = scheduledCounts,
                weeklyCompleted = completedCounts,
                weeklyCancelled = cancelledCounts,
                recentAppointments
            });
        }

        public IActionResult Privacy() => View();

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionFeature != null)
                _logger.LogError(exceptionFeature.Error, "Unhandled error on {Path}", exceptionFeature.Path);

            return View(new Models.ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        private static string CalculateTrendText(int current, int previous, string periodLabel)
        {
            if (previous == 0)
            {
                return current > 0 ? $"+100% {periodLabel}" : $"0% {periodLabel}";
            }
            double pct = ((double)(current - previous) / previous) * 100;
            string sign = pct >= 0 ? "+" : "";
            return $"{sign}{pct:F0}% {periodLabel}";
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow.Subtract(dateTime);
            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} min ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hr ago";
            return $"{(int)timeSpan.TotalDays} days ago";
        }
    }
}
