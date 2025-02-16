using RuleEngine.Domain.Products;

namespace RuleEngine.Domain.Orders;

public class OrderItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Order Order { get; set; }

    private OrderItem() { }
    public OrderItem(int productId, string productName, decimal price, int quantity, Order order)
    {
        ProductId = productId;
        ProductName = productName;
        Price = price;
        Quantity = quantity;
        Order = order;
    }
}