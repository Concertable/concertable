using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Parameters
{
    public class TicketPurchaseParams
    {
        [Required(ErrorMessage = "Payment method ID is required")]
        public string PaymentMethodId { get; set; }
        [Required(ErrorMessage = "Event ID is required")]
        public int EventId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
    }
}
