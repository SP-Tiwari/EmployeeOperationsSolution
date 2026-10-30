namespace EmployeeOperationsSolution.EmployeeOperations.EFCoreLab.Contracts;

public sealed record Lab12EntitySummary(
	string EntityName,
	string TableName,
	string? Schema,
	IReadOnlyList<string> PrimaryKeyProperties,
	IReadOnlyList<string> Properties);

public sealed record Lab12ModelSummaryResponse(
	string ContextName,
	IReadOnlyList<Lab12EntitySummary> Entities);
