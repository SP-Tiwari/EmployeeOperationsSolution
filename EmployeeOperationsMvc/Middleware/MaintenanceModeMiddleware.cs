namespace EmployeeOperationsMvc.Middleware
{
	public class MaintenanceModeMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IConfiguration _configuration;

		public MaintenanceModeMiddleware(
			RequestDelegate next,
			IConfiguration configuration)
		{
			_next = next;
			_configuration = configuration;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			// Diagnostic endpoint for this training lab.
			if (context.Request.Path.Equals(
				"/middleware-demo/maintenance-status",
				StringComparison.OrdinalIgnoreCase))
			{
				bool isEnabled = _configuration.GetValue<bool>(
					"MaintenanceMode:Enabled");

				context.Response.StatusCode = StatusCodes.Status200OK;
				context.Response.ContentType = "text/plain; charset=utf-8";

				await context.Response.WriteAsync(
					$"Maintenance mode: {(isEnabled ? "ENABLED" : "DISABLED")}");

				return;
			}

			bool maintenanceEnabled = _configuration.GetValue<bool>(
				"MaintenanceMode:Enabled");

			if (maintenanceEnabled)
			{
				context.Response.StatusCode =
					StatusCodes.Status503ServiceUnavailable;

				context.Response.ContentType = "text/plain; charset=utf-8";
				context.Response.Headers["Retry-After"] = "300";
				context.Response.Headers["Cache-Control"] = "no-store";

				await context.Response.WriteAsync(
					"Planned maintenance notice: The Employee Operations System " +
					"is temporarily unavailable due to scheduled maintenance. " +
					"Please try again later.");

				// Short-circuit the pipeline.
				return;
			}

			// Maintenance is disabled: continue normally.
			await _next(context);
		}
	}
}
