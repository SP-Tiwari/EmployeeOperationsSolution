namespace EmployeeOperationsSolution.EmployeeOperations.EFCoreLab.Contracts;

public sealed class EmployeeCreateRequest
{
	public string EmployeeNumber { get; init; } = "";
	public string FirstName { get; init; } = "";
	public string LastName { get; init; } = "";
	public string Email { get; init; } = "";
	public DateTime HireDate { get; init; }
	public decimal Salary { get; init; }
	public string EmploymentStatus { get; init; } = "Active";
	public int DepartmentId { get; init; }
}
