using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Data;
using WebAPI.Models;
using MySql.Data.MySqlClient;
using WebAPI.MiddleWare;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayController : ControllerBase
    {

        IConfiguration config = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddEnvironmentVariables()
                                .Build();

        private readonly ILogger<PayController> _logger;

        public PayController(ILogger<PayController> logger)
        {
            _logger = logger;
        }

        [HttpPost("addPay")]

        public IActionResult addPay([FromBody] Pay UserBill)
        {
            var tokenRequest = new MiddlewareToken();
            if (Request.Headers.TryGetValue("token", out var value) && Request.Headers.TryGetValue("refreshtoken", out var value2))
            {
                tokenRequest.token = value;
                tokenRequest.refreshtoken = value2;
            }
            var checkToken = new MiddlewareAuth();
            string checkAuth = checkToken.checkTokenLife(tokenRequest.token, tokenRequest.refreshtoken);
            Console.WriteLine(checkAuth);
            if(checkAuth.Equals(config["ErrorToken"]) == true)
            {
                Dictionary<string, bool> resulst = new Dictionary<string, bool>();
                resulst.Add("checkAuth", false);
                return new JsonResult(resulst);
            }
            string IdUser = checkAuth;
            var pay = new Pay
            {
                IdProduct = UserBill.IdProduct,
                NameReceive = UserBill.NameReceive,
                Phone = UserBill.Phone,
                AddressReceive = UserBill.AddressReceive,
                quantity = UserBill.quantity,
                size = UserBill.size
            };
            var check = new CheckSize();
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            MySqlCommand checkSize = new MySqlCommand($"SELECT * FROM Size WHERE Idproduct = '{pay.IdProduct}' and {pay.size} >= {pay.quantity}", conn);
            MySqlDataReader reader = checkSize.ExecuteReader();
            while (reader.Read())
            {
                check.IdSize = reader["IdSize"].ToString();
                check.quantitySize = Convert.ToInt32(reader[pay.size]);
            }
            conn.Close();
            if(check.IdSize == null)
           {
                Dictionary<string, object> dataFail = new Dictionary<string, object>();
                dataFail.Add("buy_product", false);
                return new JsonResult(dataFail);
            }
            else
            {
                conn.Open();
                string IdPay = System.Guid.NewGuid().ToString();
                DateTime today = DateTime.Now;
                MySqlCommand insertPay = new MySqlCommand($"INSERT INTO Pay(IdPay, IdUser, NameReceive, Phone, AddressReceive, IdProduct, size, quantity, statusPay, datePay)" +
                    $"values('{IdPay}', '{IdUser}', '{pay.NameReceive}', '{pay.Phone}', '{pay.AddressReceive}', '{pay.IdProduct}', '{pay.size}', '{pay.quantity}', '{config["statusDefault"]}', '{today}')", conn);
                insertPay.ExecuteNonQuery();
                MySqlCommand updateSize = new MySqlCommand($"UPDATE Size SET {pay.size}={check.quantitySize - pay.quantity} where Idproduct='{pay.IdProduct}'", conn);
                updateSize.ExecuteNonQuery();
                conn.Close();
                Dictionary<string, object> resulstSuccess = new Dictionary<string, object>();
                resulstSuccess.Add("buy_product", true);
                return new JsonResult(resulstSuccess);
            }
        }

        [HttpGet("getBill")]

        public IActionResult ReturnBillUser(string status)
        {
            string condition = "and";
            if (status.Equals(config["StatusgetAll"]))
            {
                condition = "or";
            }
            var token = new MiddlewareAuth();
            var connect = new ConnectServer();
            var tokenRequest = new MiddlewareToken();
            if (Request.Headers.TryGetValue("token", out var value) && Request.Headers.TryGetValue("refreshtoken", out var value2))
            {
                tokenRequest.token = value;
                tokenRequest.refreshtoken = value2;
            }
            string checkAuth = token.checkTokenLife(tokenRequest.token, tokenRequest.refreshtoken);
            if (checkAuth.Equals(config["ErrorToken"]) == true)
            {
                Dictionary<string, bool> resulst = new Dictionary<string, bool>();
                resulst.Add("checkAuth", false);
                return new JsonResult(resulst);
            }
            string idUser = checkAuth;
            var ListBill = new List<Dictionary<string, object>>();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            MySqlCommand getBillForUser = new MySqlCommand($"SELECT * FROM Pay join Product on Pay.IdProduct = Product.IdProduct where IdUser='{idUser}' {condition} statusPay='{status}'", conn);
            MySqlDataReader reader = getBillForUser.ExecuteReader();
            while (reader.Read())
            {
                Dictionary<string, object> Bill = new Dictionary<string, object>();
                var user = new getUserPay
                {
                    NameReceive = reader["NameReceive"].ToString(),
                    Phone = reader["Phone"].ToString(),
                    AddressReceive = reader["AddressReceive"].ToString()
                };
                var products = new customProduct
                {
                    IdProduct = reader["IdProduct"].ToString(),
                    nameProduct = reader["nameProduct"].ToString(),
                    brand = reader["brand"].ToString(),
                    picture = reader["picture"].ToString(),
                    cost = Convert.ToDouble(reader["cost"]),
                    discount = Convert.ToInt32(reader["discount"])
                };
                Bill.Add("idBill", reader["IdPay"].ToString());
                Bill.Add("user", user);
                Bill.Add("product", products);
                Bill.Add("quantity", Convert.ToInt32(reader["quantity"]));
                Bill.Add("size", reader["size"].ToString());
                Bill.Add("time",DateTime.Parse(reader["datePay"].ToString()).ToString("HH:mm:ss"));
                Bill.Add("date", DateTime.Parse(reader["datePay"].ToString()).ToString("dd/MM/yyyy"));
                Bill.Add("status", reader["statusPay"].ToString());
                ListBill.Add(Bill);
            }
            conn.Close();
            return new JsonResult(ListBill);
        }

        [HttpPost("deletePay")]

        public IActionResult DeleteBill([FromBody] DeletePay InforDeletePay)
        {
            var connect = new ConnectServer();
            var Infor = new DeletePay
            {
                IdPay = InforDeletePay.IdPay,
                IdProduct = InforDeletePay.IdProduct,
                size = InforDeletePay.size,
                quantity = InforDeletePay.quantity,
            };
            MySqlConnection conn = connect.Connect();
            conn.Open();
            MySqlCommand DeletePay = new MySqlCommand($"DELETE FROM Pay where IdPay='{Infor.IdPay}'", conn);
            DeletePay.ExecuteNonQuery();
            MySqlCommand UpdateQuantitySize = new MySqlCommand($"UPDATE Size SET {Infor.size}={Infor.size} + {Infor.quantity} where Idproduct='{Infor.IdProduct}'", conn);
            UpdateQuantitySize.ExecuteNonQuery();
            conn.Close();
            Dictionary<string, object> results = new Dictionary<string, object>();
            results.Add("deleteBill", true);
            results.Add("idBill_delete", Infor.IdPay);
            return new JsonResult(results);
        }

        [HttpGet("updateStatus")]

        public IActionResult updateStatus(string idPay, string status)
        {
            string statusUpdate;
            if (status.Equals(config["StatusDefault"]) == true)
            {
                statusUpdate = config["StatusTaking"];
            }
            else
            {
                if(status.Equals(config["StatusTaking"]) == true)
                {
                    statusUpdate = config["StatusShipping"];
                }
                else
                {
                    statusUpdate = config["StatusComplete"];
                }
            }
            
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            MySqlCommand updateStatus = new MySqlCommand($"UPDATE Pay SET statusPay='{statusUpdate}' WHERE IdPay='{idPay}'", conn);
            updateStatus.ExecuteNonQuery();
            conn.Close();
            Dictionary<string , bool> results = new Dictionary<string , bool>();
            results.Add("update", true);
            return new JsonResult(results);
        }


        [HttpPost("addPayCart")]

        public IActionResult addPayCart([FromBody] AddPayCart list)
        {
            var listFail = new List<string>();
            var listSuccess = new List<string>();
            var listPay = new AddPayCart
            {
                getUser = list.getUser,
                listProductPay = list.listProductPay
            };
            Console.WriteLine(listPay.listProductPay[1].size);
            var connect = new ConnectServer();
            var tokenRequest = new MiddlewareToken();
            if (Request.Headers.TryGetValue("token", out var value) && Request.Headers.TryGetValue("refreshtoken", out var value2))
            {
                tokenRequest.token = value;
                tokenRequest.refreshtoken = value2;
            }
            var checkAuth = new MiddlewareAuth();
            string AuthToken = checkAuth.checkTokenLife(tokenRequest.token, tokenRequest.refreshtoken);
            if (AuthToken.Equals(config["ErrorToken"]) == true)
            {
                Dictionary<string, bool> resulst = new Dictionary<string, bool>();
                resulst.Add("checkAuth", false);
                return new JsonResult(resulst);
            }
            string idUser = AuthToken;
            for(int i=0 ; i<listPay.listProductPay.Length; i++)
            {
                MySqlConnection conn = connect.Connect();
                
                var new_pay = new getProduct
                {
                    IdProduct = listPay.listProductPay[i].IdProduct,
                    size = listPay.listProductPay[i].size,
                    quantity = listPay.listProductPay[i].quantity,
                    NameProduct = listPay.listProductPay[i].NameProduct
                };

                conn.Open();
                MySqlCommand checkSize = new MySqlCommand($"SELECT * FROM Size WHERE Idproduct='{new_pay.IdProduct}' and {new_pay.size} >= {new_pay.quantity}", conn);
                MySqlDataReader reader = checkSize.ExecuteReader();
                var check = new CheckSize();
                while (reader.Read())
                {

                    check.IdSize = reader["IdSize"].ToString();
                    check.quantitySize = Convert.ToInt32(reader[new_pay.size]);
                    
                }
                conn.Close();
                if(check.IdSize == null)
                {
                    listFail.Add(new_pay.NameProduct);
                }
                else
                {
                    conn.Open();
                    
                    string IdPay = System.Guid.NewGuid().ToString();
                    DateTime today = DateTime.Now;
                    string sqlInsert = $"INSERT INTO Pay(IdPay, IdUser, NameReceive, Phone, AddressReceive, IdProduct, size, quantity, statusPay, datePay)" +
                        $"values('{IdPay}', '{idUser}', '{list.getUser.NameReceive}', '{list.getUser.Phone}', '{list.getUser.AddressReceive}', '{new_pay.IdProduct}', '{new_pay.size}', '{new_pay.quantity}', '{config["StatusDefault"]}','{today}')";
                    string sqlUpdate = $"UPDATE Size SET WHERE {new_pay.size} = {check.quantitySize - new_pay.quantity}";
                    MySqlCommand insertBill = new MySqlCommand(sqlInsert, conn);
                    insertBill.ExecuteNonQueryAsync();
                    
                    MySqlCommand updateSize = new MySqlCommand(sqlUpdate, conn);
                    updateSize.ExecuteNonQueryAsync();
                    
                    listSuccess.Add(new_pay.NameProduct);
                    conn.Close();
                }
                conn.Close();
            }
            Dictionary<string, object> results = new Dictionary<string, object>();
            results.Add("success", listSuccess);
            results.Add("fail", listFail);
            return new JsonResult(results);
        }
        

        [HttpGet("getBillAdmin")]
        public IActionResult getBillAdmin(string status)
        {
            var ListBill = new List<Dictionary<string, object>>();
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            string sqlGetBill = $"SELECT * FROM Pay join Product ON Pay.IdProduct = Product.IdProduct WHERE statusPay='{status}'";
            MySqlCommand getBillAdmin = new MySqlCommand(sqlGetBill, conn);
            MySqlDataReader reader = getBillAdmin.ExecuteReader();
            while (reader.Read())
            {
                Dictionary<string, object> Bill = new Dictionary<string, object>();
                var user = new getUserPay
                {
                    NameReceive = reader["NameReceive"].ToString(),
                    Phone = reader["Phone"].ToString(),
                    AddressReceive = reader["AddressReceive"].ToString()
                };
                var products = new customProduct
                {
                    IdProduct = reader["IdProduct"].ToString(),
                    nameProduct = reader["nameProduct"].ToString(),
                    brand = reader["brand"].ToString(),
                    picture = reader["picture"].ToString(),
                    cost = Convert.ToDouble(reader["cost"]),
                    discount = Convert.ToInt32(reader["discount"])
                };
                Bill.Add("idBill", reader["IdPay"].ToString());
                Bill.Add("user", user);
                Bill.Add("product", products);
                Bill.Add("quantity", Convert.ToInt32(reader["quantity"]));
                Bill.Add("size", reader["size"].ToString());
                Bill.Add("time", DateTime.Parse(reader["datePay"].ToString()).ToString("HH:mm:ss"));
                Bill.Add("date", DateTime.Parse(reader["datePay"].ToString()).ToString("dd/MM/yyyy"));
                Bill.Add("status", reader["statusPay"].ToString());
                ListBill.Add(Bill);
            }

            return new JsonResult(ListBill);
        }
    }
}
