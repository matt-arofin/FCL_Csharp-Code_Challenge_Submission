//using Csharp_Code_Challenge_Submission.Services;
using Csharp_Code_Challenge_Submission.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MongoDB.Driver;

namespace Csharp_Code_Challenge_Submission.Controllers
{
	[Route("api/[controller]")]
	[ApiController]

	public class AuthController : ControllerBase
	{
		public static User user = new User();
		private readonly IConfiguration _configuration;

		public AuthController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		[HttpPost("register")]
		public ActionResult<User> Register(UserReq request)
		{
			string passwordHash
				= BCrypt.Net.BCrypt.HashPassword(request.Password);

			user.Username = request.Username;
			user.PasswordHash = passwordHash;
			user.Email = request.Email;

			return Ok(user);
		}

		[HttpPost("login")]
		public ActionResult<User> Login(UserReq request)
		{
			if(user.Username != request.Username || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
			{
				return BadRequest("Invalid credentials");
			}

			return Ok(user);
		}

		private string CreateToken(User user)
		{
			List<Claim> claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, user.Username),
				new Claim(ClaimTypes.Role, "User")
            };

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
				_configuration.GetSection("AppSettings:Token").Value!));

			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

			var token = new JwtSecurityToken(
					claims: claims,
					expires: DateTime.Now.AddHours(3),
					signingCredentials: credentials
				);

			var jwt = new JwtSecurityTokenHandler().WriteToken(token);

			return jwt;
		}
	}
}

