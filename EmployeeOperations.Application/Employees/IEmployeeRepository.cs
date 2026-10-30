namespace EmployeeOperations.Application.Employees;

public sealed record EmployeeListItem(
	int EmployeeId,
	string EmployeeNumber,
	string FirstName,
	string LastName,
	string Email,
	string EmploymentStatus,
	int DepartmentId,
	DateOnly? HireDate);

public interface IEmployeeRepository
{
	Task<IReadOnlyList<EmployeeListItem>> GetAllAsync(
		CancellationToken cancellationToken = default);

	Task<EmployeeListItem?> GetByIdAsync(
		int employeeId,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<EmployeeListItem>> SearchAsync(
		string? searchTerm,
		CancellationToken cancellationToken = default);
}
