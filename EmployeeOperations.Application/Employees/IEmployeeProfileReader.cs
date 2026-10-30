namespace EmployeeOperations.Application.Employees;

public interface IEmployeeProfileReader
{
	Task<IReadOnlyList<EmployeeProfileDto>> GetAllAsync(
		CancellationToken cancellationToken = default);

	Task<EmployeeProfileDto?> GetByIdAsync(
		int id,
		CancellationToken cancellationToken = default);
}
