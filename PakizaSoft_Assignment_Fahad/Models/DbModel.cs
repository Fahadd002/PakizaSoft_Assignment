using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace PakizaSoft_Assignment_Fahad.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        [Required(ErrorMessage = "Customer Name is required"), StringLength(50), DisplayName("Customer Name")]
        public string CustomerName { get; set; } = default!;
        [Required(ErrorMessage = "Phone Number is required"), StringLength(20)]
        public string Phone { get; set; } = default!;
        [Required(ErrorMessage = "Address is required"), StringLength(150)]
        public string Address { get; set; } = default!;
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    }
    public class Order
    {
        public int OrderId { get; set; }
        public int OrderNo { get; set; }

        [Required, Column(TypeName = "datetime2")]
        public DateTime OrderDate { get; set; }
        [Required, ForeignKey("Customer")]
        public int CustomerId { get; set; }
        [Required]
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
    public class Product
    {
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Product name is required"), StringLength(50), DisplayName("Product Name")]
        public string ProductName { get; set; } = default!;
        [Required(ErrorMessage = "Unit price is required"), Column(TypeName = "money"), DisplayName("Unit Price")]
        public decimal UnitPrice { get; set; }
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
    

    public class OrderItem
    {
        public int OrderItemId { get; set; }
        [Required(ErrorMessage = "Order field is required"), ForeignKey("Order")]
        public int OrderId { get; set; }
        [Required(ErrorMessage = "Product field is required"), ForeignKey("Product")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Quantity is required")]
        public int Quantity { get; set; }
        [Required, Column(TypeName ="money")]
        public decimal UnitPrice { get; set; }
        public virtual Order? Order { get; set; }
        public virtual Product? Product { get; set; }
    }
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<int>("Order_seq", schema: "dbo")
                .StartsAt(1)
                .IncrementsBy(1);
            modelBuilder.Entity<Order>(
                ).Property(x => x.OrderNo).HasDefaultValueSql("NEXT VALUE FOR dbo.Order_seq");
        }

    }

}
