using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Data;
using WebAPI.MiddleWare;
using MySql.Data.MySqlClient;

namespace WebAPI.Controllers.Feedback
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        IConfiguration config = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddEnvironmentVariables()
                                .Build();

        [HttpPost("addFeedback")]

        public IActionResult AddFeedBack([FromBody] getFeedback new_feedback)
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
            string idUser = checkAuth;
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            string idMes = System.Guid.NewGuid().ToString();
            DateTime today = DateTime.Now;
            string date = DateTime.Parse(today.ToString()).ToString("dd/MM/yyyy");
            string time = DateTime.Parse(today.ToString()).ToString("HH:mm:ss");
            int like = Convert.ToInt32(config["DefaultLike"]);
            string sqlAddFeedback = $"INSERT INTO Feedback(IdMes, IdUser, IdProduct, content, dateFeedback, timeFeedback, liked)" +
                $"values('{idMes}', '{idUser}', '{new_feedback.IdProduct}', '{new_feedback.content}', '{date}', '{time}', '{like}')";
            MySqlCommand insertFeedback = new MySqlCommand(sqlAddFeedback, conn);
            insertFeedback.ExecuteNonQuery();
            conn.Close();
            return Ok();
        }

        [HttpGet("getFeedback")]

        public IActionResult getFeedBack(string IdProduct)
        {
            var listFeedback = new List<returnFeedback>();
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            string sqlgetFeedBack = $"SELCET * FROM Feedback WHERE IdProduct='{IdProduct}'";
            MySqlCommand GetFeedback = new MySqlCommand(sqlgetFeedBack, conn);
            MySqlDataReader reader = GetFeedback.ExecuteReader();
            while (reader.Read())
            {
                var feed = new returnFeedback
                {
                    IdMes = reader["IdMes"].ToString(),
                    IdUser = reader["IdUser"].ToString(),
                    IdProduct = reader["IdProduct"].ToString(),
                    content = reader["content"].ToString(),
                    date = reader["dateFeedback"].ToString(),
                    time = reader["timeFeedback"].ToString(),
                    like = Convert.ToInt32(reader["liked"])
                };
                listFeedback.Add(feed);
            }
            Dictionary<string, object> results = new Dictionary<string, object>();
            results.Add("feedback", listFeedback);
            return new JsonResult(results);
        }

        [HttpGet("deleteFeedback")]
        public IActionResult DeleteFeedback(string IdMes)
        {
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            string sqlDeleteFeedback = $"DELETE FROM Feedback WHERE IdMes='{IdMes}'";
            MySqlCommand DeleteFeedback = new MySqlCommand(sqlDeleteFeedback, conn);
            DeleteFeedback.ExecuteNonQuery();
            conn.Close();
            return Ok();
        }

        [HttpPost("updateFeedback")]
        public IActionResult UpdateFeedback([FromBody] updateFeedback contentUpadte)
        {
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            string sqlUpdateFeedback = $"UPDATE Feedback SET content='{contentUpadte.content}' WHERE IdMes='{contentUpadte.IdMes}'";
            MySqlCommand DeleteFeedback = new MySqlCommand(sqlUpdateFeedback, conn);
            DeleteFeedback.ExecuteNonQuery();
            conn.Close();
            return Ok();
        }

        [HttpGet("like")]

        public IActionResult LikeFeedback(string IdMes, bool check)
        {
            int update;
            if (check)
            {
                update = 1;
            }
            else
            {
                update = -1;
            }
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            string sqlLike = $"UPDATE Feedback SET liked = liked - {update} WHERE IdMes='{IdMes}'";
            MySqlCommand LikeFeedback = new MySqlCommand(sqlLike, conn);
            LikeFeedback.ExecuteNonQuery();
            conn.Close();
            return Ok();
        }
    }
}
