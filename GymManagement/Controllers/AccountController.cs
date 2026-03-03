using GymManagement.Domain.Entities.Identity;
using GymManagement.Domain.Options;
using GymManagement.Web.Bases;
using GymManagement.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GymManagement.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly JwtSettings _jwtSettings;
        private readonly CookieSettings _cookieSettings;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger,
            JwtSettings jwtSettings,
            CookieSettings cookieSettings)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _jwtSettings = jwtSettings;
            _cookieSettings = cookieSettings;
        }

        // ── Login ──────────────────────────────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Look up user by email first — PasswordSignInAsync expects a username
            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user is null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(vm);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                vm.Password,
                vm.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Update last login timestamp
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {Email} logged in.", vm.Email);

                // Apply JWT and Cookie settings
                var token = GenerateJwtToken(user);
                
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = _cookieSettings.HttpOnly,
                    Secure = _cookieSettings.Secure,
                    SameSite = Enum.Parse<SameSiteMode>(_cookieSettings.SameSite),
                    Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationTimeInMinutes)
                };

                Response.Cookies.Append("JwtToken", token, cookieOptions);

                if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                    return Redirect(vm.ReturnUrl);

                return RedirectToAction("Index", "Dashboard");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} account locked out.", vm.Email);
                ModelState.AddModelError(string.Empty,
                    "Account locked due to multiple failed attempts. Try again in 15 minutes.");
                return View(vm);
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(vm);
        }

        // ── Logout ─────────────────────────────────────────────────────────────────
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("User {Name} logged out.", User.Identity?.Name);
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete("JwtToken");
            return RedirectToAction(nameof(Login));
        }

        // ── Access Denied ──────────────────────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationTimeInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}

