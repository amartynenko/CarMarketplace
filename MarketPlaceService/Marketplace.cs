using System;
using System.Linq;
using System.Threading.Tasks;

namespace MarketPlaceService
{
    public interface ICarService
    {
        Task<Car[]> SearchAsync(string brand);
        Task<Car> SearchAsync(string brand, string name);
        Task<bool> Add(Car car);
        Task<bool> Update(Car car);
        Task<Car[]> ListCars();
        Task<bool> Delete(string brand, string name);
    }

    public class Marketplace : ICarService
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

        public async Task<bool> Add(Car car)
        {
            return await carRepository.AddAsync(car);
        }

        public async Task<bool> Update(Car car)
        {
            return await carRepository.UpdateAsync(car);
        }

        public async Task<Car[]> ListCars()
        {
            return (await carRepository.ListAll()).ToArray();
        }

        public async Task<bool> Delete(string brand, string name)
        {
            return await carRepository.DeleteAsync(brand, name);
        }

        public async Task<bool> PurchaseAsync(string brand, string name, string userId)
        {
            var car = await carRepository.SearchAsync(brand, name);
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

            await purchaseRepository.Purchase(purchase);

            return true;
        }

        public Purchase[] GetPurchaseHistory(string userId)
        {
            return purchaseRepository.GetPurchaseHistory(userId).Result
                .ToArray();
        }

        public CarSales[] GetSalesReport()
        {
            return purchaseRepository.GetSalesStatistics().Result
                .ToArray();
        }
    }
}