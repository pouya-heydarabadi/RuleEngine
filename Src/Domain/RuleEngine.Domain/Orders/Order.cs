using RuleEngine.Domain.Users;

namespace RuleEngine.Domain.Orders;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; } // شناسه کاربر
    public User User { get; set; }
    public List<OrderItem> Items { get; set; } 
    public decimal TotalAmount { get; private set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // سازنده پیش‌فرض
    public Order() { }

    // سازنده مقداردهی اولیه
    public Order(int id, User user, List<OrderItem> items)
    {
        Id = id;
        UserId = user.Id;
        User = user;
        Items = items ?? new List<OrderItem>();
        CalculateTotal();
    }

    // متد محاسبه مجموع مبلغ سفارش
    public void CalculateTotal()
    {
        TotalAmount = Items.Sum(item => item.Price * item.Quantity);
    }

    // تغییر وضعیت سفارش
    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
    }

    public override string ToString()
    {
        return $"Order: {Id}, User: {UserId}, Total: {TotalAmount:C}, Status: {Status}, Date: {OrderDate}";
    }
}

// کلاس آیتم سفارش

// وضعیت‌های سفارش
public enum OrderStatus
{
    Pending,    // در انتظار پردازش
    Processing, // در حال پردازش
    Shipped,    // ارسال شده
    Delivered,  // تحویل داده شده
    Canceled    // لغو شده
}