using System.Diagnostics;

namespace EmployeeOperationsMvc.Middleware
{
	public class RequestExecutionTimeMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<RequestExecutionTimeMiddleware> _logger;

		public RequestExecutionTimeMiddleware(
			RequestDelegate next,
			ILogger<RequestExecutionTimeMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var stopwatch = Stopwatch.StartNew();

			try
			{
				// Measure downstream middleware and the MVC endpoint,
				// if it is reached.
				await _next(context);
			}
			finally
			{
				stopwatch.Stop();

				_logger.LogInformation(
					"Pipeline execution time: {Method} {Path} " +
					"took {ElapsedMs} ms. StatusCode={StatusCode}",
					context.Request.Method,
					context.Request.Path,
					stopwatch.ElapsedMilliseconds,
					context.Response.StatusCode);

				if (stopwatch.ElapsedMilliseconds > 500)
				{
					_logger.LogWarning(
						"Slow request: {Method} {Path} took {ElapsedMs} ms.",
						context.Request.Method,
						context.Request.Path,
						stopwatch.ElapsedMilliseconds);
				}
			}
		}
	}
}
