using System;
using System.Collections.Generic;

namespace EgyWonders.Models;

public partial class Amenity
{
    public int AmenitiesId { get; set; }

    public string AmenityName { get; set; } = null!;

    public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>();
}
