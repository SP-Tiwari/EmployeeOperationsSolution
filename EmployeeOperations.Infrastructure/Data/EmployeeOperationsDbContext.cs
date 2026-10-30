using EmployeeOperations.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeOperations.Infrastructure.Data;

public class EmployeeOperationsDbContext : DbContext
{
	public EmployeeOperationsDbContext(
		DbContextOptions<EmployeeOperationsDbContext> options)
		: base(options)
	{
		Employees = Set<Employee>();
		Departments = Set<Department>();
		Projects = Set<Project>();
		EmployeeProjects = Set<EmployeeProject>();
	}

	public DbSet<Employee> Employees { get; }
	public DbSet<Department> Departments { get; }
	public DbSet<Project> Projects { get; }
	public DbSet<EmployeeProject> EmployeeProjects { get; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Department>(entity =>
		{
			entity.ToTable("Departments");
			entity.HasKey(d => d.DepartmentId);
			entity.Property(d => d.DepartmentName).IsRequired();
		});

		modelBuilder.Entity<Employee>(entity =>
		{
			entity.ToTable("Employees");
			entity.HasKey(e => e.EmployeeId);
			entity.Property(e => e.EmployeeNumber).IsRequired();
			entity.Property(e => e.FirstName).IsRequired();
			entity.Property(e => e.LastName).IsRequired();
			entity.Property(e => e.Email).IsRequired();
			entity.Property(e => e.Salary).HasPrecision(12, 2);
			entity.Property(e => e.EmploymentStatus).IsRequired();

			entity.HasOne(e => e.Department)
				.WithMany(d => d.Employees)
				.HasForeignKey(e => e.DepartmentId);
		});

		modelBuilder.Entity<Project>(entity =>
		{
			entity.ToTable("Projects");
			entity.HasKey(p => p.ProjectId);
			entity.Property(p => p.ProjectCode).IsRequired();
			entity.Property(p => p.ProjectName).IsRequired();
			entity.Property(p => p.ProjectStatus).IsRequired();
		});

		modelBuilder.Entity<EmployeeProject>(entity =>
		{
			entity.ToTable("EmployeeProjects");
			entity.HasKey(ep => new { ep.EmployeeId, ep.ProjectId });
			entity.Property(ep => ep.AssignedRole).IsRequired();
			entity.Property(ep => ep.AllocationPercent).HasPrecision(5, 2);

			entity.HasOne(ep => ep.Employee)
				.WithMany(e => e.EmployeeProjects)
				.HasForeignKey(ep => ep.EmployeeId);

			entity.HasOne(ep => ep.Project)
				.WithMany(p => p.EmployeeProjects)
				.HasForeignKey(ep => ep.ProjectId);
		});
	}
}
