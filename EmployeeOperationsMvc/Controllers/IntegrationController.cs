using EmployeeOperationsMvc.Attributes;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EmployeeOperationsMvc.Controllers
{
	[ApiController]
	[Route("integration")]
	public class IntegrationController : ControllerBase
	{
		[EnableCors("ApprovedFrontendPolicy")]
		[HttpGet("/integration/employee-summary")]
		[HttpGet("employee-summary")]
		[EnableRateLimiting("IntegrationPolicy")]
		[ApiKeyRequired]
		[IpRestrictionRequired]
		public IActionResult GetEmployeeSummary()
		{
			var employee = new
			{
				Id = 101,
				Name = "Amit Sharma",
				Department = "IT",
				Designation = "Software Developer"
			};

			return Ok(employee);
		}

		[HttpGet("status")]
		public IActionResult GetStatus()
		{
			return Ok(new
			{
				Status = "Integration service is available"
			});
		}
	}
}
