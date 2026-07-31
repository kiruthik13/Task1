using HospitalManagement.Web.Data;
using HospitalManagement.Web.Helpers;
using HospitalManagement.Web.Interfaces;
using HospitalManagement.Web.Middleware;
using HospitalManagement.Web.Repositories;
using HospitalManagement.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Fix for Npgsql: treat all DateTime as UTC (required for PostgreSQL timestamptz)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ─── MVC ──────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
{
    // Global anti-forgery filter
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

// ─── Database ─────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// ─── Repositories ─────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository,        UserRepository>();
builder.Services.AddScoped<IDoctorRepository,      DoctorRepository>();
builder.Services.AddScoped<IPatientRepository,     PatientRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// ─── Services ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserService,        UserService>();
builder.Services.AddScoped<IDoctorService,      DoctorService>();
builder.Services.AddScoped<IPatientService,     PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<JwtService>();

// ─── Authentication — Cookie (MVC) + JWT Bearer (API/Postman) ─────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey   = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
var key         = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath          = "/Account/Login";
        options.LogoutPath         = "/Account/Logout";
        options.AccessDeniedPath   = "/Account/AccessDenied";
        options.ExpireTimeSpan     = TimeSpan.FromMinutes(60);
        options.SlidingExpiration  = true;
        options.Cookie.HttpOnly    = true;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
        options.Cookie.SameSite    = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(key),
            ValidateIssuer           = true,
            ValidIssuer              = jwtSettings["Issuer"],
            ValidateAudience         = true,
            ValidAudience            = jwtSettings["Audience"],
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ─── Session for TempData ─────────────────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout    = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ─── Data Protection & HTTP Context ────────────────────────────────────────
builder.Services.AddDataProtection()
    .SetApplicationName("HospitalManagement");

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ─── Seed Database ─────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await SeedData.InitializeAsync(app.Services, logger);
}

// ─── Middleware Pipeline ───────────────────────────────────────────────────
app.UseGlobalExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name:    "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
