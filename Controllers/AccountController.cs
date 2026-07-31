using HospitalManagement.Web.DTOs;
using HospitalManagement.Web.Helpers;
using HospitalManagement.Web.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly JwtService   _jwtService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, JwtService jwtService,
            ILogger<AccountController> logger)
        {
            _userService = userService;
            _jwtService  = jwtService;
            _logger      = logger;
        }

        // GET /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO dto, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(dto);

            var user = await _userService.AuthenticateAsync(dto.Email, dto.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                _logger.LogWarning("Failed login attempt for {Email}", dto.Email);
                return View(dto);
            }

            // Build cookie claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,           user.Name),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role?.Name ?? "Patient"),
                new Claim("UserId",                  user.Id.ToString()),
                new Claim("JwtToken",                _jwtService.GenerateToken(user))
            };

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = dto.RememberMe,
                    ExpiresUtc   = dto.RememberMe
                        ? DateTimeOffset.UtcNow.AddDays(7)
                        : DateTimeOffset.UtcNow.AddHours(1)
                });

            _logger.LogInformation("User {Email} logged in.", user.Email);
            TempData["Success"] = $"Welcome back, {user.Name}!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // GET /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var (success, message) = await _userService.RegisterAsync(dto);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(dto);
            }

            _logger.LogInformation("New user registered: {Email}", dto.Email);
            TempData["Success"] = "Registration successful! Please log in.";
            return RedirectToAction(nameof(Login));
        }

        // POST /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var name = User.Identity?.Name;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User {Name} logged out.", name);
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied() => View();
    }
}
