using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace HurryUpHaul.Domain.Models.Database
{
    [Index(nameof(CreatedBy))]
    [Index(nameof(CreatedAt))]
    internal class Order
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Details { get; set; }

        public OrderStatus Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        [Required]
        [MaxLength(256)]
        public string CreatedBy { get; set; }

        public DateTimeOffset LastUpdatedAt { get; set; }

        public virtual Restaurant Restaurant { get; set; }

        [Required]
        [ForeignKey(nameof(Restaurant))]
        public string RestaurantId { get; set; }
    }
}