 using System;
using System.Collections.Generic;

namespace EgyWonders.Models;

public partial class User
{
    public int UserId { get; set; }
  public string AspNetUserId {get; set; } = null!;
    public string Email { get; set; } = null!;


    public string? Gender { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Phone { get; set; }

    public string? Nationality { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public bool? EmailConfirmed { get; set; }

    public string? EmailConfirmationToken { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public int? AccessFailedCount { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiry { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<GuideCertification> GuideCertifications { get; set; } = new List<GuideCertification>();

    public virtual ICollection<HostDocument> HostDocuments { get; set; } = new List<HostDocument>();

    public virtual ICollection<ListingBooking> ListingBookings { get; set; } = new List<ListingBooking>();

    public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<TourBooking> TourBookings { get; set; } = new List<TourBooking>();

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();

    public virtual ApplicationUser IdentityUser { get; set; } // The Navigation

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
