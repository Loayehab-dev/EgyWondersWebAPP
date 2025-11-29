using System;
using System.Collections.Generic;

namespace EgyWonders.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public string? Comment { get; set; }

    public int Rating { get; set; }

    public int UserId { get; set; }

    public int? ListingId { get; set; }

    public int? TourId { get; set; }

    public virtual Listing? Listing { get; set; }

    public virtual Tour? Tour { get; set; }

    public virtual User User { get; set; } = null!;
}
