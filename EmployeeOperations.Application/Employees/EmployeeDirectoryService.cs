namespace EmployeeOperations.Application.Employees;

public sealed class EmployeeDirectoryService
{
	private readonly IEmployeeRepository _repository;

	public EmployeeDirectoryService(IEmployeeRepository repository)
	{
		_repository = repository;
	}

	public Task<IReadOnlyList<EmployeeListItem>> GetEmployeesAsync(
		CancellationToken cancellationToken = default)
		=> _repository.GetAllAsync(cancellationToken);

	public Task<EmployeeListItem?> GetEmployeeAsync(
		int employeeId,
		CancellationToken cancellationToken = default)
		=> _repository.GetByIdAsync(employeeId, cancellationToken);

	public Task<IReadOnlyList<EmployeeListItem>> SearchEmployeesAsync(
		string? term,
		CancellationToken cancellationToken = default)
		=> _repository.SearchAsync(term, cancellationToken);
}
