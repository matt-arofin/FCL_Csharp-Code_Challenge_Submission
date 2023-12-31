﻿using Csharp_Code_Challenge_Submission.Models;
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
	[Route("api/")]
	[ApiController]

	public class UserController : ControllerBase
	{
		private readonly IConfiguration _configuration;
        private readonly UserRepository _userRepository;

        public UserController(IConfiguration configuration, UserRepository userRepository)
		{
			_configuration = configuration;
            _userRepository = userRepository;
        }

		[HttpPost("register")]
		public ActionResult<User> Register(UserReq request)
		{
            var usernameFilter = Builders<User>.Filter.Eq(u => u.Username, request.Username);
            var duplicateUser = _userRepository.FindUserByUsername(request.Username);
            if (duplicateUser != null)
            {
                return BadRequest("Username already exists.");
            }

            var emailFilter = Builders<User>.Filter.Eq(u => u.Email, request.Email);
            var duplicateEmail = _userRepository.FindUserByEmail(request.Email);
            if (duplicateEmail != null)
            {
                return BadRequest("Email already exists.");
            }

            string passwordHash
				= BCrypt.Net.BCrypt.HashPassword(request.Password);

			var newUser = new User
			{
				Username = request.Username,
				PasswordHash = passwordHash,
				Email = request.Email
			};

            _userRepository.AddUser(newUser);

            return Ok(new { message = "User successfully registered", newUser.Username, newUser.Email });
		}


		[HttpPost("login")]
		public ActionResult<User> Login(UserReq request)
		{
			var filter = Builders<User>.Filter.Eq(u => u.Username, request.Username);
			var user = _userRepository.FindUserByUsername(request.Username);


            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
			{
				return BadRequest("Invalid credentials");
			}

			return Ok(new { Id = user.Id, Token = CreateToken(user)});
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

        [HttpGet("user/{id}"), Authorize(Roles = "User")]
        public ActionResult<User> GetUserInfo(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var user = _userRepository.FindUserById(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new { Username = user.Username, Email = user.Email });
        }


        [HttpPut("user/{id}"), Authorize(Roles = "User")]
        public ActionResult<User> UpdateUserInfo(string id, UserReq request)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var user = _userRepository.FindUserById(id);

            bool isUpdated = false;

            if (!string.IsNullOrEmpty(request.Username))
            {
                user.Username = request.Username;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                user.Email = request.Email;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(request.Password))
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                user.PasswordHash = passwordHash;
                isUpdated = true;
            }

            if (isUpdated)
            {
                _userRepository.UpdateUser(user);
                return Ok(new { Message = "Update successful.", UpdatedField = request.Username ?? request.Email });
            }
            else
            {
                return BadRequest("No fields to update");
            }            
        }
    }
}

