namespace RuleEngine.Domain.Products;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal LimitBonus { get; set; } = 0;

    // سازنده پیش‌فرض
    public Product() { }

    // سازنده مقداردهی اولیه
    public Product(int id, string name, string description, decimal price, int stockQuantity, decimal limitBonus)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        CreatedAt = DateTime.UtcNow;
        LimitBonus = limitBonus;
    }

    // متد نمایش اطلاعات محصول
    public override string ToString()
    {
        return $"Product: {Id}, {Name}, {Price:C}, Stock: {StockQuantity}, Created At: {CreatedAt}";
    }

    // متد کاهش موجودی
    public bool ReduceStock(int quantity)
    {
        if (quantity > StockQuantity)
        {
            return false; // موجودی کافی نیست
        }
        StockQuantity -= quantity;
        return true;
    }

    // متد افزایش موجودی
    public void IncreaseStock(int quantity)
    {
        StockQuantity += quantity;
    }
}