using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OrderManagementApi.Models;
using System.Security.Claims;
using OrderManagementApi.Dtos;



var builder = WebApplication.CreateBuilder(args);
var jwtKey = "SUPER_SECRET_DEV_KEY_CHANGE_LATER_123456789";



// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<PasswordHasher>();

builder.Services.AddSingleton(new JwtTokenService(jwtKey));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source = orders.db"));



builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(); // 👈 THIS LINE


var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapGet("/", () => "Order Management API is running");

app.MapPost("/auth/register", async (
    RegisterRequest request,
    AppDbContext db,
    PasswordHasher hasher) =>
{
    var exists = await db.Users
        .AnyAsync(u => u.Email == request.Email);

    if (exists)
        return Results.BadRequest("Email already registered");

    var user = new User
    {
        Email = request.Email,
        PasswordHash = hasher.Hash(request.Password)
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok();
});

app.MapPost("/auth/login", async (
    LoginRequest request,
    AppDbContext db,
    PasswordHasher hasher,
    JwtTokenService jwt) =>
{
    var user = await db.Users
        .FirstOrDefaultAsync(u => u.Email == request.Email);

    if (user is null ||
        !hasher.Verify(user.PasswordHash, request.Password))
        return Results.Unauthorized();

    var token = jwt.GenerateToken(user);
    return Results.Ok(new { token });
});

app.MapGet("/me", (ClaimsPrincipal user) =>
{
    var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var email = user.FindFirstValue(ClaimTypes.Email);

    return Results.Ok(new { id, email });
})
.RequireAuthorization();


static int GetUserId(ClaimsPrincipal user)
{
    return int.Parse(
        user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

app.MapPost("/orders", async (
    ClaimsPrincipal user,
    AppDbContext db) =>
{
    var userId = GetUserId(user);

    var order = new Order
    {
        UserId = userId
    };

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    return Results.Ok(order);
})
.RequireAuthorization();


app.MapPost("/orders/{orderId}/items", async (
    int orderId,
    AddOrderItemRequest request,
    ClaimsPrincipal user,
    AppDbContext db) =>
{
    var userId = GetUserId(user);

    var order = await db.Orders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

    if (order is null)
        return Results.NotFound("Order not found");

    if (order.Status != "Draft")
        return Results.BadRequest("Cannot add items to a non-draft order");

    var item = new OrderItem
    {
        ProductName = request.ProductName,
        Quantity = request.Quantity,
        UnitPrice = request.UnitPrice,
        Order = order
    };

    db.OrderItems.Add(item);
    await db.SaveChangesAsync();

    return Results.Ok(order);
})
.RequireAuthorization();

app.MapGet("/orders/{orderId}", async (
    int orderId,
    ClaimsPrincipal user,
    AppDbContext db) =>
{
    var userId = GetUserId(user);

    var order = await db.Orders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

    if (order is null)
        return Results.NotFound();

    return Results.Ok(order);
})
.RequireAuthorization();

app.MapPost("/orders/{orderId}/submit", async (
    int orderId,
    ClaimsPrincipal user,
    AppDbContext db) =>
{
    var userId = GetUserId(user);

    var order = await db.Orders
        .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

    if (order is null)
        return Results.NotFound();

    if (order.Status != "Draft")
        return Results.BadRequest("Order already submitted");

    if (!await db.OrderItems.AnyAsync(i => i.OrderId == order.Id))
        return Results.BadRequest("Order must contain at least one item");

    order.Status = "Submitted";
    await db.SaveChangesAsync();

    return Results.Ok(order);
})
.RequireAuthorization();

app.MapGet("/orders/{orderId}/items", async (
    int orderId,
    ClaimsPrincipal user,
    AppDbContext db) =>
{
    var userId = GetUserId(user);

    var order = await db.Orders
        .Include(o => o.Items)
        .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

    if (order is null)
        return Results.NotFound("Order not found");

    return Results.Ok(order.Items);
})
.RequireAuthorization();




app.MapGet("/orders", async (
    ClaimsPrincipal user,
    AppDbContext db) =>
{
    var userId = GetUserId(user);

    var orders = await db.Orders
        .Where(o => o.UserId == userId)
        .OrderByDescending(o => o.CreatedAt)
        .ToListAsync();

    return Results.Ok(orders);
})
.RequireAuthorization();



app.Run();

