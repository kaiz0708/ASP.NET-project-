using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Controllers.Product
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        IConfiguration config = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddEnvironmentVariables()
                                .Build();

        private readonly ILogger<ProductController> _logger;
        public ProductController(ILogger<ProductController> logger)
        {
            _logger = logger;
        }

        [HttpGet("product")]

        public IActionResult ReturnProduct(string category, string type, string page)
        {
            string res;
            string TableString;
            string ConditionString;
            string ColumnString;
            string condition;
            string tablecondition;
            if (type.Equals("error") == true)
            {
                res = category;
                TableString = "Category";
                ColumnString = "category";
                ConditionString = "IdCategory";
                condition = "";
                tablecondition = "";
            }
            else
            {
                res = type;
                TableString = "TypeProduct";
                ColumnString = "typeProduct";
                ConditionString = "IdTypeProduct";
                condition = "and";
                tablecondition = $"Product.category = {category}";
            }
            int pageNum = Convert.ToInt32(page);
            int pageSize = Convert.ToInt32(config["PageSize"]);
            List<Products> products = new List<Products>();
            int quantityPage = 0;
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            MySqlCommand getProduct = new MySqlCommand($"SELECT IdProduct, nameProduct, brand, picture, cost, discount FROM Product join {TableString} on Product.{ColumnString} = {TableString}.{ConditionString} where {TableString}.Title='{res}' {condition} {tablecondition} limit {pageSize} offset {pageSize * pageNum - pageSize}", conn);
            MySqlDataReader reader = getProduct.ExecuteReader();
            while (reader.Read())
            {
                var product = new Products
                {
                    IdProduct = reader["IdProduct"].ToString(),
                    nameProduct = reader["nameProduct"].ToString(),
                    brand = reader["brand"].ToString(),
                    picture = reader["picture"].ToString(),
                    cost = Convert.ToDouble(reader["cost"]),
                    discount = Convert.ToInt32(reader["discount"]),
                };
                quantityPage++;
                products.Add(product);
            };
            Dictionary<string, object> results = new Dictionary<string, object>();
            results.Add("infor_product", products);
            results.Add("quantityPage", quantityPage);
            results.Add("pageCurrent", pageNum);
            return new JsonResult(results);
        }


        [HttpGet("product/sub")]

        public IActionResult ReturnProductId(string idProduct)
        {
            var ListSize = new List<Size>();
            Dictionary<string, object> resultsData = new Dictionary<string, object>();
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            MySqlCommand getInforProuductId = new MySqlCommand($"SELECT * FROM Product join Size on " +
                $"Product.size = Size.IdSize join DescribeProduct on Product.describeProduct = DescribeProduct.idDescribe where Product.IdProduct={idProduct}", conn);
            MySqlDataReader reader = getInforProuductId.ExecuteReader();
            while (reader.Read())
            {
                var infor_product = new Products
                {
                    IdProduct = reader["IdProduct"].ToString(),
                    nameProduct = reader["nameProduct"].ToString(),
                    brand = reader["brand"].ToString(),
                    picture = reader["picture"].ToString(),
                    cost = Convert.ToDouble(reader["cost"]),
                    discount = Convert.ToInt32(reader["discount"])
                };
                var sizeM = new Size
                {
                    nameSize = "M",
                    quantity = Convert.ToInt32(reader["M"]),
                };
                var sizeL = new Size
                {
                    nameSize = "L",
                    quantity = Convert.ToInt32(reader["L"]),
                };
                var sizeXL = new Size
                {
                    nameSize = "XL",
                    quantity = Convert.ToInt32(reader["XL"])
                };
                ListSize.Add(sizeM);
                ListSize.Add(sizeL);
                ListSize.Add(sizeXL);
                resultsData.Add("infor_product", infor_product);
                resultsData.Add("Size", ListSize);
                resultsData.Add("material", reader["material"]);
                resultsData.Add("form", reader["form"]);
                resultsData.Add("fit", reader["fit"]);
                resultsData.Add("color", reader["color"]);
            }
            conn.Close();
            return new JsonResult(resultsData);
        }

        [HttpGet("getProductHomePage")]

        public IActionResult ReturnProductHomePage()
        {
            var listProductPage = new List<Products>();
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            MySqlCommand getProductHomePage = new MySqlCommand($"SELECT * FROM Product Limit 4", conn);
            MySqlDataReader reader = getProductHomePage.ExecuteReader();
            while (reader.Read())
            {
                var product = new Products
                {
                    IdProduct = reader["IdProduct"].ToString(),
                    nameProduct = reader["nameProduct"].ToString(),
                    brand = reader["brand"].ToString(),
                    picture = reader["picture"].ToString(),
                    cost = Convert.ToDouble(reader["cost"]),
                    discount = Convert.ToInt32(reader["discount"])
                };
                listProductPage.Add(product);
            }
            conn.Close();
            return new JsonResult(listProductPage);
        }


        [HttpPost("admin/addProduct")]

        public IActionResult addProductAdmin([FromBody] GetProductUpdate Product)
        {
            var connect = new ConnectServer();
            MySqlConnection conn = connect.Connect();
            conn.Open();
            string idProduct = System.Guid.NewGuid().ToString();
            string idSize = System.Guid.NewGuid().ToString();
            string idDescribeProduct = System.Guid.NewGuid().ToString();
            string sqlAddProduct = $"INSERT INTO Product(IdProduct, nameProduct, brand, picture, cost, discount, category, typeProduct, size, color, describeProduct)" +
                $"values('{idProduct}', '{Product.InforBasic.nameProduct}', '{Product.InforBasic.brand}', '{Product.InforBasic.picture}', '{Product.InforBasic.cost}', '{Product.InforBasic.discount}','{Product.Title.category}', '{Product.Title.typeProduct}', '{idSize}','{Product.SizeAndColor.color}', '{idDescribeProduct}')";
            string sqlAddDescribe = $"INSERT INTO DescribeProduct(idDescribe, material, form, fit)" +
                $"values('{idDescribeProduct}', '{Product.describe.material}', '{Product.describe.form}', '{Product.describe.fit}')";
            string sqlAddSize = $"INSERT INTO Size(IdSize, Idproduct, M,L,XL)" +
                $"values('{idSize}', '{idProduct}', '{Product.SizeAndColor.sizeM}', '{Product.SizeAndColor.sizeL}', '{Product.SizeAndColor.sizeXL}')";
            MySqlCommand addSize = new MySqlCommand(sqlAddSize, conn);
            addSize.ExecuteNonQuery();
            MySqlCommand addDescribe = new MySqlCommand(sqlAddDescribe, conn);
            addDescribe.ExecuteNonQuery();
            MySqlCommand addProduct = new MySqlCommand(sqlAddProduct, conn);
            addProduct.ExecuteNonQuery();
            Dictionary<string, object> results = new Dictionary<string, object>();
            results.Add("resultsAdd", true);
            conn.Close();
            return new JsonResult(results);
        }
    }
}
