using System;
using System.Collections.Generic;

namespace EgyWonders.Models;

public partial class ListingPhoto
{
    public int PhotoId { get; set; }

    public string Url { get; set; } = null!;

    public string? Caption { get; set; }

    public int ListingId { get; set; }

    public virtual Listing Listing { get; set; } = null!;
}
