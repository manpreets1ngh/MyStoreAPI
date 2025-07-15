namespace ApplicationToSellThings.APIs.Models;

public class OrderDetailApiResponseModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
    public string ProductName { get; set; }
}