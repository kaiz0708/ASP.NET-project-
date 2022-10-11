namespace WebAPI.Models
{
    public class Pay
    {
        public string IdProduct { get; set; }
        public string NameReceive { get; set; }
        public string Phone { get; set; }
        public string AddressReceive { get; set; }
        public int quantity { get; set; }
        public string size { get; set; }

    }

    public class getUserPay
    {
        public string NameReceive { get; set; }
        public string Phone { get; set;}
        public string AddressReceive { get; set; }

    }

    public class getProduct
    {
        public string IdProduct { get; set;}

        public string NameProduct { get; set; }

        public string size { get; set; }

        public int quantity { get; set; }
    }

    public class customProduct
    {
        public string IdProduct { get; set; }
        public string nameProduct { get; set; }
        public string brand { get; set; }
        public string picture { get; set; } 
        public double cost { get; set; }
        public int discount { get; set; }

    }

    public class DeletePay
    {
        public string IdPay { get; set; }
        public string IdProduct { get; set; }
        public int quantity { get; set; }
        public string size { get; set; }

    }

    public class AddPayCart
    {
        public getUserPay getUser { get; set; }

        public getProduct[] listProductPay { get; set; }
    }
}
