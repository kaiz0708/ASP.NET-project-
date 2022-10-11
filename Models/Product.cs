namespace WebAPI.Models
{
    public class Products
    {
        public string IdProduct { get; set; }
        public string nameProduct { get; set; }
        public string brand { get; set; }
        public string picture { get; set; }
        public double cost { get; set; }
        public int discount { get; set; }
    }

    public class InforBasicProduct
    {
        public string nameProduct { get; set; }
        public string brand { get; set; }
        public string picture { get; set; }
        public double cost { get; set; }
        public int discount { get; set; }
    }

    public class Title
    {
        public string category { get; set; }
        public string typeProduct { get; set; }
    }

    public class SizeAndColor
    {
        public int sizeM { get; set; }
        public int sizeL { get; set; }
        public int sizeXL { get; set; }
        public string color { get; set; } 

    }

    public class Describe
    {
        public string material { get; set; }
        public string form { get; set; }
        public string fit { get; set; }

    }

    public class GetProductUpdate
    {
        public InforBasicProduct InforBasic { get; set; }
        public Title Title { get; set; }
        public SizeAndColor SizeAndColor { get; set; }
        public Describe describe { get; set; }
    }
}
