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
	[Route("api/")]
	[ApiController]

	public class UserController : ControllerBase
	{
		private readonly IConfiguration _configuration;
        private readonly IMongoCollection<User> _usersCollection;

        public UserController(IConfiguration configuration)
		{
			_configuration = configuration;

            var mongoClient = new MongoClient(_configuration.GetConnectionString("UserDatabaseSettings:ConnectionString"));
            var database = mongoClient.GetDatabase(_configuration.GetConnectionString("UserDatabaseSettings:DatabaseName"));
            _usersCollection = database.GetCollection<User>("users");
        }

		[HttpPost("register")]
		public ActionResult<User> Register(UserReq request)
		{
            var usernameFilter = Builders<User>.Filter.Eq(u => u.Username, request.Username);
            var usernameUser = _usersCollection.Find(usernameFilter).FirstOrDefault();
            if (usernameUser != null)
            {
                return BadRequest("Username already exists.");
            }

            var emailFilter = Builders<User>.Filter.Eq(u => u.Email, request.Email);
            var emailUser = _usersCollection.Find(emailFilter).FirstOrDefault();
            if (emailUser != null)
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

			var mongoClient = new MongoClient(_configuration.GetConnectionString("UserDatabaseSettings:ConnectionString"));
            var database = mongoClient.GetDatabase(_configuration.GetConnectionString("UserDatabaseSettings:DatabaseName"));
            var usersCollection = database.GetCollection<User>("users");
            usersCollection.InsertOne(newUser);

            return Ok(newUser);
		}


		[HttpPost("login")]
		public ActionResult<User> Login(UserReq request)
		{
            var mongoClient = new MongoClient(_configuration.GetConnectionString("UserDatabaseSettings:ConnectionString"));
            var database = mongoClient.GetDatabase(_configuration.GetConnectionString("UserDatabaseSettings:DatabaseName"));
            var usersCollection = database.GetCollection<User>("users");

			var filter = Builders<User>.Filter.Eq(u => u.Username, request.Username);
			var user = usersCollection.Find(filter).FirstOrDefault();


            if (user.Username != request.Username || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
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

        [HttpGet("user/{id}"), Authorize(Roles = "User")]
        public ActionResult<User> GetUserInfo(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var user = _usersCollection.Find(filter).FirstOrDefault();

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
            var user = _usersCollection.Find(filter).FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            user.Username = request.Username;
            user.Email = request.Email;

            if (request.Password != null)
            {

            }

            _usersCollection.ReplaceOne(filter, user);

            return Ok(new { Username = user.Username, Email = user.Email });
        }
    }
}

