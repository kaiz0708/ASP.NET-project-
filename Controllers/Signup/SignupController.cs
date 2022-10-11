using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Data;
using WebAPI.TokenAuth;
using MySql.Data.MySqlClient;
using WebAPI.Featear;
namespace WebAPI.Controllers.Signup
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignupController : ControllerBase
    {
        IConfiguration config = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddEnvironmentVariables()
                                .Build();
        private readonly ILogger<SignupController> _logger;
        public SignupController(ILogger<SignupController> logger)
        {
            _logger = logger;
        }

        [HttpPost("signup")]
        public IActionResult SighUp([FromBody] UserSignup UserFromSignUp)
        {
            var connect = new ConnectServer();
            var checkData = new UserGet();
            var dataResponse = new ResponseAuth();
            var userDataResponse = new UserResponseAuth();
            var ResulstRequest = new UserSignup()
            {
                username = UserFromSignUp.username,
                account = UserFromSignUp.account,
                pass = UserFromSignUp.pass,
                address = UserFromSignUp.address,
                phone = UserFromSignUp.phone,
                name = UserFromSignUp.name,
                date_of_birth = UserFromSignUp.date_of_birth
            };
            MySqlConnection conn = connect.Connect();
            MySqlCommand selectAccount = new MySqlCommand($"SELECT * FROM User Where accountUser='{ResulstRequest.account}'", conn);
            selectAccount.Connection = conn;
            conn.Open();
            MySqlDataReader reader = selectAccount.ExecuteReader();
            while (reader.Read())
            {
                checkData.UserId = reader["idUser"].ToString();
                checkData.typeEmployee = reader["typeEmployee"].ToString();
                checkData.account = reader["accountUser"].ToString();
                checkData.pass = reader["pass"].ToString();
            }
            conn.Close();
            if (checkData.account != null)
            {
                Dictionary<string, string> resultsfail = new Dictionary<string, string>();
                resultsfail.Add("SignupFail", "fail");
                return new JsonResult(resultsfail);
            }
            else
            {
                conn.Open();
                string id = System.Guid.NewGuid().ToString();
                var Cart = new List<Dictionary<string, object>>();
                var new_hash = new HashandVerifyPass();
                string passwordHash = new_hash.HashPassword(ResulstRequest.pass);
                MySqlCommand insertUser = new MySqlCommand($"insert into User(idUser, userName, nameUser, date_of_birth, phone, address, pic, accountUser, pass, typeEmployee) " +
                    $"values('{id}' , '{ResulstRequest.username}', '{ResulstRequest.name}', '{ResulstRequest.date_of_birth}', '{ResulstRequest.phone}', '{ResulstRequest.address}', ' ', '{ResulstRequest.account}', '{passwordHash}', 'user')", conn);
                insertUser.ExecuteNonQuery();
                conn.Close();
                userDataResponse.username = ResulstRequest.username;
                userDataResponse.avatar = " ";
                Dictionary<string, object> data = dataResponse.ResultsResponse(id, userDataResponse , Cart);
                return new JsonResult(data);
            }
        }
    }
}
