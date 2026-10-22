using System.Security.Cryptography;
using System.Text;
using EmployeeOperationsMvc.Attributes;

namespace EmployeeOperationsMvc.Middleware
{
	public class ApiKeyValidationMiddleware
	{
		private const string HeaderName = "X-API-Key";

		private readonly RequestDelegate _next;
		private readonly IConfiguration _configuration;
		private readonly ILogger<ApiKeyValidationMiddleware> _logger;

		public ApiKeyValidationMiddleware(
			RequestDelegate next,
			IConfiguration configuration,
			ILogger<ApiKeyValidationMiddleware> logger)
		{
			_next = next;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			// Routing must run before this middleware.
			var endpoint = context.GetEndpoint();

			var requiresApiKey =
				endpoint?.Metadata
					.GetMetadata<ApiKeyRequiredAttribute>() is not null;

			// Public endpoints do not require an API key.
			if (!requiresApiKey)
			{
				await _next(context);
				return;
			}

			var expectedKey = _configuration["ApiKey:Secret"];

			if (string.IsNullOrWhiteSpace(expectedKey))
			{
				_logger.LogError(
					"API key validation is not configured.");

				context.Response.StatusCode =
					StatusCodes.Status500InternalServerError;

				await context.Response.WriteAsync(
					"Server configuration error.");

				return;
			}

			var providedKey =
				context.Request.Headers[HeaderName].ToString();

			if (string.IsNullOrWhiteSpace(providedKey) ||
				!KeysMatch(providedKey, expectedKey))
			{
				context.Response.StatusCode =
					StatusCodes.Status401Unauthorized;

				context.Response.ContentType = "text/plain";

				await context.Response.WriteAsync(
					"A valid API key is required.");

				return;
			}

			// The key is valid; continue to the controller.
			await _next(context);
		}

		private static bool KeysMatch(
			string providedKey,
			string expectedKey)
		{
			var providedBytes =
				Encoding.UTF8.GetBytes(providedKey);

			var expectedBytes =
				Encoding.UTF8.GetBytes(expectedKey);

			return CryptographicOperations.FixedTimeEquals(
				providedBytes,
				expectedBytes);
		}
	}
}
