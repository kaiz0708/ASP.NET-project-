using WebAPI.TokenAuth;
using WebAPI.Models;
namespace WebAPI.Models
{
    public class UserGet
    {
        public string UserId { get; set; }
        public string account { get; set; }
        public string pass { get; set; }
        public string typeEmployee { get; set; }
    }

    public class UserResponseAuth
    {
        public string username { get; set; }
        public string avatar { get; set; }
    }

    public class GetUserForClient
    {
        
        public string username { get; set; }
        public string name { get; set; }
        public string date_of_birth { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
       

    }

    public class ReturnInforUser : GetUserForClient
    {
        public string avatar { get; set; }
    }

    public class GetUserForAdmin : GetUserForClient
    {
        public string IdUser { get; set; }
        public string account { set; get; }
    }
    
}
