namespace EmployeeOperations.Infrastructure.Persistence;

public interface IContextLifetimeProbe
{
	bool SameInstanceForCurrentScope();
}
