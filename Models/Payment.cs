using System;
using System.Collections.Generic;

namespace EgyWonders.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? PaymentMethod { get; set; }

    public decimal Amount { get; set; }

    public string? Status { get; set; }

    public int? BookId { get; set; }

    public int? BookingId { get; set; }

    public virtual ListingBooking? Book { get; set; }

    public virtual TourBooking? Booking { get; set; }
}
