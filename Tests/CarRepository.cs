using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class CarRepository : Repository
    {
        public CarRepository(string connectionString)
            : base(connectionString)
        { }

        public void Add(Car car)
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

            Execute(sql, car);
        }

        public IEnumerable<Car> Search(string brand)
        {
            var sql = @"SELECT 
                            [Name], 
                            [Brand], 
                            [Price] 
                        FROM [dbo].[Car]
                        WHERE [Brand] = @Brand";

            return Query<Car>(sql, new { Brand = brand });
        }

        public IEnumerable<Car> ListAll()
        {
            return Query<Car>(@"SELECT 
                                    [Name], 
                                    [Brand], 
                                    [Price] 
                                FROM [dbo].[Car]");
        }

        public Car Search(string brand, string name)
        {
            var sql = @"SELECT 
                            [Name], 
                            [Brand], 
                            [Price] 
                        FROM [dbo].[Car]
                        WHERE [Brand] = @Brand AND [Name] = @Name";

            return Query<Car>(sql, new { Brand = brand, Name = name })
                .FirstOrDefault();
        }
    }
}