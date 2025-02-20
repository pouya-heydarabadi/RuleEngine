namespace RuleEngine.Domain.Users;


public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal Bonus { get; set; }

    public bool Verified { get; set; } = false;
    
    private User() { }

    // سازنده برای مقداردهی اولیه
    public User(string name, string email, decimal bonus)
    {
        Name = name;
        Email = email;
        CreatedAt = DateTime.UtcNow;
        Bonus = bonus;
    }

    public void Verify()
    {
        Verified = true;
    }

    // متد نمایش اطلاعات کاربر
    public override string ToString()
    {
        return $"User: {Id}, {Name}, {Email}, Created At: {CreatedAt}";
    }
}