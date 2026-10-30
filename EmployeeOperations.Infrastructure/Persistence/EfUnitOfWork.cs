using EmployeeOperations.Application.Abstractions;
using EmployeeOperations.Infrastructure.Data;

namespace EmployeeOperations.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
	private readonly EmployeeOperationsDbContext _dbContext;

	public EfUnitOfWork(EmployeeOperationsDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public Task<int> SaveChangesAsync(
		CancellationToken cancellationToken = default)
	{
		return _dbContext.SaveChangesAsync(cancellationToken);
	}
}
