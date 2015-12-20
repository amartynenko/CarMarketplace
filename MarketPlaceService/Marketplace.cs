using System;
using System.Linq;
using System.Threading.Tasks;

namespace MarketPlaceService
{
    public interface ICarService
    {
        Task<Car[]> SearchAsync(string brand);
        Task<Car> SearchAsync(string brand, string name);
        Task<bool> AddAsync(Car car);
        Task<bool> UpdateAsync(Car car);
        Task<Car[]> ListCarsAsync();
        Task<bool> DeleteAsync(string brand, string name);
    }

    public interface IMarketplace
    {
        Task<Car[]> ListCarsAsync();
        Task<Purchase> PurchaseAsync(string brand, string name, string userId);
        Task<Purchase[]> GetPurchaseHistoryAsync(string userId);
        Task<CarSales[]> GetSalesReportAsync();
    }

    public class Marketplace : ICarService, IMarketplace
    {
        private readonly CarRepository carRepository;
        private readonly PurchaseRepository purchaseRepository;

        public Marketplace(string connectionString)
        {
            purchaseRepository = new PurchaseRepository(connectionString);
            carRepository = new CarRepository(connectionString);
        }

        public async Task<Car[]> SearchAsync(string brand)
        {
            return (await carRepository.SearchAsync(brand))
                .ToArray();
        }

        public async Task<Car> SearchAsync(string brand, string name)
        {
            return await carRepository.SearchAsync(brand, name);
        }

        public async Task<bool> AddAsync(Car car)
        {
            return await carRepository.AddAsync(car);
        }

        public async Task<bool> UpdateAsync(Car car)
        {
            return await carRepository.UpdateAsync(car);
        }

        public async Task<Car[]> ListCarsAsync()
        {
            return (await carRepository.ListAll()).ToArray();
        }

        public async Task<bool> DeleteAsync(string brand, string name)
        {
            return await carRepository.DeleteAsync(brand, name);
        }

        public async Task<Purchase> PurchaseAsync(string brand, string name, string userId)
        {
            var car = await carRepository.SearchAsync(brand, name);
            if (car == null)
                return null;

            decimal price = car.Price;
            var purchase = new Purchase
            {
                Brand = brand,
                Name = name,
                UserId = userId,
                Time = DateTimeOffset.Now,
                Price = price
            };

            await purchaseRepository.Purchase(purchase);

            return purchase;
        }

        public async Task<Purchase[]> GetPurchaseHistoryAsync(string userId)
        {
            return purchaseRepository.GetPurchaseHistory(userId).Result
                .ToArray();
        }

        public async Task<CarSales[]> GetSalesReportAsync()
        {
            return purchaseRepository.GetSalesStatistics().Result
                .ToArray();
        }
    }
}