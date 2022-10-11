using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
namespace WebAPI.TokenAuth
{
    public class Token
    {

        IConfiguration config = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddEnvironmentVariables()
                                .Build();
        public string CreateToken(string id, string keySecret, int time)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(keySecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", id),
                }),
                Expires = DateTime.UtcNow.AddHours(time),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokens = tokenHandler.WriteToken(token);
            return tokens.ToString();
        }

        public string VerifyToken(string token, string keySecret)
        {
            string userId;
            var SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keySecret));
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                {
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidAudience = config["Jwt:Audience"],
                        IssuerSigningKey = SecurityKey,
                    }, out SecurityToken validatedToken);
                    var jwtToken = (JwtSecurityToken)validatedToken;
                    userId = jwtToken.Claims.First(x => x.Type == "id").Value;
                }
            }
            catch
            {
                return config["ErrorToken"];
            }

            return userId;
        }
    }
}
