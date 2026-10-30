namespace EmployeeOperations.Infrastructure.Models;

public class Project
{
	public int ProjectId { get; set; }
	public string ProjectCode { get; set; } = string.Empty;
	public string ProjectName { get; set; } = string.Empty;
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public string ProjectStatus { get; set; } = "Planned";

	public ICollection<EmployeeProject> EmployeeProjects { get; set; }
		= new List<EmployeeProject>();
}
