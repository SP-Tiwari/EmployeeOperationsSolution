using System.Net;
using EmployeeOperationsMvc.Attributes;

namespace EmployeeOperationsMvc.Middleware
{
	public class IpRestrictionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IConfiguration _configuration;
		private readonly ILogger<IpRestrictionMiddleware> _logger;

		public IpRestrictionMiddleware(
			RequestDelegate next,
			IConfiguration configuration,
			ILogger<IpRestrictionMiddleware> logger)
		{
			_next = next;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			// Routing must run before this middleware.
			var endpoint = context.GetEndpoint();

			var requiresIpRestriction =
				endpoint?.Metadata
					.GetMetadata<IpRestrictionRequiredAttribute>()
					is not null;

			if (!requiresIpRestriction)
			{
				await _next(context);
				return;
			}

			var allowedIps = _configuration
				.GetSection("IpRestrictions:AllowedIPs")
				.Get<string[]>();

			// Fail closed if the allowlist is missing or empty.
			if (allowedIps is null || allowedIps.Length == 0)
			{
				_logger.LogError(
					"IP restriction allowlist is not configured.");

				context.Response.StatusCode =
					StatusCodes.Status500InternalServerError;

				await context.Response.WriteAsync(
					"Server configuration error.");

				return;
			}

			var remoteIp = context.Connection.RemoteIpAddress;

			if (remoteIp is null ||
				!allowedIps.Any(ip => IsAllowedIp(remoteIp, ip)))
			{
				_logger.LogWarning(
					"Request blocked by IP restriction. Remote IP: {RemoteIp}",
					remoteIp?.ToString() ?? "Unknown");

				context.Response.StatusCode =
					StatusCodes.Status403Forbidden;

				context.Response.ContentType = "text/plain";
				await context.Response.WriteAsync(
					"Access denied for this IP address.");
				return;
			}

			await _next(context);
		}

		private static bool IsAllowedIp(
			IPAddress remoteIp,
			string configuredIp)
		{
			if (!IPAddress.TryParse(
					configuredIp, out var allowedIp))
			{
				return false;
			}

			return NormalizeIp(remoteIp).Equals(
				NormalizeIp(allowedIp));
		}

		private static IPAddress NormalizeIp(IPAddress ip)
		{
			return ip.IsIPv4MappedToIPv6
				? ip.MapToIPv4()
				: ip;
		}
	}
}
