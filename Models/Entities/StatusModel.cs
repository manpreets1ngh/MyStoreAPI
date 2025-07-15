using System.ComponentModel.DataAnnotations;

namespace MyStoreAPI.Models
{
    public class StatusModel
    {
        [Key]
        public int StatusId { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Type { get; set; }
    }
}
