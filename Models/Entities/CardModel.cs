using System.ComponentModel.DataAnnotations;

namespace MyStoreAPI.Models
{
    public class CardModel
    {
        [Key]
        public Guid CardId { get; set; }

        // Stores the owning user's id. No EF navigation/FK is configured here, so this
        // is a plain column (the previous [ForeignKey("User")] referenced a navigation
        // that does not exist and broke model building).
        public string UserId { get; set; }
        public string CardHolderName { get; set; }
        public string Last4Digits { get; set; }
    public string CardBrand { get; set; }
        public DateTime AddedOn { get; set; }
    }
}
