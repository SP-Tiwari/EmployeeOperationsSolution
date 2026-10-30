using EmployeeOperations.Infrastructure.Data;

namespace EmployeeOperations.Infrastructure.Persistence;

public sealed class ContextLifetimeProbe : IContextLifetimeProbe
{
	private readonly EmployeeOperationsDbContext _first;
	private readonly EmployeeOperationsDbContext _second;

	public ContextLifetimeProbe(
		EmployeeOperationsDbContext first,
		EmployeeOperationsDbContext second)
	{
		_first = first;
		_second = second;
	}

	public bool SameInstanceForCurrentScope()
		=> ReferenceEquals(_first, _second);
}
