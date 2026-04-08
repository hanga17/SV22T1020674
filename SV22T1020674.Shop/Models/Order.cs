using System;

namespace SV22T1020674.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime OrderTime { get; set; }
        public string Status { get; set; }
    }
}