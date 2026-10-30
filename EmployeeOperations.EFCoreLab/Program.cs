using EmployeeOperations.Application.Abstractions;
using EmployeeOperations.Application.Employees;
using EmployeeOperations.Infrastructure.Data;
using EmployeeOperations.Infrastructure.Employees;
using EmployeeOperations.Infrastructure.Models;
using EmployeeOperations.Infrastructure.Persistence;
using EmployeeOperationsSolution.EmployeeOperations.EFCoreLab.Contracts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEmployeeRepository, EfEmployeeRepository>();
builder.Services.AddScoped<EmployeeDirectoryService>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped<IContextLifetimeProbe, ContextLifetimeProbe>();



// Read ConnectionStrings:EmployeeOperations from appsettings.json
// and register the DbContext with ASP.NET Core dependency injection.
var connectionString =
	builder.Configuration.GetConnectionString("EmployeeOperations")
	?? throw new InvalidOperationException(
		"Connection string 'EmployeeOperations' was not found in appsettings.json.");

builder.Services.AddDbContext<EmployeeOperationsDbContext>(options =>
	options.UseSqlServer(connectionString));

builder.Services.AddScoped<IEmployeeRepository, EfEmployeeRepository>();
builder.Services.AddScoped<EmployeeDirectoryService>();


var app = builder.Build();

// Simple application status endpoint.
app.MapGet("/", () => Results.Ok(new
{
	application = "Employee Operations EF Core Lab 7",
	framework = ".NET 10",
	status = "Running"
}));

// Connectivity check: does not create or migrate the database.
app.MapGet("/health/database", async (EmployeeOperationsDbContext db) =>
{
	var canConnect = await db.Database.CanConnectAsync();

	return canConnect
		? Results.Ok(new { database = "EmployeeOperationsLab", connected = true })
		: Results.Problem(
			title: "Database connection failed",
			detail: "Check the SQL Server instance, database name, and authentication.");
});

// List departments.
app.MapGet("/api/departments", async (EmployeeOperationsDbContext db) =>
{
	var departments = await db.Departments
		.AsNoTracking()
		.OrderBy(d => d.DepartmentName)
		.Select(d => new
		{
			d.DepartmentId,
			d.DepartmentName,
			d.Location
		})
		.ToListAsync();

	return Results.Ok(departments);
});

// List employees with department names.
app.MapGet("/api/employees", async (EmployeeOperationsDbContext db) =>
{
	var employees = await db.Employees
		.AsNoTracking()
		.OrderBy(e => e.EmployeeId)
		.Select(e => new
		{
			e.EmployeeId,
			e.EmployeeNumber,
			e.FirstName,
			e.LastName,
			e.Email,
			e.EmploymentStatus,
			e.Salary,
			DepartmentName = e.Department == null
				? null
				: e.Department.DepartmentName
		})
		.ToListAsync();

	return Results.Ok(employees);
});

// Get one employee by ID, including its department name.
app.MapGet("/api/employees/{id:int}", async (
	int id,
	EmployeeOperationsDbContext db) =>
{
	var employee = await db.Employees
		.AsNoTracking()
		.Where(e => e.EmployeeId == id)
		.Select(e => new
		{
			e.EmployeeId,
			e.EmployeeNumber,
			e.FirstName,
			e.LastName,
			e.Email,
			e.HireDate,
			e.Salary,
			e.EmploymentStatus,
			e.DepartmentId,
			DepartmentName = e.Department == null
				? null
				: e.Department.DepartmentName
		})
		.FirstOrDefaultAsync();

	return employee is null
		? Results.NotFound(new { message = $"Employee {id} was not found." })
		: Results.Ok(employee);
});

// GET /api/lab8/employees-with-department
app.MapGet("/api/lab8/employees-with-department", async (EmployeeOperationsDbContext db) =>
{
	var entities = await db.Employees
		.AsNoTracking()
		.Include(e => e.Department)
		.OrderBy(e => e.EmployeeId)
		.ToListAsync();

	var employees = entities.Select(e => new
	{
		e.EmployeeId,
		e.FirstName,
		e.LastName,
		Department = e.Department == null ? null : new
		{
			e.Department.DepartmentId,
			e.Department.DepartmentName,
			e.Department.Location
		}
	});

	return Results.Ok(employees);
});

// GET /api/lab8/employees-with-projects
app.MapGet("/api/lab8/employees-with-projects", async (EmployeeOperationsDbContext db) =>
{
	var entities = await db.Employees
		.AsNoTracking()
		.Include(e => e.Department)
		.Include(e => e.EmployeeProjects)
			.ThenInclude(ep => ep.Project)
		.OrderBy(e => e.EmployeeId)
		.ToListAsync();

	var employees = entities.Select(e => new
	{
		e.EmployeeId,
		FullName = e.FirstName + " " + e.LastName,
		DepartmentName = e.Department == null ? null : e.Department.DepartmentName,
		Projects = e.EmployeeProjects.Select(ep => new
		{
			ep.ProjectId,
			ep.Project.ProjectCode,
			ep.Project.ProjectName,
			ep.Project.ProjectStatus,
			ep.AssignedRole,
			ep.AllocationPercent
		})
	});

	return Results.Ok(employees);
});


// GET /api/lab8/projects-with-team
app.MapGet("/api/lab8/projects-with-team", async (EmployeeOperationsDbContext db) =>
{
	var entities = await db.Projects
		.AsNoTracking()
		.Include(p => p.EmployeeProjects)
			.ThenInclude(ep => ep.Employee)
		.OrderBy(p => p.ProjectName)
		.ToListAsync();

	var projects = entities.Select(p => new
	{
		p.ProjectId,
		p.ProjectCode,
		p.ProjectName,
		p.ProjectStatus,
		Team = p.EmployeeProjects.Select(ep => new
		{
			ep.Employee.EmployeeId,
			ep.Employee.FirstName,
			ep.Employee.LastName,
			ep.AssignedRole,
			ep.AllocationPercent
		})
	});

	return Results.Ok(projects);
});

// GET /api/lab8/employees/{id:int}/explicit-department
app.MapGet("/api/lab8/employees/{id:int}/explicit-department", async (
	int id,
	EmployeeOperationsDbContext db) =>
{
	var employee = await db.Employees
		.SingleOrDefaultAsync(e => e.EmployeeId == id);

	if (employee is null)
		return Results.NotFound(new { message = $"Employee {id} was not found." });

	await db.Entry(employee)
		.Reference(e => e.Department)
		.LoadAsync();

	return Results.Ok(new
	{
		employee.EmployeeId,
		employee.FirstName,
		employee.LastName,
		Department = employee.Department is null ? null : new
		{
			employee.Department.DepartmentId,
			employee.Department.DepartmentName
		}
	});
});

// GET /api/lab8/n-plus-one-fixed
app.MapGet("/api/lab8/n-plus-one-fixed", async (EmployeeOperationsDbContext db) =>
{
	var results = await db.Employees
		.AsNoTracking()
		.OrderBy(e => e.EmployeeId)
		.Select(e => new
		{
			e.EmployeeId,
			e.FirstName,
			e.LastName,
			DepartmentName = e.Department == null ? null : e.Department.DepartmentName
		})
		.ToListAsync();

	return Results.Ok(results);
});

// LAB 9: Querying, filtering, sorting, projection, and pagination.
// Add this block before the existing app.Run(); statement.

// Example:
// GET /api/lab9/employees?q=an&status=Active&page=1&pageSize=10&sortBy=lastName
app.MapGet("/api/lab9/employees", async (
	EmployeeOperationsDbContext db,
	string? q,
	int? departmentId,
	string? status,
	decimal? minSalary,
	decimal? maxSalary,
	string? sortBy,
	bool descending = false,
	int page = 1,
	int pageSize = 10) =>
{
	// Validate request values before building the database query.
	if (minSalary is not null && minSalary < 0)
	{
		return Results.BadRequest(new { message = "minSalary cannot be negative." });
	}

	if (maxSalary is not null && maxSalary < 0)
	{
		return Results.BadRequest(new { message = "maxSalary cannot be negative." });
	}

	if (minSalary is not null && maxSalary is not null
		&& minSalary > maxSalary)
	{
		return Results.BadRequest(new
		{
			message = "minSalary cannot be greater than maxSalary."
		});
	}

	// Prevent invalid or excessive page requests.
	page = Math.Max(page, 1);
	pageSize = Math.Clamp(pageSize, 1, 100);

	IQueryable<Employee> query = db.Employees.AsNoTracking();

	// Optional text filter across employee name, number, and email.
	if (!string.IsNullOrWhiteSpace(q))
	{
		var term = $"%{q.Trim()}%";

		query = query.Where(e =>
			EF.Functions.Like(e.FirstName, term) ||
			EF.Functions.Like(e.LastName, term) ||
			EF.Functions.Like(e.EmployeeNumber, term) ||
			EF.Functions.Like(e.Email, term));
	}

	if (departmentId.HasValue)
	{
		query = query.Where(e => e.DepartmentId == departmentId.Value);
	}

	if (!string.IsNullOrWhiteSpace(status))
	{
		query = query.Where(e => e.EmploymentStatus == status.Trim());
	}

	if (minSalary.HasValue)
	{
		query = query.Where(e => e.Salary >= minSalary.Value);
	}

	if (maxSalary.HasValue)
	{
		query = query.Where(e => e.Salary <= maxSalary.Value);
	}

	// Count the filtered rows before Skip/Take so clients know total matches.
	var totalCount = await query.CountAsync();

	// Allow-list sort fields rather than accepting arbitrary property names.
	var normalizedSort = (sortBy ?? "employeeId").Trim().ToLowerInvariant();

	query = (normalizedSort, descending) switch
	{
		("firstname", false) => query.OrderBy(e => e.FirstName).ThenBy(e => e.EmployeeId),
		("firstname", true) => query.OrderByDescending(e => e.FirstName).ThenBy(e => e.EmployeeId),
		("lastname", false) => query.OrderBy(e => e.LastName).ThenBy(e => e.EmployeeId),
		("lastname", true) => query.OrderByDescending(e => e.LastName).ThenBy(e => e.EmployeeId),
		("salary", false) => query.OrderBy(e => e.Salary).ThenBy(e => e.EmployeeId),
		("salary", true) => query.OrderByDescending(e => e.Salary).ThenBy(e => e.EmployeeId),
		("hiredate", false) => query.OrderBy(e => e.HireDate).ThenBy(e => e.EmployeeId),
		("hiredate", true) => query.OrderByDescending(e => e.HireDate).ThenBy(e => e.EmployeeId),
		("employeeid", true) => query.OrderByDescending(e => e.EmployeeId),
		("employeeid", false) => query.OrderBy(e => e.EmployeeId),
		_ => query.OrderBy(e => e.EmployeeId)
	};

	var items = await query
		.Skip((page - 1) * pageSize)
		.Take(pageSize)
		.Select(e => new
		{
			e.EmployeeId,
			e.EmployeeNumber,
			FullName = e.FirstName + " " + e.LastName,
			e.Email,
			e.HireDate,
			e.Salary,
			e.EmploymentStatus,
			e.DepartmentId,
			DepartmentName = e.Department == null
				? null
				: e.Department.DepartmentName
		})
		.ToListAsync();

	return Results.Ok(new
	{
		page,
		pageSize,
		totalCount,
		totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
		items
	});
});

// Aggregate example: SQL Server performs the calculations.
app.MapGet("/api/lab9/employees/summary", async (
	EmployeeOperationsDbContext db) =>
{
	var summary = await db.Employees
		.AsNoTracking()
		.GroupBy(e => 1)
		.Select(g => new
		{
			EmployeeCount = g.Count(),
			AverageSalary = g.Average(e => (decimal?)e.Salary),
			MinimumSalary = g.Min(e => (decimal?)e.Salary),
			MaximumSalary = g.Max(e => (decimal?)e.Salary)
		})
		.FirstOrDefaultAsync();

	return Results.Ok(summary ?? new
	{
		EmployeeCount = 0,
		AverageSalary = (decimal?)null,
		MinimumSalary = (decimal?)null,
		MaximumSalary = (decimal?)null
	});
});

// Grouped report: employee count by department.
app.MapGet("/api/lab9/reports/by-department", async (
	EmployeeOperationsDbContext db) =>
{
	var report = await db.Employees
		.AsNoTracking()
		.GroupBy(e => new
		{
			e.DepartmentId,
			DepartmentName = e.Department == null
				? "(No department)"
				: e.Department.DepartmentName
		})
		.Select(g => new
		{
			g.Key.DepartmentId,
			g.Key.DepartmentName,
			EmployeeCount = g.Count(),
			AverageSalary = g.Average(e => (decimal?)e.Salary)
		})
		.OrderBy(x => x.DepartmentName)
		.ToListAsync();

	return Results.Ok(report);
});

// LAB 10: Insert, update, delete, and change-tracking examples.

// Create employee.
// POST /api/lab10/employees
app.MapPost("/api/lab10/employees", async (
	EmployeeCreateRequest request,
	EmployeeOperationsDbContext db) =>
{
	var validationErrors = new Dictionary<string, string[]>();

	if (string.IsNullOrWhiteSpace(request.EmployeeNumber))
		validationErrors["employeeNumber"] = ["Employee number is required."];
	if (string.IsNullOrWhiteSpace(request.FirstName))
		validationErrors["firstName"] = ["First name is required."];
	if (string.IsNullOrWhiteSpace(request.LastName))
		validationErrors["lastName"] = ["Last name is required."];
	if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
		validationErrors["email"] = ["A valid email address is required."];
	if (request.Salary < 0)
		validationErrors["salary"] = ["Salary cannot be negative."];
	if (request.DepartmentId <= 0)
		validationErrors["departmentId"] = ["A valid department ID is required."];

	var validStatuses = new[] { "Active", "OnLeave", "Inactive" };
	if (!validStatuses.Contains(request.EmploymentStatus))
		validationErrors["employmentStatus"] = ["Status must be Active, OnLeave, or Inactive."];

	if (validationErrors.Count > 0)
		return Results.ValidationProblem(validationErrors);

	if (!await db.Departments.AnyAsync(d => d.DepartmentId == request.DepartmentId))
		return Results.BadRequest(new { message = "The specified department does not exist." });

	if (await db.Employees.AnyAsync(e => e.EmployeeNumber == request.EmployeeNumber))
		return Results.Conflict(new { message = "Employee number already exists." });

	if (await db.Employees.AnyAsync(e => e.Email == request.Email))
		return Results.Conflict(new { message = "Email already exists." });

	var employee = new Employee
	{
		EmployeeNumber = request.EmployeeNumber.Trim(),
		FirstName = request.FirstName.Trim(),
		LastName = request.LastName.Trim(),
		Email = request.Email.Trim(),
		HireDate = request.HireDate,
		Salary = request.Salary,
		EmploymentStatus = request.EmploymentStatus,
		DepartmentId = request.DepartmentId
	};

	db.Employees.Add(employee); // Entity state becomes Added.
	await db.SaveChangesAsync(); // INSERT is sent to SQL Server.

	return Results.Created($"/api/lab10/employees/{employee.EmployeeId}", new
	{
		employee.EmployeeId,
		employee.EmployeeNumber,
		employee.FirstName,
		employee.LastName,
		employee.Email,
		employee.HireDate,
		employee.Salary,
		employee.EmploymentStatus,
		employee.DepartmentId
	});
});

// Update an existing employee.
// PUT /api/lab10/employees/1
app.MapPut("/api/lab10/employees/{id:int}", async (
	int id,
	EmployeeUpdateRequest request,
	EmployeeOperationsDbContext db) =>
{
	var validationErrors = new Dictionary<string, string[]>();

	if (string.IsNullOrWhiteSpace(request.FirstName))
		validationErrors["firstName"] = ["First name is required."];
	if (string.IsNullOrWhiteSpace(request.LastName))
		validationErrors["lastName"] = ["Last name is required."];
	if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
		validationErrors["email"] = ["A valid email address is required."];
	if (request.Salary < 0)
		validationErrors["salary"] = ["Salary cannot be negative."];

	var validStatuses = new[] { "Active", "OnLeave", "Inactive" };
	if (!validStatuses.Contains(request.EmploymentStatus))
		validationErrors["employmentStatus"] = ["Status must be Active, OnLeave, or Inactive."];

	if (validationErrors.Count > 0)
		return Results.ValidationProblem(validationErrors);

	var employee = await db.Employees
		.FirstOrDefaultAsync(e => e.EmployeeId == id);

	if (employee is null)
		return Results.NotFound(new { message = $"Employee {id} was not found." });

	if (!await db.Departments.AnyAsync(d => d.DepartmentId == request.DepartmentId))
		return Results.BadRequest(new { message = "The specified department does not exist." });

	if (await db.Employees.AnyAsync(e => e.Email == request.Email &&
										 e.EmployeeId != id))
		return Results.Conflict(new { message = "Email is already used by another employee." });

	// The entity is tracked. Assign only approved writable properties.
	employee.FirstName = request.FirstName.Trim();
	employee.LastName = request.LastName.Trim();
	employee.Email = request.Email.Trim();
	employee.HireDate = request.HireDate;
	employee.Salary = request.Salary;
	employee.EmploymentStatus = request.EmploymentStatus;
	employee.DepartmentId = request.DepartmentId;

	await db.SaveChangesAsync(); // EF Core detects changes and sends UPDATE.

	return Results.Ok(new
	{
		employee.EmployeeId,
		employee.EmployeeNumber,
		employee.FirstName,
		employee.LastName,
		employee.Email,
		employee.HireDate,
		employee.Salary,
		employee.EmploymentStatus,
		employee.DepartmentId
	});
});

// Delete an employee.
// DELETE /api/lab10/employees/1
app.MapDelete("/api/lab10/employees/{id:int}", async (
	int id,
	EmployeeOperationsDbContext db) =>
{
	var employee = await db.Employees
		.FirstOrDefaultAsync(e => e.EmployeeId == id);

	if (employee is null)
		return Results.NotFound(new { message = $"Employee {id} was not found." });

	// If related rows restrict deletion, SQL Server may reject SaveChangesAsync().
	db.Employees.Remove(employee); // Entity state becomes Deleted.
	await db.SaveChangesAsync();

	return Results.NoContent();
});

// Inspect tracked state. This endpoint is for learning/debugging only.
// GET /api/lab10/employees/1/tracking
app.MapGet("/api/lab10/employees/{id:int}/tracking", async (
	int id,
	EmployeeOperationsDbContext db) =>
{
	var employee = await db.Employees
		.FirstOrDefaultAsync(e => e.EmployeeId == id);

	if (employee is null)
		return Results.NotFound();

	var entry = db.Entry(employee);

	return Results.Ok(new
	{
		employee.EmployeeId,
		EntityState = entry.State.ToString(),
		PropertyStates = entry.Properties.ToDictionary(
			property => property.Metadata.Name,
			property => property.IsModified ? "Modified" : "NotModified")
	});
});

// LAB 11: Atomic employee + project assignment operation.
// POST /api/lab11/employee-project-assignment
// Set simulateFailure=true to demonstrate rollback. Do not use that flag in production.
app.MapPost("/api/lab11/employee-project-assignment", async (
	EmployeeOperationsDbContext db,
	CreateEmployeeAssignmentRequest request) =>
{
	if (string.IsNullOrWhiteSpace(request.EmployeeNumber) ||
		string.IsNullOrWhiteSpace(request.FirstName) ||
		string.IsNullOrWhiteSpace(request.LastName) ||
		string.IsNullOrWhiteSpace(request.Email))
	{
		return Results.BadRequest(new { message = "Employee number, first name, last name, and email are required." });
	}

	if (request.Salary < 0 || request.AllocationPercent is < 1 or > 100)
	{
		return Results.BadRequest(new { message = "Salary must be non-negative and allocationPercent must be between 1 and 100." });
	}

	if (!await db.Departments.AnyAsync(d => d.DepartmentId == request.DepartmentId))
		return Results.BadRequest(new { message = "Department does not exist." });

	if (!await db.Projects.AnyAsync(p => p.ProjectId == request.ProjectId))
		return Results.BadRequest(new { message = "Project does not exist." });

	if (await db.Employees.AnyAsync(e => e.EmployeeNumber == request.EmployeeNumber))
		return Results.Conflict(new { message = "Employee number already exists." });

	if (await db.Employees.AnyAsync(e => e.Email == request.Email))
		return Results.Conflict(new { message = "Email already exists." });

	await using var transaction = await db.Database.BeginTransactionAsync();

	try
	{
		var employee = new Employee
		{
			EmployeeNumber = request.EmployeeNumber.Trim(),
			FirstName = request.FirstName.Trim(),
			LastName = request.LastName.Trim(),
			Email = request.Email.Trim(),
			HireDate = request.HireDate,
			Salary = request.Salary,
			EmploymentStatus = "Active",
			DepartmentId = request.DepartmentId
		};

		db.Employees.Add(employee);
		await db.SaveChangesAsync(); // First save, still uncommitted.

		var assignment = new EmployeeProject
		{
			EmployeeId = employee.EmployeeId,
			ProjectId = request.ProjectId,
			AssignedRole = request.AssignedRole.Trim(),
			AllocationPercent = request.AllocationPercent,
			AssignedAtUtc = DateTime.UtcNow
		};

		db.EmployeeProjects.Add(assignment);
		await db.SaveChangesAsync(); // Second save, same explicit transaction.

		if (request.SimulateFailure)
		{
			await transaction.RollbackAsync();
			db.ChangeTracker.Clear();

			return Results.Conflict(new
			{
				message = "Simulated failure: transaction rolled back.",
				employeeIdGeneratedDuringAttempt = employee.EmployeeId,
				verify = "Query Employees and EmployeeProjects; neither new row should exist."
			});
		}

		await transaction.CommitAsync();

		return Results.Created(
			$"/api/employees/{employee.EmployeeId}",
			new
			{
				employee.EmployeeId,
				employee.EmployeeNumber,
				employee.FirstName,
				employee.LastName,
				employee.Email,
				assignment.ProjectId,
				assignment.AssignedRole,
				assignment.AllocationPercent,
				message = "Employee and project assignment committed together."
			});
	}
	catch (Exception)
	{
		await transaction.RollbackAsync();
		db.ChangeTracker.Clear();

		// Do not expose raw database exception details to the API caller.
		return Results.Problem(
			title: "The employee assignment could not be completed.",
			statusCode: StatusCodes.Status500InternalServerError);
	}
});

// LAB 12 — EF Core ORM completion endpoints
app.MapGet("/api/lab12/model", (EmployeeOperationsDbContext db) =>
{
	var entitySummaries = db.Model.GetEntityTypes()
		.Where(entityType => entityType.GetTableName() is not null)
		.OrderBy(entityType => entityType.ClrType.Name)
		.Select(entityType =>
		{
			var tableName = entityType.GetTableName()!;
			var schema = entityType.GetSchema();
			var primaryKey = entityType.FindPrimaryKey();

			var keyProperties = primaryKey?.Properties
				.Select(property => property.Name)
				.ToArray() ?? Array.Empty<string>();

			var properties = entityType.GetProperties()
				.Select(property => property.Name)
				.OrderBy(name => name)
				.ToArray();

			return new Lab12EntitySummary(
				entityType.ClrType.Name,
				tableName,
				schema,
				keyProperties,
				properties);
		})
		.ToArray();

	return Results.Ok(new Lab12ModelSummaryResponse(
		db.GetType().Name,
		entitySummaries));
});

app.MapGet("/api/lab12/sql-preview/employees", (EmployeeOperationsDbContext db) =>
{
	var query = db.Employees
		.AsNoTracking()
		.Where(employee => employee.EmploymentStatus == "Active")
		.OrderBy(employee => employee.LastName)
		.ThenBy(employee => employee.FirstName)
		.Select(employee => new
		{
			employee.EmployeeId,
			employee.EmployeeNumber,
			employee.FirstName,
			employee.LastName,
			employee.Email,
			employee.DepartmentId
		});

	// ToQueryString() is for inspection/debugging; it does not execute the query.
	return Results.Text(query.ToQueryString(), "text/plain");
});

app.MapGet("/api/lab12/employees/active-summary", async (
	EmployeeOperationsDbContext db,
	CancellationToken cancellationToken) =>
{
	var employees = await db.Employees
		.AsNoTracking()
		.Where(employee => employee.EmploymentStatus == "Active")
		.OrderBy(employee => employee.LastName)
		.ThenBy(employee => employee.FirstName)
		.Select(employee => new
		{
			employee.EmployeeId,
			employee.EmployeeNumber,
			FullName = employee.FirstName + " " + employee.LastName,
			employee.Email,
			employee.DepartmentId
		})
		.ToListAsync(cancellationToken);

	return Results.Ok(employees);
});

app.MapGet("/api/lab12/departments-with-employees", async (
	EmployeeOperationsDbContext db,
	CancellationToken cancellationToken) =>
{
	// Projection keeps the response shape explicit and avoids returning entity graphs.
	var departments = await db.Departments
		.AsNoTracking()
		.OrderBy(department => department.DepartmentName)
		.Select(department => new
		{
			department.DepartmentId,
			department.DepartmentName,
			department.Location,
			Employees = department.Employees
				.OrderBy(employee => employee.LastName)
				.ThenBy(employee => employee.FirstName)
				.Select(employee => new
				{
					employee.EmployeeId,
					employee.FirstName,
					employee.LastName,
					employee.Email
				})
				.ToList()
		})
		.ToListAsync(cancellationToken);

	return Results.Ok(departments);
});

app.MapGet("/api/lab12/employee-count-by-status", async (
	EmployeeOperationsDbContext db,
	CancellationToken cancellationToken) =>
{
	var report = await db.Employees
		.AsNoTracking()
		.GroupBy(employee => employee.EmploymentStatus)
		.Select(group => new
		{
			EmploymentStatus = group.Key,
			EmployeeCount = group.Count()
		})
		.OrderBy(item => item.EmploymentStatus)
		.ToListAsync(cancellationToken);

	return Results.Ok(report);
});

// LAB 14 — Repository Pattern endpoints

app.MapGet("/api/lab14/employees", async (
	EmployeeDirectoryService service,
	CancellationToken cancellationToken) =>
{
	var employees = await service.GetEmployeesAsync(cancellationToken);
	return Results.Ok(employees);
});

app.MapGet("/api/lab14/employees/search", async (
	string? term,
	EmployeeDirectoryService service,
	CancellationToken cancellationToken) =>
{
	var employees = await service.SearchEmployeesAsync(
		term, cancellationToken);
	return Results.Ok(employees);
});

app.MapGet("/api/lab14/employees/{id:int}", async (
	int id,
	EmployeeDirectoryService service,
	CancellationToken cancellationToken) =>
{
	if (id <= 0)
		return Results.BadRequest(
			new { message = "Employee ID must be positive." });

	var employee = await service.GetEmployeeAsync(id, cancellationToken);

	return employee is null
		? Results.NotFound()
		: Results.Ok(employee);
});
app.MapGet("/api/lab15/unit-of-work/scope-check",
	(IContextLifetimeProbe probe) =>
		Results.Ok(new
		{
			SameDbContextInstanceWithinScope =
				probe.SameInstanceForCurrentScope()
		}));
app.Run();
