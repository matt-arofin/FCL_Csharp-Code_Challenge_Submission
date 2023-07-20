using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Csharp_Code_Challenge_Submission.Models
{
	public class UserReq
	{
		[Required(ErrorMessage = "Username is required")]
		[MinLength(8, ErrorMessage = "Username must be at least 8 characters long")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
		[PasswordFormat(ErrorMessage = "Password must have at least one uppercase, one lowercase, one symbol, one digit")]
        public required string Password { get; set; }

		[EmailAddress(ErrorMessage = "Invalid email format")]
		public string? Email { get; set; }

		// Custom password format validation
		private class PasswordFormatAttribute : ValidationAttribute
		{
            public override bool IsValid(object? value)
            {
				var password = value as string;
				if (string.IsNullOrEmpty(password))
				{
					return false;
				}

				var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$");
				return regex.IsMatch(password);
            }
        }
	}
}