using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace HurryUpHaul.Domain.Models.Database
{
    internal class Restaurant
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<IdentityUser> Managers { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}