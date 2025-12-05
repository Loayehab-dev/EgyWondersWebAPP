using System;
using System.Collections.Generic;

namespace EgyWonders.Models;

public partial class ListingBooking
{
    public int BookId { get; set; }

    public DateTime CheckIn { get; set; }

    public DateTime CheckOut { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }

    public int UserId { get; set; }
    public string? Status { get; set; }
    public DateTime BookingDate { get; set; }
    public int ListingId { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual Listing Listing { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
