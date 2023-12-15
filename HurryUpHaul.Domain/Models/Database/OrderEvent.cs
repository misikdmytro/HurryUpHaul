using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HurryUpHaul.Domain.Models.Database
{
    [Table("order_events")]
    internal class OrderEvent
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("order_id")]
        [ForeignKey(nameof(Order))]
        public Guid OrderId { get; set; }

        [Column("event_type")]
        public string EventType { get; set; }

        [Column("event_time")]
        public DateTime EventTime { get; set; }

        [Column("payload", TypeName = "jsonb")]
        public object Payload { get; set; }

        public Order Order { get; set; }
    }
}