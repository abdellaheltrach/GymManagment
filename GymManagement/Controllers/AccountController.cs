using GymManagement.Domain.Entities.Identity;
using GymManagement.Web.Bases;
using GymManagement.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GymManagement.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        // ── Login GET ──────────────────────────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // ── Login POST ─────────────────────────────────────────────────────────────
        [HttpPost]
        [AllowAnonymous]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            // return validation errors to the view if model state is invalid
            if (!ModelState.IsValid) return View(vm);

            // get user by email and check if they exist and are active
            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user is null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(vm);
            }

            // sign in the user — SignInManager issues the Identity cookie automatically
            // the cookie contains: userId, roles, receptionist_permissions claim
            // [Authorize], User.IsInRole(), HasPermissionHandler all read from this cookie
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                vm.Password,
                vm.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // update last login timestamp
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {Email} logged in.", vm.Email);

                // redirect to return url if it's valid and local
                if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                    return Redirect(vm.ReturnUrl);

                // role-based redirect — each role lands on its own dashboard
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Trainer"))
                    return RedirectToAction("Index", "TrainerDashboard");

                if (roles.Contains("Receptionist"))
                    return RedirectToAction("Index", "ReceptionistDashboard");

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

            // SignOutAsync deletes the Identity cookie — no DB call needed
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }

        // ── Access Denied ──────────────────────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();
    }
}