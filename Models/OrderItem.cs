namespace OrderManagementApi.Models;
using System.Text.Json.Serialization;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    [JsonIgnore]
    public Order Order { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}