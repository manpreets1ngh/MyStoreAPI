public class ProcessPaymentRequestModel
{
    public string UserId { get; set; }
    public string Nonce { get; set; }
    public int Amount { get; set; } 
    public bool SaveCard { get; set; }
}
