using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyStoreAPI.Areas.Identity.Data;

namespace MyStoreAPI.Models
{
    public class AddressModel
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } // Foreign key for the user
        public string Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }

        public virtual MyStoreAPIUser User { get; set; } // Navigation property
    }
}
