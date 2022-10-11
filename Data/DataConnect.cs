using MySql.Data.MySqlClient;
namespace WebAPI.Data
{
    public class ConnectServer
    {
        IConfiguration config = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddEnvironmentVariables()
                                .Build();
        public MySqlConnection Connect()
        {
            MySqlConnection connect;
           
            string connectString = "server=localhost;database=shop;uid=root;password=kyanh0708";
            connect = new MySqlConnection(connectString);
            try
            {
                connect.Open();
                Console.WriteLine("Success");
                connect.Close();
            }
            catch
            {
                Console.WriteLine(config["ConncetServer"]);
                Console.WriteLine("Gail");
            }
            return connect;
        }
    }
}
