namespace OrderManagementApi.Dtos;

public record AddOrderItemRequest(
    string ProductName,
    int Quantity,
    decimal UnitPrice
);