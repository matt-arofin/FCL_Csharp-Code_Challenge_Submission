using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Csharp_Code_Challenge_Submission.Models
{
	public class User
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set;  }

		public string Username { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
    }
}