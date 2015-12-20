using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace MarketPlaceService
{
    public class CarRepository : Repository
    {
        public CarRepository(string connectionString)
            : base(connectionString)
        { }

        public async Task<bool> AddAsync(Car car)
        {
            var sql = @"INSERT INTO [dbo].[Car] (
                            [Name], 
                            [Brand],
                            [Price]
                        ) 
                        VALUES (
                            @Name, 
                            @Brand, 
                            @Price
                        );";
            try
            {
                await ExecuteAsync(sql, car);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    return false;
                }
                throw;
            }

            return true;
        }

        public async Task<bool> UpdateAsync(Car car)
        {
            var sql = @"UPDATE [dbo].[Car] 
                        SET [Price] = @Price
                        WHERE [Brand] = @Brand AND [Name] = @Name

                        SELECT @@ROWCOUNT";

            var rows = await ExecuteAsync(sql, car);
            return rows > 0;
        }

        public async Task<IEnumerable<Car>> SearchAsync(string brand)
        {
            var sql = @"SELECT 
                            [Name], 
                            [Brand],
                            [Price] 
                        FROM [dbo].[Car]
                        WHERE [Brand] = @Brand";

            return await QueryAsync<Car>(sql, new { Brand = brand });
        }

        public async Task<IEnumerable<Car>> ListAll()
        {
            return await QueryAsync<Car>(@"SELECT 
                                            [Name], 
                                            [Brand], 
                                            [Price] 
                                        FROM [dbo].[Car]");
        }

        public async Task<Car> SearchAsync(string brand, string name)
        {
            var sql = @"SELECT 
                            [Name], 
                            [Brand], 
                            [Price] 
                        FROM [dbo].[Car]
                        WHERE [Brand] = @Brand AND [Name] = @Name";

            var searchResult = await QueryAsync<Car>(sql, new { Brand = brand, Name = name });

            return searchResult.FirstOrDefault();
        }

        public async Task<bool> DeleteAsync(string brand, string name)
        {
            var sql = @"DELETE FROM [dbo].[Car]
                        WHERE [Brand] = @Brand AND [Name] = @Name

                        SELECT @@ROWCOUNT";

            var rows = await ExecuteAsync(sql, new { Brand = brand, Name = name });
            if (rows > 0)
                return true;

            return false;
        }
    }
}