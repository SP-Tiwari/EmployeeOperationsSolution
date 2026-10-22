using EmployeeOperationsMvc.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeOperationsMvc.Controllers
{
	public class HomeController : Controller
	{
		private readonly IConfiguration _configuration;

		public HomeController(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		[AllowAnonymous]
		[HttpGet]
		public IActionResult Login(string? returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			return View(new LoginViewModel());
		}

		[AllowAnonymous]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(
			LoginViewModel model,
			string? returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var expectedUsername = _configuration["DemoLogin:Username"];
			var expectedPassword = _configuration["DemoLogin:Password"];

			if (string.IsNullOrWhiteSpace(expectedUsername) ||
				string.IsNullOrWhiteSpace(expectedPassword))
			{
				// Do not expose secret values in the response or logs.
				return StatusCode(StatusCodes.Status500InternalServerError,
					"Demo login is not configured. Set local User Secrets.");
			}

			var usernameMatches = string.Equals(
				model.Username,
				expectedUsername,
				StringComparison.Ordinal);

			var passwordMatches = string.Equals(
				model.Password,
				expectedPassword,
				StringComparison.Ordinal);

			if (!usernameMatches || !passwordMatches)
			{
				ModelState.AddModelError(string.Empty,
					"The username or password is incorrect.");
				return View(model);
			}

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, expectedUsername),
				new Claim(ClaimTypes.Name, expectedUsername)
			};

			var identity = new ClaimsIdentity(
				claims,
				CookieAuthenticationDefaults.AuthenticationScheme);

			var principal = new ClaimsPrincipal(identity);

			var properties = new AuthenticationProperties
			{
				IsPersistent = model.RememberMe,
				AllowRefresh = true
			};

			await HttpContext.SignInAsync(
				CookieAuthenticationDefaults.AuthenticationScheme,
				principal,
				properties);

			if (!string.IsNullOrWhiteSpace(returnUrl) &&
				Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}

			return RedirectToAction(nameof(SecureDashboard));
		}

		[Authorize]
		[HttpGet]
		public IActionResult SecureDashboard()
		{
			return View();
		}

		[AllowAnonymous]
		[HttpGet]
		public IActionResult AccessDenied()
		{
			return View();
		}

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(
				CookieAuthenticationDefaults.AuthenticationScheme);

			return RedirectToAction(nameof(Login));
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Login()
		{
			return View();
		}

		public IActionResult AboutSystem()
		{
			return Content(
				"Employee Operations System - ASP.NET Core MVC Training Project");
		}

		public IActionResult ServiceUnavailable()
		{
			return StatusCode(
				StatusCodes.Status503ServiceUnavailable,
				"Employee Operations System is temporarily unavailable.");
		}

		public IActionResult ConfirmProcessing()
		{
			return NoContent();
		}

		public IActionResult GetEmployeeSummary()
		{
			var employee = new
			{
				Id = 101,
				Name = "Amit Sharma",
				Department = "IT",
				Designation = "Software Developer"
			};

			return Json(employee);
		}

		public IActionResult OpenEmployees()
		{
			return RedirectToAction(
				actionName: "Index",
				controllerName: "Employee");
		}

		public IActionResult OpenEmployeeDetails()
		{
			return RedirectToAction(
				actionName: "Details",
				controllerName: "Employee",
				routeValues: new { id = 205 });
		}

		public IActionResult OpenEmployeePage()
		{
			return Redirect("/Employee/Index");
		}
	}
}
