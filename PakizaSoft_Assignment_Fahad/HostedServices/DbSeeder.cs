using PakizaSoft_Assignment_Fahad.Models;

namespace PakizaSoft_Assignment_Fahad.HostedServices
{
    public class DbSeeder
    {
        private readonly ProductDbContext db;
        public DbSeeder(ProductDbContext db)
        {
            this.db = db;
        }
        public async Task SeedAsync()
        {
            if (!await db.Database.CanConnectAsync())
            {
                await db.Database.EnsureCreatedAsync();

            }
            if (!db.Customers.Any())
            {
                await db.Customers.AddRangeAsync(new Customer[]
                {
                        new Customer {CustomerName="Mr. Rahat Ahmed",Phone="123456789",Address="Farmgate" },
                        new Customer {CustomerName="Mr. Aowal Azad",Phone="123456789",Address="Gulshan"},
                });
                await db.Products.AddRangeAsync(new Product[]
                {
                        new Product {ProductName="Book",UnitPrice=100.00M},
                        new Product {ProductName="Pen",UnitPrice=20.00M},
                        new Product {ProductName="Calculator",UnitPrice=1000.00M},
                });
                await db.Orders.AddRangeAsync(new Order[]
                {
                     new Order{ OrderDate=DateTime.Now, CustomerId=1}
                });


                await db.SaveChangesAsync();
            }


        }
    }

}
