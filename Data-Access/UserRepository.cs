using System;
using System.Collections.Generic;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Csharp_Code_Challenge_Submission.Models;

public class UserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("UserDatabaseSettings:ConnectionString").Value;
        var databaseName = configuration.GetSection("UserDatabaseSettings:DatabaseName").Value;

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _users = database.GetCollection<User>("users");
    }

    public User AddUser(User user)
    {
        _users.InsertOne(user);
        return user;
    }

    public void UpdateUser(User user)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
        _users.ReplaceOne(filter, user);
    }

    public User FindUserById(string id)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        return _users.Find(filter).FirstOrDefault();
    }

    public User FindUserByUsername(string username)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Username, username);
        return _users.Find(filter).FirstOrDefault();
    }
}
