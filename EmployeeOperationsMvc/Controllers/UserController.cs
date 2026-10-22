using Microsoft.AspNetCore.Mvc;

namespace EmployeeOperationsMvc.Controllers
{
	public class UserController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Details(int id)
		{
			return Content($"User details for ID: {id}");
		}
	}
}
