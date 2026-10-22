namespace EmployeeOperationsMvc.Attributes
{
	[AttributeUsage(
		AttributeTargets.Class | AttributeTargets.Method,
		AllowMultiple = false,
		Inherited = true)]
	public sealed class IpRestrictionRequiredAttribute : Attribute
	{
	}
}
