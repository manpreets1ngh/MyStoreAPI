namespace ApplicationToSellThings.APIs.Models
{
    public class CardResponseApiModel
    {
        public Guid CardId { get; set; }
        public string CardHolderName { get; set; }
        public string Last4Digits  { get; set; }
        public string CardBrand { get; set; }
        public DateTime AddedOn { get; set; }
    }
}
