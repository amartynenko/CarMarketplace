using System;
using System.Linq;

namespace Tests
{
    public class Marketplace
    {
        private readonly CarRepository carRepository;
        private readonly PurchaseRepository purchaseRepository;

        public Marketplace(string connectionString)
        {
            purchaseRepository = new PurchaseRepository(connectionString);
            carRepository = new CarRepository(connectionString);
        }

        public Car[] Search(string brand)
        {
            return carRepository.Search(brand)
                .ToArray();
        }

        public Car Search(string brand, string name)
        {
            return carRepository.Search(brand, name);
        }

        public void Add(Car car)
        {
            carRepository.Add(car);
        }

        public Car[] ListCars()
        {
            return carRepository.ListAll().ToArray();
        }

        public bool Purchase(string brand, string name, string userId)
        {
            var car = carRepository.Search(brand, name);
            if (car == null)
                return false;

            decimal price = car.Price;
            var purchase = new Purchase
            {
                Brand = brand,
                Name = name,
                UserId = userId,
                Time = DateTimeOffset.Now,
                Price = price
            };

            purchaseRepository.Purchase(purchase);

            return true;
        }

        public Purchase[] GetPurchaseHistory(string userId)
        {
            return purchaseRepository.GetPurchaseHistory(userId)
                .ToArray();
        }

        public CarSales[] GetSalesReport()
        {
            return purchaseRepository.GetSalesStatistics()
                .ToArray();
        }
    }
}