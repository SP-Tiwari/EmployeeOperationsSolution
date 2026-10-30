namespace EmployeeOperations.Infrastructure.Models;

public class EmployeeProject
{
    public int EmployeeId { get; set; }
    public int ProjectId { get; set; }
    public string AssignedRole { get; set; } = string.Empty;
    public decimal AllocationPercent { get; set; }
    public DateTime AssignedAtUtc { get; set; }
 
    public Employee Employee { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
