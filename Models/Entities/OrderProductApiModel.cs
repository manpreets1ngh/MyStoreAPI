namespace ApplicationToSellThings.APIs.Models;

public class OrderProductApiModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}