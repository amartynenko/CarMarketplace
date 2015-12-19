using System;

namespace Tests
{
    public class Car
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
    }

    public class Purchase
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public string UserId { get; set; }
        public DateTimeOffset Time { get; set; }
    }

    public class CarSales
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public int Count { get; set; }
        public decimal TotalPrice { get; set; }
    }
}