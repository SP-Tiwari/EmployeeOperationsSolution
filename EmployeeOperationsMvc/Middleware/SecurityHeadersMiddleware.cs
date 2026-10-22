namespace EmployeeOperationsMvc.Middleware
{
	public class SecurityHeadersMiddleware
	{
		private readonly RequestDelegate _next;

		public SecurityHeadersMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			context.Response.OnStarting(() =>
			{
				var headers = context.Response.Headers;

				headers["X-Content-Type-Options"] = "nosniff";
				headers["X-Frame-Options"] = "DENY";
				headers["Referrer-Policy"] =
					"strict-origin-when-cross-origin";
				headers["Permissions-Policy"] =
					"camera=(), microphone=(), geolocation=()";

				headers["Content-Security-Policy"] =
					"default-src 'self'; " +
					"object-src 'none'; " +
					"base-uri 'self'; " +
					"frame-ancestors 'none'; " +
					"form-action 'self'; " +
					"img-src 'self' data:; " +
					"style-src 'self' 'unsafe-inline'; " +
					"script-src 'self'";

				return Task.CompletedTask;
			});

			await _next(context);
		}
	}
}
