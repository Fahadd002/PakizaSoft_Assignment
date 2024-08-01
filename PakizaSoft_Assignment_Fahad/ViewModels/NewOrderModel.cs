using PakizaSoft_Assignment_Fahad.Models;

namespace PakizaSoft_Assignment_Fahad.ViewModels
{
    public class NewOrderModel
    {
        public Customer Customer { get; set; } = default!;
        public OrderItemViewModel OrderItem { get; set; } = default!;
    }
}
