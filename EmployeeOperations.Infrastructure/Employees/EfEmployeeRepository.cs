using EmployeeOperations.Application.Employees;
using EmployeeOperations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeOperations.Infrastructure.Employees;

public sealed class EfEmployeeRepository : IEmployeeRepository
{
	private readonly EmployeeOperationsDbContext _db;

	public EfEmployeeRepository(EmployeeOperationsDbContext db)
	{
		_db = db;
	}

	public async Task<IReadOnlyList<EmployeeListItem>> GetAllAsync(
		CancellationToken cancellationToken = default)
	{
		return await _db.Employees
			.AsNoTracking()
			.OrderBy(employee => employee.LastName)
			.ThenBy(employee => employee.FirstName)
			.Select(employee => new EmployeeListItem(
				employee.EmployeeId,
				employee.EmployeeNumber,
				employee.FirstName,
				employee.LastName,
				employee.Email,
				employee.EmploymentStatus,
				employee.DepartmentId,
				DateOnly.FromDateTime(employee.HireDate)))
			.ToListAsync(cancellationToken);
	}

	public async Task<EmployeeListItem?> GetByIdAsync(
		int employeeId,
		CancellationToken cancellationToken = default)
	{
		return await _db.Employees
			.AsNoTracking()
			.Where(employee => employee.EmployeeId == employeeId)
			.Select(employee => new EmployeeListItem(
				employee.EmployeeId,
				employee.EmployeeNumber,
				employee.FirstName,
				employee.LastName,
				employee.Email,
				employee.EmploymentStatus,
				employee.DepartmentId,
				DateOnly.FromDateTime(employee.HireDate)))
			.SingleOrDefaultAsync(cancellationToken);
	}

	public async Task<IReadOnlyList<EmployeeListItem>> SearchAsync(
		string? searchTerm,
		CancellationToken cancellationToken = default)
	{
		var query = _db.Employees.AsNoTracking();

		if (!string.IsNullOrWhiteSpace(searchTerm))
		{
			var term = searchTerm.Trim();

			query = query.Where(employee =>
				employee.EmployeeNumber.Contains(term) ||
				employee.FirstName.Contains(term) ||
				employee.LastName.Contains(term) ||
				employee.Email.Contains(term));
		}

		return await query
			.OrderBy(employee => employee.LastName)
			.ThenBy(employee => employee.FirstName)
			.Select(employee => new EmployeeListItem(
				employee.EmployeeId,
				employee.EmployeeNumber,
				employee.FirstName,
				employee.LastName,
				employee.Email,
				employee.EmploymentStatus,
				employee.DepartmentId,
				DateOnly.FromDateTime(employee.HireDate)))
			.ToListAsync(cancellationToken);
	}
}
