using System.Threading.RateLimiting;
using EmployeeOperationsMvc.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
Console.WriteLine("Hello Git");
var builder = WebApplication.CreateBuilder(args);

// Keep existing builder creation and service registrations.
builder.Services.AddControllersWithViews();

builder.Services
	.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.Cookie.Name = "__Host-EmployeeOperations.Auth";
		options.Cookie.HttpOnly = true;
		options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
		options.Cookie.SameSite = SameSiteMode.Lax;
		options.LoginPath = "/Home/Login";
		options.AccessDeniedPath = "/Home/AccessDenied";
		options.SlidingExpiration = true;
		options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
	});



builder.Services.AddAuthorization();
builder.Services.AddMvc();

builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

	options.OnRejected = async (context, cancellationToken) =>
	{
		context.HttpContext.Response.ContentType = "application/json";

		if (context.Lease.TryGetMetadata(
			MetadataName.RetryAfter, out TimeSpan retryAfter))
		{
			context.HttpContext.Response.Headers.RetryAfter =
				((int)retryAfter.TotalSeconds).ToString();
		}

		await context.HttpContext.Response.WriteAsJsonAsync(
			new
			{
				error = "rate_limit_exceeded",
				message = "Too many requests. Wait before trying again."
			},
			cancellationToken);
	};

	options.AddFixedWindowLimiter("IntegrationPolicy", limiterOptions =>
	{
		limiterOptions.PermitLimit = 5;
		limiterOptions.Window = TimeSpan.FromMinutes(1);
		limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
		limiterOptions.QueueLimit = 0;
		limiterOptions.AutoReplenishment = true;
	});
});

builder.Services.AddCors(options =>
{
	options.AddPolicy("ApprovedFrontendPolicy", policy =>
	{
		policy
			.WithOrigins("http://localhost:5500")
			.WithMethods("GET")
			.WithHeaders("Content-Type", "X-API-Key");
	});
});


var app = builder.Build();

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RequestExecutionTimeMiddleware>();

if (!app.Environment.IsDevelopment())
{
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// CORS must handle preflight OPTIONS before custom middleware rejects requests.
app.UseCors();

// Keep IP restrictions and API-key validation after CORS.
// Verify these middleware implementations and their intended security behavior.
app.UseMiddleware<IpRestrictionMiddleware>();
app.UseMiddleware<ApiKeyValidationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

// Preserve existing middleware.
app.UseMiddleware<MaintenanceModeMiddleware>();
app.UseMiddleware<ConditionalRequestMiddleware>();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Login}/{id?}");

app.MapControllers();

app.Run();