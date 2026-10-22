using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeOperationsMvc.Controllers
{

	[Authorize]
	public class EmployeeController : Controller
	{
		public IActionResult Index()
		{
			return Content("Employee Operations: Employee Index");
		}

		public IActionResult Details(int id)
		{
			return Content($"Employee details requested for ID: {id}");
		}

		public IActionResult FindEmployee(int id)
		{
			if (id <= 0)
			{
				return NotFound("Employee was not found.");
			}

			return Content($"Employee ID {id} passed the demonstration check.");
		}

		public IActionResult ValidateEmployeeId(int id)
		{
			if (id <= 0)
			{
				return BadRequest("Employee ID must be greater than zero.");
			}

			return Content("Employee ID is valid.");
		}
	}
}
