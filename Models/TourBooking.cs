using System;
using System.Collections.Generic;

namespace EgyWonders.Models;

public partial class TourBooking
{
    public int BookingId { get; set; }

    public decimal TotalPrice { get; set; }

    public string? Status { get; set; }

    public int NumParticipants { get; set; }

    public int UserId { get; set; }

    public int ScheduleId { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual TourSchedule Schedule { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
