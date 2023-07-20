using Xunit;
using Microsoft.AspNetCore.Mvc;
using Csharp_Code_Challenge_Submission.Controllers;
using Csharp_Code_Challenge_Submission.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;

namespace Csharp_Code_Challenge_Submission.Tests
{
    public class UserControllerTests
    {
        private readonly UserController _controller;

        public UserControllerTests()
        {
            // Configure and create mocks
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(config => config.GetSection("AppSettings:Token").Value).Returns("YqkeHlzAiPUWck+avW33kRcocnhClMKcCvVz6P32+4ynempJRZZUFzil0VLY0kfAHQR4dfevLbsxcdg8rMMAg0IC1Mjq1AcuDnCBbZ3B1FEU=");


            var userRepository = new Mock<UserRepository>();

            // Set up the mock repo methods
            userRepository.Setup(repo => repo.FindUserByUsername(It.IsAny<string>())).Returns((string username) =>
            {
                if (username == "existingUser")
                {
                    return new User { Username = "existingUser" };
                }
                return null;
            });

            userRepository.Setup(repo => repo.FindUserByEmail(It.IsAny<string>())).Returns((string email) =>
            {
                if (email == "existing@example.com")
                {
                    return new User { Email = "existing@example.com" };
                }
                return null;
            });

            userRepository.Setup(repo => repo.AddUser(It.IsAny<User>())).Returns((User user) =>
            {
                return user;
            });

            _controller = new UserController(configuration.Object, userRepository.Object);
        }


        [Fact]
        public void Register_Should_Return_Ok_With_New_User_Data_If_Username_And_Email_Are_Unique()
        {
            // Arrange
            var request = new UserReq
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "passworD!23"
            };

            var userRepositoryMock = new Mock<UserRepository>();
            userRepositoryMock.Setup(repo => repo.FindUserByUsername(request.Username)).Returns((User)null);
            userRepositoryMock.Setup(repo => repo.FindUserByEmail(request.Email)).Returns((User)null);

            var configurationMock = new Mock<IConfiguration>();

            var controller = new UserController(configurationMock.Object, userRepositoryMock.Object);

            // Act
            var result = controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var userModel = Assert.IsType<User>(okResult.Value);
            Assert.Equal("testuser", userModel.Username);
            Assert.Equal("test@example.com", userModel.Email);
        }


        [Fact]
        public void Register_Should_Return_BadRequest_If_Username_Already_Exists()
        {
            // Arrange
            var request = new UserReq
            {
                Username = "existinguser",
                Email = "test@example.com",
                Password = "password123"
            };

            var existingUser = new User
            {
                Username = "existinguser",
                Email = "existing@example.com"
            };

            var userRepositoryMock = new Mock<UserRepository>();
            userRepositoryMock.Setup(repo => repo.FindUserByUsername(request.Username)).Returns(existingUser);

            var configurationMock = new Mock<IConfiguration>();

            var controller = new UserController(configurationMock.Object, userRepositoryMock.Object);

            // Act
            var result = controller.Register(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }


        [Fact]
        public void Register_Should_Return_BadRequest_If_Email_Already_Exists()
        {
            // Arrange
            var request = new UserReq
            {
                Username = "testuser",
                Email = "existing@example.com",
                Password = "password123"
            };

            var existingUser = new User
            {
                Username = "existinguser",
                Email = "existing@example.com"
            };

            var userRepositoryMock = new Mock<UserRepository>();
            userRepositoryMock.Setup(repo => repo.FindUserByUsername(request.Username)).Returns((User)null);
            userRepositoryMock.Setup(repo => repo.FindUserByEmail(request.Email)).Returns(existingUser);

            var configurationMock = new Mock<IConfiguration>();

            var controller = new UserController(configurationMock.Object, userRepositoryMock.Object);

            // Act
            var result = controller.Register(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }


        [Fact]
        public void Login_Should_Return_Ok_With_User_Data_If_Credentials_Are_Valid()
        {
            // Arrange
            var request = new UserReq
            {
                Username = "testuser",
                Password = "password123"
            };

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
            var existingUser = new User
            {
                Username = "testuser",
                PasswordHash = hashedPassword
            };

            var userRepositoryMock = new Mock<UserRepository>();
            userRepositoryMock.Setup(repo => repo.FindUserByUsername(request.Username)).Returns(existingUser);

            var configurationMock = new Mock<IConfiguration>();

            var controller = new UserController(configurationMock.Object, userRepositoryMock.Object);

            // Act
            var result = controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var userModel = Assert.IsType<User>(okResult.Value);
            Assert.Equal("testuser", userModel.Username);
            // Add more assertions if needed
        }


        [Fact]
        public void Login_Should_Return_BadRequest_If_Credentials_Are_Invalid()
        {
            // Arrange
            var request = new UserReq
            {
                Username = "testuser",
                Password = "invalidpassword"
            };

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
            var existingUser = new User
            {
                Username = "testuser",
                PasswordHash = hashedPassword
            };

            var userRepositoryMock = new Mock<UserRepository>();
            userRepositoryMock.Setup(repo => repo.FindUserByUsername(request.Username)).Returns(existingUser);

            var configurationMock = new Mock<IConfiguration>();

            var controller = new UserController(configurationMock.Object, userRepositoryMock.Object);

            // Act
            var result = controller.Login(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }


        [Fact]
        public void GetUserInfo_Should_Return_Ok_With_User_Data_If_User_Exists()
        {
            // Arrange
            var userId = "e5c091ea";
            var user = new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com"
            };

            var userRepositoryMock = new Mock<UserRepository>();
            userRepositoryMock.Setup(repo => repo.FindUserById(userId)).Returns(user);

            var configurationMock = new Mock<IConfiguration>();

            var controller = new UserController(configurationMock.Object, userRepositoryMock.Object);

            // Act
            var result = controller.GetUserInfo(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var userModel = Assert.IsType<User>(okResult.Value);
            Assert.Equal(userId, userModel.Id);
            Assert.Equal("testuser", userModel.Username);
            Assert.Equal("test@example.com", userModel.Email);
        }


        [Fact]
        public void GetUserInfo_Should_Return_NotFound_If_User_Does_Not_Exist()
        {
            // Arrange
            var userId = "nonexistentuser"; // Replace with a non-existing user ID

            var userRepositoryMock = new Mock<UserRepository>();
            userRepositoryMock.Setup(repo => repo.FindUserById(userId)).Returns((User)null);

            var configurationMock = new Mock<IConfiguration>();

            var controller = new UserController(configurationMock.Object, userRepositoryMock.Object);

            // Act
            var result = controller.GetUserInfo(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public void UpdateUserInfo_Should_Return_Ok_With_Updated_User_Data_If_User_Exists()
        {
            // Arrange
            var userId = "e5c091ea"; 
            var request = new UserReq
            {
                Username = "updateduser",
                Email = "updated@example.com",
                Password = "newpassword"
            };

            var user = new User
            {
                Id = userId,
                Username = "oldusername",
                Email = "old@example.com",
                PasswordHash = "oldpasswordhash"
            };

            var userRepositoryMock = new Mock<UserRepository>();
            userRepositoryMock.Setup(repo => repo.FindUserById(userId)).Returns(user);

            var configurationMock = new Mock<IConfiguration>();

            var controller = new UserController(configurationMock.Object, userRepositoryMock.Object);

            // Act
            var result = controller.UpdateUserInfo(userId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var userModel = Assert.IsType<User>(okResult.Value);
            Assert.Equal(userId, userModel.Id);
            Assert.Equal("updateduser", userModel.Username);
            Assert.Equal("updated@example.com", userModel.Email);
            Assert.NotEqual("oldpasswordhash", userModel.PasswordHash);
        }


        [Fact]
        public void UpdateUserInfo_Should_Return_NotFound_If_User_Does_Not_Exist()
        {
            // Arrange
            var userId = "nonexistentuser";
            var request = new UserReq
            {
                Username = "updateduser",
                Email = "updated@example.com",
                Password = "newpassword"
            };

            var userRepositoryMock = new Mock<UserRepository>();
            userRepositoryMock.Setup(repo => repo.FindUserById(userId)).Returns((User)null);

            var configurationMock = new Mock<IConfiguration>();

            var controller = new UserController(configurationMock.Object, userRepositoryMock.Object);

            // Act
            var result = controller.UpdateUserInfo(userId, request);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
