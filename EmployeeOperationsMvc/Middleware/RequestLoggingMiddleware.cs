using System.Diagnostics;

namespace EmployeeOperationsMvc.Middleware
{
	public class RequestLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<RequestLoggingMiddleware> _logger;

		public RequestLoggingMiddleware(
			RequestDelegate next,
			ILogger<RequestLoggingMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var stopwatch = Stopwatch.StartNew();

			try
			{
				// Continue processing through the pipeline.
				await _next(context);
			}
			finally
			{
				// Runs even if downstream processing throws an exception.
				stopwatch.Stop();

				_logger.LogInformation(
					"HTTP Request: Method={Method}, Path={Path}, QueryString={QueryString}, " +
					"StatusCode={StatusCode}, ElapsedMs={ElapsedMs}",
					context.Request.Method,
					context.Request.Path,
					context.Request.QueryString,
					context.Response.StatusCode,
					stopwatch.ElapsedMilliseconds);
			}
		}
	}
}
