using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationToSellThings.APIs.Models
{
    public class CardModel
    {
        [Key]
        public Guid CardId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public string CardHolderName { get; set; }
        public string Last4Digits { get; set; }
    public string CardBrand { get; set; }
        public DateTime AddedOn { get; set; }
    }
}
