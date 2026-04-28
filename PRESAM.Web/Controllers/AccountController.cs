// PRESAM.Web/Controllers/AccountController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using PRESAM.Application.DTOs;
using PRESAM.Domain.Entities;

namespace PRESAM.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Please verify your email first. Check your email for the verification code.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    RegistrationDate = DateTime.UtcNow,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");

                    // Generate 6-digit confirmation code
                    var code = new Random().Next(100000, 999999).ToString();

                    // Save code to user
                    user.EmailConfirmationCode = code;
                    user.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(10);
                    await _userManager.UpdateAsync(user);

                    // Send email with code
                    await _emailSender.SendEmailAsync(model.Email, "Your PRESAM Verification Code",
                        $"<div style='font-family: Arial, sans-serif; text-align: center; padding: 30px;'>" +
                        $"<h2 style='color: #c6a43f;'>Welcome to PRESAM!</h2>" +
                        $"<p style='font-size: 16px;'>Use the code below to verify your account:</p>" +
                        $"<div style='background: #f5f5f5; padding: 15px; border-radius: 10px; margin: 20px 0;'>" +
                        $"<h1 style='font-size: 48px; letter-spacing: 8px; color: #c6a43f; margin: 0;'>{code}</h1>" +
                        $"</div>" +
                        $"<p>This code will expire in <strong>10 minutes</strong>.</p>" +
                        $"<p>Enter this code on the verification page to activate your account.</p>" +
                        $"<hr style='margin: 20px 0;'>" +
                        $"<small style='color: #888;'>If you didn't create this account, please ignore this email.</small>" +
                        $"</div>");

                    return RedirectToAction("VerifyCode", new { email = model.Email });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyCode(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }

            var model = new VerifyCodeDto { Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyCode(VerifyCodeDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found. Please register again.");
                return View(model);
            }

            if (user.EmailConfirmed)
            {
                TempData["SuccessMessage"] = "Account already verified! Please login.";
                return RedirectToAction("Login");
            }

            if (user.EmailConfirmationCodeExpiry < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Verification code has expired. Please request a new code.");
                return View(model);
            }

            if (user.EmailConfirmationCode != model.Code)
            {
                ModelState.AddModelError("", "Invalid verification code. Please try again.");
                return View(model);
            }
            user.EmailConfirmed = true;
            user.EmailConfirmationCode = null;
            user.EmailConfirmationCodeExpiry = null;
            await _userManager.UpdateAsync(user);

            await _signInManager.SignInAsync(user, isPersistent: false);

            TempData["SuccessMessage"] = "Account verified successfully! Welcome to PRESAM!";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> ResendCode(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.EmailConfirmed)
            {
                return BadRequest("Email already verified. Please login.");
            }

            var newCode = new Random().Next(100000, 999999).ToString();

            user.EmailConfirmationCode = newCode;
            user.EmailConfirmationCodeExpiry = DateTime.UtcNow.AddMinutes(10);
            await _userManager.UpdateAsync(user);

            await _emailSender.SendEmailAsync(email, "Your New PRESAM Verification Code",
                $"<div style='font-family: Arial, sans-serif; text-align: center; padding: 30px;'>" +
                $"<h2 style='color: #c6a43f;'>New Verification Code</h2>" +
                $"<div style='background: #f5f5f5; padding: 15px; border-radius: 10px; margin: 20px 0;'>" +
                $"<h1 style='font-size: 48px; letter-spacing: 8px; color: #c6a43f; margin: 0;'>{newCode}</h1>" +
                $"</div>" +
                $"<p>This code will expire in <strong>10 minutes</strong>.</p>" +
                $"</div>");

            return Ok("Code resent successfully");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}