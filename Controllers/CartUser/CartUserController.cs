using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.MiddleWare;
using WebAPI.Data;
using MySql.Data.MySqlClient;
namespace WebAPI.Controllers.CartUser
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartUserController : ControllerBase
    {
        IConfiguration config = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddEnvironmentVariables()
                                .Build();

        [HttpPost("addcart")]

        public IActionResult AddCart([FromBody] Cart CartRequest)
        {
            var tokenRequest = new MiddlewareToken();
            if(Request.Headers.TryGetValue("token" , out var value) && Request.Headers.TryGetValue("refreshtoken", out var value2))
            {
                tokenRequest.token = value;
                tokenRequest.refreshtoken = value2;
            }
            var checkAuth = new MiddlewareAuth();
            Dictionary<string, bool> resultsAuth = new Dictionary<string, bool>();
            if (config["ErrorToken"].Equals(checkAuth.checkTokenLife(tokenRequest.token, tokenRequest.refreshtoken)) == false)
            {
                resultsAuth.Add("checkAuth", false);
                return new JsonResult(resultsAuth);
            }
            string idCart = System.Guid.NewGuid().ToString();
            string idUser = checkAuth.checkTokenLife(tokenRequest.token, tokenRequest.refreshtoken);
            var cartRequest = new Cart
            {
                idProduct = CartRequest.idProduct,
                quantity = CartRequest.quantity,
                size = CartRequest.size,
            };
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            MySqlCommand insertCart = new MySqlCommand($"insert into Cart(IdCart, IdUser, Idproduct, quantity, size)" +
                $"values('{idCart}', '{idUser}', '{cartRequest.idProduct}', '{cartRequest.quantity}', '{cartRequest.size}')", conn);
            insertCart.ExecuteNonQuery();
            conn.Close();
            resultsAuth.Add("addCart", true);
            return new JsonResult(resultsAuth);
        }

        [HttpGet("deleteCart")]

        public IActionResult DeleteCart(string id)
        {
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            MySqlCommand deleteCart = new MySqlCommand($"delete from Cart where IdCart='{id}'", conn);
            deleteCart.ExecuteNonQuery();
            conn.Close();
            Dictionary<string, object> results = new Dictionary<string, object>();
            results.Add("id_cart", id);
            results.Add("delete" , true);
            return new JsonResult(results);
        }
    }
}
