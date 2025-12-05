using System;
using System.Collections.Generic;

namespace EgyWonders.Models;

public partial class Tour
{
    public int TourId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Description { get; set; }

    public string? Category { get; set; }

    public string? CityName { get; set; }

    public decimal? CityLatitude { get; set; }

    public decimal? CityLongitude { get; set; }

    public decimal? BasePrice { get; set; }

    public string Title { get; set; } = null!;

    public int UserId { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<TourSchedule> TourSchedules { get; set; } = new List<TourSchedule>();

    public virtual User User { get; set; } = null!;
}
