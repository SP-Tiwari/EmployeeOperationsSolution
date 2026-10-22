// File: Models/LoginViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace EmployeeOperationsMvc.Models
{
	public sealed class LoginViewModel
	{
		[Required]
		[Display(Name = "Username")]
		public string Username { get; set; } = string.Empty;

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; } = string.Empty;

		[Display(Name = "Remember me")]
		public bool RememberMe { get; set; }
	}
}
