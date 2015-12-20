using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketPlaceService
{
    public class PurchaseRepository : Repository
    {
        public PurchaseRepository(string connectionString)
            : base(connectionString)
        { }

        public async Task Purchase(Purchase purchase)
        {
            await ExecuteAsync(@"INSERT INTO [dbo].[Purchase] (
                    [Name], 
                    [Brand],
                    [Price],
                    [UserId],
                    [Time]
                ) 
                VALUES (
                    @Name, 
                    @Brand, 
                    @Price,
                    @UserId,
                    @Time
                );", purchase);
        }

        public async Task<IEnumerable<Purchase>> GetPurchaseHistory(string userId)
        {
            var sql = @"SELECT 
                            [Name], 
                            [Brand], 
                            [UserId], 
                            [Time],
                            [Price]
                        FROM [dbo].[Purchase]
                        WHERE [UserId] = @UserId";

            return await QueryAsync<Purchase>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<CarSales>> GetSalesStatistics()
        {
            var sql = @"SELECT 
                            [Name], 
                            [Brand], 
                            count(1) as [Count],
                            sum([Price]) as [TotalPrice]
                        FROM [dbo].[Purchase] as [p]
                        GROUP BY [Name], [Brand]
                        ORDER BY count(1) DESC";

            return await QueryAsync<CarSales>(sql);
        }
    }
}