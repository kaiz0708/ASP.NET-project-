using WebAPI.TokenAuth;
using WebAPI.Models;
using MySql.Data.MySqlClient;
using WebAPI.Data;
namespace WebAPI.Featear
{
    public class ResponseAuth
    {
        IConfiguration config = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddEnvironmentVariables()
                                .Build();
        public List<Dictionary<string, object>> ReturnCart(string id)
        {
            var middle = new MiddlewareToken();
            var ListCart = new List<Dictionary<string, object>>();
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            string sql = $"select nameProduct, brand, picture, cost, discount, Cart.IdCart, Product.IdProduct, quantity, Cart.size from Cart join Product on Cart.Idproduct = Product.IdProduct where Cart.IdUser='{id}'";
            MySqlCommand selectAccount = new MySqlCommand(sql, conn);
            selectAccount.Connection = conn;
            conn.Open();
            MySqlDataReader reader = selectAccount.ExecuteReader();
            while (reader.Read())
            {
                var cart = new Dictionary<string, object>();
                var infor_product = new Dictionary<string, object>();
                infor_product.Add("title", reader["nameProduct"].ToString());
                infor_product.Add("brand", reader["brand"].ToString());
                infor_product.Add("pic", reader["picture"].ToString());
                infor_product.Add("cost",Convert.ToDouble(reader["cost"]));
                infor_product.Add("discount",Convert.ToInt32(reader["discount"]));
                cart.Add("infor_product", infor_product);
                cart.Add("id_cart", reader["IdCart"].ToString());
                cart.Add("id_product", reader["IdProduct"]);
                cart.Add("quantity", reader["quantity"].ToString());
                cart.Add("size", reader["size"]);
                ListCart.Add(cart);
            }
            conn.Close();
            return ListCart;
        }
        public Dictionary<string, object> ResultsResponse(string id, UserResponseAuth userResponse, List<Dictionary<string, object>> list)
        {
            var token = new Token();
            Dictionary<string, object> result = new Dictionary<string, object>();
            Dictionary<string, object> resultSuccess = new Dictionary<string, object>();
            result.Add("token", token.CreateToken(id, config["Jwt:Secret"],Convert.ToInt32(config["Jwt:tokenLife"])));
            result.Add("refreshToken", token.CreateToken(id, config["Jwt:SecretRefreshToken"], Convert.ToInt32(config["Jwt:refreshTokenLife"])));
            result.Add("data", userResponse);
            resultSuccess.Add("cart", list);
            resultSuccess.Add("user", result);
            return resultSuccess;
        }
    }
}
