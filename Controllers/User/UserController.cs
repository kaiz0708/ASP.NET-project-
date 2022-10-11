using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.MiddleWare;
using WebAPI.Data;
using MySql.Data.MySqlClient;

namespace WebAPI.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        IConfiguration config = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddEnvironmentVariables()
                                .Build();
        private readonly ILogger<UserController> _logger;

        [HttpGet("getUserClient")]

        public IActionResult getUser()
        {
            var tokenRequest = new MiddlewareToken();
            if (Request.Headers.TryGetValue("token", out var value) && Request.Headers.TryGetValue("refreshtoken", out var value2))
            {
                tokenRequest.token = value;
                tokenRequest.refreshtoken = value2;
            }
            var checkToken = new MiddlewareAuth();
            string checkAuth = checkToken.checkTokenLife(tokenRequest.token, tokenRequest.refreshtoken);
            if (checkAuth.Equals(config["ErrorToken"]) == true)
            {
                Dictionary<string, bool> resulst = new Dictionary<string, bool>();
                resulst.Add("checkAuth", false);
                return new JsonResult(resulst);
            }
            string IdUser = checkAuth;

            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            string sqlgetUser = $"SELECT * FROM User WHERE idUser='{IdUser}'";
            MySqlCommand getInforUser = new MySqlCommand(sqlgetUser, conn);
            MySqlDataReader reader = getInforUser.ExecuteReader();
            var user = new ReturnInforUser();
            while (reader.Read())
            {
                user.username = reader["userName"].ToString();
                user.name = reader["nameUser"].ToString();
                user.date_of_birth = reader["date_of_birth"].ToString();
                user.phone = reader["phone"].ToString();
                user.address = reader["address"].ToString();
                user.avatar = reader["pic"].ToString();
            }
            Dictionary<string, object> User = new Dictionary<string, object>();
            User.Add("infor_user", user);
            return new JsonResult(User);
        }

        [HttpPost("uploadAvatar/user")]

        public IActionResult UploadAvatar([FromForm] FileStr filePicture)
        {
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            var tokenRequest = new MiddlewareToken();
            if (Request.Headers.TryGetValue("token", out var value) && Request.Headers.TryGetValue("refreshtoken", out var value2))
            {
                tokenRequest.token = value;
                tokenRequest.refreshtoken = value2;
            }
            var checkToken = new MiddlewareAuth();
            string checkAuth = checkToken.checkTokenLife(tokenRequest.token, tokenRequest.refreshtoken);
            if (checkAuth.Equals(config["ErrorToken"]) == true)
            {
                Dictionary<string, bool> resulst = new Dictionary<string, bool>();
                resulst.Add("checkAuth", false);
                return new JsonResult(resulst);
            }
            IFormFile fileUser = filePicture.file;
            string idUser = checkAuth;
            var stream = new FileStream($"Avatar/images/{fileUser.FileName}", FileMode.Create);
            fileUser.CopyTo(stream);
            conn.Open();
            string sqlUpdateAvatar = $"UPDATE User Set pic = 'avatar/images/{fileUser.FileName}' where idUser='{idUser}'";
            MySqlCommand updateAvatar = new MySqlCommand(sqlUpdateAvatar, conn);
            updateAvatar.ExecuteNonQuery();
            conn.Close();
            Dictionary<string, bool> resulstUpdate = new Dictionary<string, bool>();
            resulstUpdate.Add("update", true);
            return new JsonResult(resulstUpdate);
        }

        [HttpPost("updateUser")]

        public IActionResult UpdateUser([FromBody] GetUserForClient UserClient)
        {
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            var tokenRequest = new MiddlewareToken();
            if (Request.Headers.TryGetValue("token", out var value) && Request.Headers.TryGetValue("refreshtoken", out var value2))
            {
                tokenRequest.token = value;
                tokenRequest.refreshtoken = value2;
            }
            var checkToken = new MiddlewareAuth();
            string checkAuth = checkToken.checkTokenLife(tokenRequest.token, tokenRequest.refreshtoken);
            if (checkAuth.Equals(config["ErrorToken"]) == true)
            {
                Dictionary<string, bool> resulst = new Dictionary<string, bool>();
                resulst.Add("checkAuth", false);
                return new JsonResult(resulst);
            }
            string idUser = checkAuth;
            conn.Open();
            string sqlUpdateUser = $"UPDATE User SET userName='{UserClient.username}', nameUser='{UserClient.name}', date_of_birth='{UserClient.date_of_birth}', phone='{UserClient.phone}', address='{UserClient.address}' WHERE idUser='{idUser}'";
            MySqlCommand UpdateUser = new MySqlCommand(sqlUpdateUser, conn);
            UpdateUser.ExecuteNonQuery();
            conn.Close();
            Dictionary<string, bool> resulstUpdate = new Dictionary<string,bool>();
            resulstUpdate.Add("update", true);
            return new JsonResult(resulstUpdate);
        }




        [HttpGet("admin/deleteUser")]

        public IActionResult DeleteUser(string idUser)
        {
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            string sqlDeleteUser = $"DELETE FROM User WHERE idUser='{idUser}'";
            MySqlCommand deleteUser = new MySqlCommand(sqlDeleteUser, conn);
            deleteUser.ExecuteNonQueryAsync();
            conn.Close();
            Dictionary<string, bool> resulstDeleteUser = new Dictionary<string, bool>();
            resulstDeleteUser.Add("delete", true);
            return new JsonResult(resulstDeleteUser);
        }

        [HttpGet("admin/getAllUser")]

        public IActionResult GetAllUser()
        {
            var ListuserAdmin = new List<GetUserForAdmin>();
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            string sqlgetAllUser = $"SELECT * FROM User";
            MySqlCommand getAllUser = new MySqlCommand(sqlgetAllUser, conn);
            MySqlDataReader reader = getAllUser.ExecuteReader();
            while (reader.Read())
            {
                var user = new GetUserForAdmin
                {
                    IdUser = reader["idUser"].ToString(),
                    account = reader["accountUser"].ToString(),
                    username = reader["userName"].ToString(),
                    date_of_birth = reader["date_of_birth"].ToString(),
                    phone = reader["phone"].ToString(),
                    address = reader["address"].ToString()
                };
                ListuserAdmin.Add(user);
            }
            return new JsonResult(ListuserAdmin);
        }



    }
}
