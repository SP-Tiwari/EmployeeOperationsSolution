namespace EmployeeOperationsSolution.EmployeeOperations.EFCoreLab.Contracts;

public sealed class CreateEmployeeAssignmentRequest
{
	public string EmployeeNumber { get; init; } = "";
	public string FirstName { get; init; } = "";
	public string LastName { get; init; } = "";
	public string Email { get; init; } = "";
	public DateTime HireDate { get; init; }
	public decimal Salary { get; init; }
	public int DepartmentId { get; init; }
	public int ProjectId { get; init; }
	public string AssignedRole { get; init; } = "Contributor";
	public int AllocationPercent { get; init; } = 50;
	public bool SimulateFailure { get; init; }
}
