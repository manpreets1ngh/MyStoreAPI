public class ProcessPaymentResponseModel
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public Guid? CardId { get; set; }
    public string PaymentId { get; set; }
}
