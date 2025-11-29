using System;
using System.Collections.Generic;

namespace EgyWonders.Models;

public partial class Listing
{
    public int ListingId { get; set; }

    public string? Status { get; set; }

    public int Capacity { get; set; }

    public decimal PricePerNight { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Description { get; set; }

    public string Title { get; set; } = null!;

    public string? Category { get; set; }

    public string? CityName { get; set; }

    public decimal? CityLongitude { get; set; }

    public decimal? CityLatitude { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<ListingPhoto> ListingPhotos { get; set; } = new List<ListingPhoto>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User User { get; set; } = null!;
    public virtual ICollection<ListingBooking> ListingBookings { get; set; } = new List<ListingBooking>();

    public virtual ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
}
