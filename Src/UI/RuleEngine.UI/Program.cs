using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using RuleEngine.Domain.Orders;
using RuleEngine.Domain.Products;
using RuleEngine.Domain.Users;
using RuleEngine.Infrastructure.Configs.DbContexts;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://192.168.11.18:5000");

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.MaxDepth = 5;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.IgnoreNullValues = true;
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddOpenApi();


builder.Services.AddDbContext<AppDbContext>(options=>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

#region Products

// دریافت همه محصولات
app.MapGet("/products", async (AppDbContext db) =>
    await db.Products.ToListAsync());

// دریافت محصول با شناسه مشخص
app.MapGet("/products/{id}", async (int id, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

// ایجاد محصول جدید با DTO
app.MapPost("/products", async (ProductDto productDto, AppDbContext db) =>
{
    var product = new Product
    {
        Name = productDto.Name,
        Description = productDto.Description,
        Price = productDto.Price,
        StockQuantity = productDto.StockQuantity,
        LimitBonus = productDto.LimitBonus
    };

    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{product.Id}", product);
});

// به‌روزرسانی محصول با DTO
app.MapPut("/products/{id}", async (int id, ProductDto updatedProductDto, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    product.Name = updatedProductDto.Name;
    product.Description = updatedProductDto.Description;
    product.Price = updatedProductDto.Price;
    product.StockQuantity = updatedProductDto.StockQuantity;
    product.LimitBonus = updatedProductDto.LimitBonus;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// حذف محصول
app.MapDelete("/products/{id}", async (int id, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
#endregion

#region Users


// دریافت همه کاربران
app.MapGet("/users", async (AppDbContext db) =>
    await db.Users.ToListAsync());

// دریافت کاربر با شناسه مشخص
app.MapGet("/users/{id}", async (int id, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
});

// ایجاد کاربر جدید با DTO
app.MapPost("/users", async (UserDto userDto, AppDbContext db) =>
{
    var user = new User (userDto.Name, userDto.Email, userDto.Bonus); // `Id` مقداردهی نمی‌شود
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{user.Id}", user);
});

// به‌روزرسانی کاربر با DTO
app.MapPut("/users/{id}", async (int id, UserDto updatedUserDto, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    user.Name = updatedUserDto.Name;
    user.Email = updatedUserDto.Email;
    user.Bonus = updatedUserDto.Bonus;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// حذف کاربر
app.MapDelete("/users/{id}", async (int id, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

#endregion

#region Orders


// دریافت همه سفارش‌ها
app.MapGet("/orders", async (AppDbContext db) =>
    await db.Orders.Include(o => o.Items).ToListAsync());

// دریافت سفارش با شناسه
app.MapGet("/orders/{id}", async (int id, AppDbContext db) =>
{
    var order = await db.Orders.Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == id);
    return order is not null ? Results.Ok(order) : Results.NotFound();
});

// ایجاد سفارش جدید
app.MapPost("/orders", async (OrderDto orderDto, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(orderDto.UserId);
    if (user is null) return Results.BadRequest("User not found");

    var items = new List<OrderItem>();

    foreach (var itemDto in orderDto.Items)
    {
        var product = await db.Products.FindAsync(itemDto.ProductId);
        if (product is null) return Results.BadRequest($"Product {itemDto.ProductId} not found");
        if (product.StockQuantity < itemDto.Quantity) return Results.BadRequest($"Insufficient stock for product {product.Name}");

        var orderItem = new OrderItem(itemDto.ProductId, product.Name, product.Price, itemDto.Quantity, null);
        items.Add(orderItem);

        product.ReduceStock(itemDto.Quantity); // کاهش موجودی
    }

    var order = new Order(0, user, items);
    db.Orders.Add(order);
    await db.SaveChangesAsync();
    return Results.Created($"/orders/{order.Id}", order);
});

// تغییر وضعیت سفارش
app.MapPut("/orders/{id}/status", async (int id, OrderStatus status, AppDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order is null) return Results.NotFound();

    order.UpdateStatus(status);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// حذف سفارش
app.MapDelete("/orders/{id}", async (int id, AppDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order is null) return Results.NotFound();

    db.Orders.Remove(order);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


app.MapPost("/orders/{orderId}/items", async (int orderId, OrderItemDto newItem, AppDbContext dbContext) =>
{
    var order = await dbContext.Orders
        .Include(x=>x.User)
        .Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
    
    
    
    #region Rule Engine

    object orderItemForRuleEngine = new
    {
        orderId = orderId,
        productId = newItem.ProductId,
        quantity = newItem.Quantity,
        userId = order.User.Id
    };
    
    var options = new RestClientOptions("http://localhost:5678");
    var client = new RestClient(options);
    var request = new RestRequest("/webhook/RuleEngine", Method.Post);
    request.AddHeader("Content-Type", "application/json");
    request.AddJsonBody(orderItemForRuleEngine);

    var response = await client.ExecuteAsync(request);

    if (response.Content is not null)
    {
        ResponseDto responseContent = JsonSerializer.Deserialize<ResponseDto>(response.Content); 
        if (responseContent.Response.ToLower() != "ok")
            return Results.BadRequest(responseContent.Response);
        else if (responseContent.Response.ToLower() == "Error in workflow".ToLower())
            return Results.BadRequest(responseContent.Response);
    }
    #endregion
    
    
    if (order == null)
        return Results.NotFound($"Order with ID {orderId} not found.");

    var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == newItem.ProductId);
    if (product == null)
        return Results.NotFound($"Product with ID {newItem.ProductId} not found.");

    var orderItem = new OrderItem(newItem.ProductId, product.Name, product.Price, newItem.Quantity, order);
    order.Items.Add(orderItem);

    order.CalculateTotal();
    
    await dbContext.OrderItems.AddAsync(orderItem);
    await dbContext.SaveChangesAsync(); // ذخیره تغییرات در دیتابیس

    return Results.Created();
});

#endregion

app.Run();

public class ProductDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public decimal LimitBonus { get; set; } = 0;
}

public class UserDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public decimal Bonus { get; set; }
}

public class OrderDto
{
    public int UserId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class ResponseDto
{
    public string Response { get; set; }
}