using System;

namespace Messages.Models
{
    public class OrderViewModel
    {
        public double OrderAmount { get; set; }

        public string OrderNumber { get; set; }

        public DateTime OrderDate { get; set; }
    }
}
