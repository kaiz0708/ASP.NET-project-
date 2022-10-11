namespace WebAPI.Models
{
    public class getFeedback
    {
        public string IdProduct { get; set; }
        public string content { get; set; }

    }

    public class returnFeedback
    {
        public string IdMes { get; set; }
        public string IdUser { get; set; }
        public string IdProduct { get; set; }
        public string content { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public int like { get; set; }

    }

    public class updateFeedback
    {
        public string IdMes { get; set; }
        public string content { get; set; }

    }
}
