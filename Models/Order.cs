namespace OrderManagementApi.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User{ get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public String Status { get; set; } = "Draft";

    public List<OrderItem> Items { get; set; } = new();
}