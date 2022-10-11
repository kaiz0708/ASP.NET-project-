using WebAPI.TokenAuth;
using WebAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace WebAPI.MiddleWare
{
    public class MiddlewareAuth
    {
        IConfiguration config = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddEnvironmentVariables()
                                .Build();

        public string checkTokenLife(string token, string refreshToken)
        {
            var checkToken = new Token();
            string resultsToken = checkToken.VerifyToken(token, config["Jwt:Secret"]);
            string resultsRefreshToken = checkToken.VerifyToken(refreshToken, config["Jwt:SecretRefreshToken"]);
            if (resultsToken.Equals(config["ErrorToken"]) == false)
            {
                return resultsToken;
            }
            if (resultsRefreshToken.Equals(config["ErrorToken"]) == false)
            {
                return resultsRefreshToken;
            }
            else
            {
                return config["ErrorToken"];
            }
        }
    }

}
