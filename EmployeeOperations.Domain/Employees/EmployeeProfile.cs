namespace EmployeeOperations.Domain.Employees;

public sealed class EmployeeProfile
{
	public int Id { get; private set; }
	public string EmployeeNumber { get; private set; }
	public string FirstName { get; private set; }
	public string LastName { get; private set; }

	private EmployeeProfile(
		int id,
		string employeeNumber,
		string firstName,
		string lastName)
	{
		Id = id;
		EmployeeNumber = employeeNumber;
		FirstName = firstName;
		LastName = lastName;
	}

	public static EmployeeProfile Create(
		int id,
		string employeeNumber,
		string firstName,
		string lastName)
	{
		if (id < 0)
			throw new ArgumentOutOfRangeException(nameof(id));

		if (string.IsNullOrWhiteSpace(employeeNumber))
			throw new ArgumentException(
				"Employee number is required.", nameof(employeeNumber));

		if (string.IsNullOrWhiteSpace(firstName))
			throw new ArgumentException(
				"First name is required.", nameof(firstName));

		if (string.IsNullOrWhiteSpace(lastName))
			throw new ArgumentException(
				"Last name is required.", nameof(lastName));

		return new EmployeeProfile(
			id,
			employeeNumber.Trim(),
			firstName.Trim(),
			lastName.Trim());
	}

	public string FullName => $"{FirstName} {LastName}";
}
