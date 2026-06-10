using Libraryapp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Libraryapp.Controllers
{
    public class AccountController : Controller
    {
        private readonly LibraryContext _context;

        public AccountController(LibraryContext context)
        {
            _context = context;
        }

        // ──────────────────────────── REGISTER ────────────────────────────

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if username already taken
            bool userExists = _context.Users.Any(u => u.Username == model.Username);
            if (userExists)
            {
                ModelState.AddModelError("Username", "Username is already taken. Please choose another.");
                return View(model);
            }

            // Check if email already taken
            bool emailExists = _context.Users.Any(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(model);
            }

            // Generate a random salt and hash the password
            string salt = GenerateSalt();
            string passwordHash = HashPassword(model.Password, salt);

            var newUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = passwordHash,
                Salt = salt,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToAction(nameof(Login));
        }

        // ──────────────────────────── LOGIN ────────────────────────────

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            // Look up user in database
            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

            if (user == null || !VerifyPassword(model.Password, user.Salt, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            // Build claims and sign in
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToLocal(returnUrl);
        }

        // ──────────────────────────── LOGOUT ────────────────────────────

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // ──────────────────────────── HELPERS ────────────────────────────

        private static string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        private static string HashPassword(string password, string salt)
        {
            string combined = salt + password;
            byte[] bytes = Encoding.UTF8.GetBytes(combined);
            byte[] hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        private static bool VerifyPassword(string password, string salt, string storedHash)
        {
            string computedHash = HashPassword(password, salt);
            return string.Equals(computedHash, storedHash, StringComparison.Ordinal);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
