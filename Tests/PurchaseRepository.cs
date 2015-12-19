using System.Collections.Generic;

namespace Tests
{
    public class PurchaseRepository : Repository
    {
        public PurchaseRepository(string connectionString)
            : base(connectionString)
        { }

        public void Purchase(Purchase purchase)
        {
            Execute(@"INSERT INTO [dbo].[Purchase] (
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

        public IEnumerable<Purchase> GetPurchaseHistory(string userId)
        {
            var sql = @"SELECT 
                            [Name], 
                            [Brand], 
                            [UserId], 
                            [Time],
                            [Price]
                        FROM [dbo].[Purchase]
                        WHERE [UserId] = @UserId";

            return Query<Purchase>(sql, new { UserId = userId });
        }

        public IEnumerable<CarSales> GetSalesStatistics()
        {
            var sql = @"SELECT 
                            [Name], 
                            [Brand], 
                            count(1) as [Count],
                            sum([Price]) as [TotalPrice]
                        FROM [dbo].[Purchase] as [p]
                        GROUP BY [Name], [Brand]
                        ORDER BY count(1) DESC";

            return Query<CarSales>(sql);
        }
    }
}