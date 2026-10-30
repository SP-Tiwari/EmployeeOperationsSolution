namespace EmployeeOperations.Application.Employees;

public sealed record EmployeeProfileDto(
	int Id,
	string EmployeeNumber,
	string FirstName,
	string LastName,
	string FullName);
