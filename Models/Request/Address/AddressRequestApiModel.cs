namespace ApplicationToSellThings.APIs.Models
{
    public class AddressRequestApiModel
    {
        public string UserId { get; set; } // Foreign key for the user
        public string Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
    }
}
