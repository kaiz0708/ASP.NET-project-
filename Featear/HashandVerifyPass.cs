using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using BCrypt.Net;
using WebAPI.Models;
namespace WebAPI.Featear
{
    public class HashandVerifyPass
    {
		IConfiguration config = new ConfigurationBuilder()
								.AddJsonFile("appsettings.json")
								.AddEnvironmentVariables()
								.Build();
		public string HashPassword(string password)
		{
			return BCrypt.Net.BCrypt.HashPassword(password);
		}

		public bool ValidatePassword(string password, string correctHash)
		{
			return BCrypt.Net.BCrypt.Verify(password, correctHash);
		}
	}
}
