using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Data;
using WebAPI.TokenAuth;
using WebAPI.Featear;
using MySql.Data.MySqlClient;
using WebAPI.MiddleWare;
namespace WebAPI.Controllers.Login
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        IConfiguration config = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddEnvironmentVariables()
                                .Build();
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLogin UserFromLogin)
        {
            var connect = new ConnectServer();
            var checkData = new UserGet();
            var userDataResponse = new UserResponseAuth();
            var verifyPass = new HashandVerifyPass();
            var resultsAuth = new ResponseAuth();
            var ResultsRequest = new UserLogin()
            {
                account = UserFromLogin.account,
                pass = UserFromLogin.pass
            };
            MySqlConnection conn = connect.Connect();
            MySqlCommand selectAccount = new MySqlCommand($"SELECT * FROM User Where accountUser='{ResultsRequest.account}'", conn);
            selectAccount.Connection = conn;
            conn.Open();
            MySqlDataReader reader = selectAccount.ExecuteReader();
            while (reader.Read())
            {
                checkData.UserId = reader["idUser"].ToString();
                checkData.typeEmployee = reader["typeEmployee"].ToString();
                checkData.account = reader["accountUser"].ToString();
                checkData.pass = reader["pass"].ToString();
                userDataResponse.username = reader["userName"].ToString();
                userDataResponse.avatar = reader["pic"].ToString();
            }
            conn.Close();
            if(checkData == null || verifyPass.ValidatePassword(ResultsRequest.pass, checkData.pass) == false)
            {
                Dictionary<string, object> resultsLogin = new Dictionary<string, object>();
                resultsLogin.Add("Login", "fail");
                return new JsonResult(resultsLogin);
            }
            else
            {
                if(checkData.typeEmployee == config["UserType"])
                {
                    List<Dictionary<string, object>> Cart = resultsAuth.ReturnCart(checkData.UserId);
                    Dictionary<string, object> resultsAuthLogin = resultsAuth.ResultsResponse(checkData.UserId, userDataResponse, Cart);
                    resultsAuthLogin.Add("authorization", config["UserType"]);
                    return new JsonResult(resultsAuthLogin);
                }
                else
                {
                    Dictionary<string, string> resultsAdmin = new Dictionary<string, string>();
                    resultsAdmin.Add("authorization", config["AdminType"]);
                    resultsAdmin.Add("secret_id", config["SecretIdUrlAdmin"]);
                    return new JsonResult(resultsAdmin);
                }
            } 
        }

        [HttpPost("loginService")]

        public IActionResult LoginService([FromBody] LoginService LoginAuth)
        {
            var user = new UserGet();
            var userAuth = new UserResponseAuth();
            var dataLogin = new LoginService
            {
                username = LoginAuth.username,
                userIdAuth = LoginAuth.userIdAuth,
            };
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            MySqlCommand getUserAuth = new MySqlCommand($"SELECT * FROM User where accountUser='{dataLogin.userIdAuth}'", conn);
            MySqlDataReader reader = getUserAuth.ExecuteReader();
            while (reader.Read())
            {
                user.UserId = reader["idUser"].ToString();
                user.account = reader["accountUser"].ToString();
                user.pass = reader["pass"].ToString();
                user.typeEmployee = reader["typeEmployee"].ToString();
                userAuth.username = reader["userName"].ToString();
                userAuth.avatar = reader["pic"].ToString();
            }
            conn.Close();
            if (user.UserId == null)
            {
                string id = System.Guid.NewGuid().ToString();
                conn.Open();
                MySqlCommand insertUser = new MySqlCommand($"INSERT INTO User(idUser, userName, nameUser, date_of_birth, phone, address, pic, accountUser, pass, typeEmployee)" +
                    $"values('{id}', '{dataLogin.username}', ' ', ' ', ' ', ' ', ' ', '{dataLogin.userIdAuth}', '{config["PassDefault"]}','{config["UserType"]}')", conn);
                insertUser.ExecuteNonQuery();
                conn.Close();
                userAuth.username = dataLogin.username;
                userAuth.avatar = " ";
                var Cart = new List<Dictionary<string, object>>();
                var results = new ResponseAuth();
                var resultsData = results.ResultsResponse(id, userAuth, Cart);
                return new JsonResult(resultsData);
            }
            else
            {
                var results = new ResponseAuth();
                var Cart = results.ReturnCart(user.UserId);
                var resultsData = results.ResultsResponse(user.UserId, userAuth, Cart);
                return new JsonResult(resultsData);
            }
        }

        [HttpGet("loginAuth")]

        public IActionResult LoginAuth()
        {
            var connect = new ConnectServer();
            var userAuth = new UserResponseAuth();
            MySqlConnection conn = connect.Connect();
            var tokenRequest = new MiddlewareToken();
            if (Request.Headers.TryGetValue("token", out var value) && Request.Headers.TryGetValue("refreshtoken", out var value2))
            {
                tokenRequest.token = value;
                tokenRequest.refreshtoken = value2;
            }
            var tokenCheckAuth = new MiddlewareAuth();
            var tokenAuth = tokenCheckAuth.checkTokenLife(tokenRequest.token, tokenRequest.refreshtoken);
            if (tokenAuth.Equals(config["ErrorToken"]) == true)
            {
                Dictionary<string, object> resultsLoginAuth = new Dictionary<string, object>();
                resultsLoginAuth.Add("Login", "fail");
                return new JsonResult(resultsLoginAuth);
            }
            string idUser = tokenAuth;
            var cart = new ResponseAuth();
            List<Dictionary<string, object>> Cart = cart.ReturnCart(idUser);
            conn.Open();
            MySqlCommand getInforUser = new MySqlCommand($"SELECT userName, pic FROM User WHERE idUser='{idUser}'", conn);
            MySqlDataReader reader = getInforUser.ExecuteReader();
            while (reader.Read())
            {
                userAuth.username = reader["userName"].ToString();
                userAuth.avatar = reader["pic"].ToString();
            }
            Dictionary<string, object> results = new Dictionary<string, object>();
            results.Add("cart", Cart);
            results.Add("user", userAuth);
            return new JsonResult(results);
        }
    }
}

