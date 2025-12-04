using EgyWonders.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EgyWonders.Data;

// Ensure ApplicationUser inherits from IdentityUser
public partial class TravelDbContext : IdentityDbContext<ApplicationUser>
{
    public TravelDbContext()
    {
    }

    public TravelDbContext(DbContextOptions<TravelDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Amenity> Amenities { get; set; }

    public virtual DbSet<GuideCertification> GuideCertifications { get; set; }

    public virtual DbSet<HostDocument> HostDocuments { get; set; }

    public virtual DbSet<Listing> Listings { get; set; }

    public virtual DbSet<ListingBooking> ListingBookings { get; set; }

    public virtual DbSet<ListingPhoto> ListingPhotos { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Tour> Tours { get; set; }

    public virtual DbSet<TourBooking> TourBookings { get; set; }

    public virtual DbSet<TourSchedule> TourSchedules { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=localhost;Database=TravelDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder) { 
         base.OnModelCreating(modelBuilder);
    
        modelBuilder.Entity<Amenity>(entity =>
        {
            entity.HasKey(e => e.AmenitiesId).HasName("PK__Amenitie__431B4E095395317B");

            entity.HasIndex(e => e.AmenityName, "UQ__Amenitie__7B4A459F071CF136").IsUnique();

            entity.Property(e => e.AmenitiesId).HasColumnName("AmenitiesID");
            entity.Property(e => e.AmenityName).HasMaxLength(255);
        });

        modelBuilder.Entity<GuideCertification>(entity =>
        {
            entity.HasKey(e => e.CertificationId).HasName("PK__GuideCer__1237E58AEB0DFDAF");

            entity.ToTable("GuideCertification");

            entity.Property(e => e.CertificationName).HasMaxLength(150);

            entity.HasOne(d => d.Guide).WithMany(p => p.GuideCertifications)
                .HasForeignKey(d => d.GuideId)
                .HasConstraintName("FK__GuideCert__Guide__0A9D95DB");
        });

        modelBuilder.Entity<HostDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__HostDocu__1ABEEF6FFC14A6FC");

            entity.Property(e => e.DocumentId).HasColumnName("DocumentID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DocumentPath).HasMaxLength(500);
            entity.Property(e => e.NationalId)
                .HasMaxLength(50)
                .HasColumnName("NationalID");
            entity.Property(e => e.UserId).HasColumnName("userID");

            entity.HasOne(d => d.User).WithMany(p => p.HostDocuments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__HostDocum__userI__5070F446");
        });

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.HasKey(e => e.ListingId).HasName("PK__Listing__BF3EBEF07A43A04E");

            entity.ToTable("Listing");

            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CityLatitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.CityLongitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.CityName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PricePerNight).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("userID");

            entity.HasOne(d => d.User).WithMany(p => p.Listings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Listing__userID__571DF1D5");

            entity.HasMany(d => d.Amenities).WithMany(p => p.Listings)
                .UsingEntity<Dictionary<string, object>>(
                    "ListingAmenity",
                    r => r.HasOne<Amenity>().WithMany()
                        .HasForeignKey("AmenitiesId")
                        .HasConstraintName("FK__ListingAm__Ameni__619B8048"),
                    l => l.HasOne<Listing>().WithMany()
                        .HasForeignKey("ListingId")
                        .HasConstraintName("FK__ListingAm__Listi__60A75C0F"),
                    j =>
                    {
                        j.HasKey("ListingId", "AmenitiesId").HasName("PK__ListingA__3B0F0A10334AA6DF");
                        j.ToTable("ListingAmenities");
                        j.IndexerProperty<int>("ListingId").HasColumnName("ListingID");
                        j.IndexerProperty<int>("AmenitiesId").HasColumnName("AmenitiesID");
                    });
        });

        modelBuilder.Entity<ListingBooking>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__ListingB__3DE0C227881215E0");

            entity.ToTable("ListingBooking");

            entity.Property(e => e.BookId).HasColumnName("BookID");
            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserId).HasColumnName("userID");

            // 1. Link to User (Already there)
            entity.HasOne(d => d.User).WithMany(p => p.ListingBookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ListingBo__userI__68487DD7");

            modelBuilder.Entity<ListingBooking>(entity =>
            {
                // ... existing lines ...
                entity.Property(e => e.BookId).HasColumnName("BookID");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");

                // --- ADD THESE MAPPINGS ---
                entity.Property(e => e.Status).HasMaxLength(50); // Matches NVARCHAR(50)
                entity.Property(e => e.BookingDate)
                      .HasDefaultValueSql("(getdate())")
                      .HasColumnType("datetime2");

                // ... foreign keys ...
            });
            // This makes .Include(x => x.ListingBookings) work!
            entity.HasOne(d => d.Listing)
                .WithMany(p => p.ListingBookings) // Matches the collection you added to Listing.cs
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ListingBooking_Listing"); // (Verify this name in SQL if needed, but this usually works)
        });

        modelBuilder.Entity<ListingPhoto>(entity =>
        {
            entity.HasKey(e => e.PhotoId).HasName("PK__ListingP__21B7B582A3D2B0BB");

            entity.Property(e => e.PhotoId).HasColumnName("PhotoID");
            entity.Property(e => e.Caption).HasMaxLength(255);
            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.Url)
                .HasMaxLength(500)
                .HasColumnName("URL");

            entity.HasOne(d => d.Listing).WithMany(p => p.ListingPhotos)
                .HasForeignKey(d => d.ListingId)
                .HasConstraintName("FK__ListingPh__Listi__6477ECF3");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            // 1. Primary Key
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A581D315CAB");
            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");

            // 2. Properties
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);

            // 3. Foreign Key Mapping (FIXED)
            // We map the C# property 'BookingId' to the SQL column 'BookID'
            entity.Property(e => e.BookingId).HasColumnName("BookID");

            // 4. Relationship Configuration (FIXED)
            // HasOne must use the Navigation Property (ListingBooking), NOT the int ID
            entity.HasOne(d => d.ListingBooking)
                  .WithMany(p => p.Payments)
                  .HasForeignKey(d => d.BookingId)
                  .HasConstraintName("FK__Payments__BookID__01142BA1");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79AE0E3307F3");

            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.ListingId).HasColumnName("ListingID");
            entity.Property(e => e.TourId).HasColumnName("TourID");
            entity.Property(e => e.UserId).HasColumnName("userID");

            entity.HasOne(d => d.Listing).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ListingId)
                .HasConstraintName("FK__Reviews__Listing__06CD04F7");

            entity.HasOne(d => d.Tour).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.TourId)
                .HasConstraintName("FK__Reviews__TourID__07C12930");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reviews__userID__05D8E0BE");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A90690466");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B616075A6A664").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(225)
                .IsUnicode(false);
            entity.Property(e => e.Permission).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.TourId).HasName("PK__Tours__604CEA1028BB5C86");

            entity.Property(e => e.TourId).HasColumnName("TourID");
            entity.Property(e => e.BasePrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CityLatitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.CityLongitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.CityName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UserId).HasColumnName("userID");

            entity.HasOne(d => d.User).WithMany(p => p.Tours)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Tours__userID__6E01572D");
        });

        modelBuilder.Entity<TourBooking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__TourBook__73951ACD913708DC");

            entity.ToTable("TourBooking");

            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserId).HasColumnName("userID");

            entity.HasOne(d => d.Schedule).WithMany(p => p.TourBookings)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TourBooki__Sched__7A672E12");

            entity.HasOne(d => d.User).WithMany(p => p.TourBookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TourBooki__userI__797309D9");
        });

        modelBuilder.Entity<TourSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__TourSche__9C8A5B699D802CBD");

            entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID");
            entity.Property(e => e.CurrentBooked).HasDefaultValue(0);
            entity.Property(e => e.TourId).HasColumnName("TourID");

            entity.HasOne(d => d.Tour).WithMany(p => p.TourSchedules)
                .HasForeignKey(d => d.TourId)
                .HasConstraintName("FK__TourSched__TourI__73BA3083");
        });

        modelBuilder.Entity<User>(entity =>
        {
            // ... keep your existing HasKey / HasIndex lines ...
            entity.HasKey(e => e.UserId).HasName("PK__Users__CB9A1CDF87030596");
            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534B5048DA7").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("userID");

            // --- 1. MAP THE COLUMN CORRECTLY ---
            entity.Property(e => e.AspNetUserId)
                  .HasColumnName("AspNetUserId")
                  .HasMaxLength(450)
                  .IsRequired();

            // --- 2. DEFINE THE RELATIONSHIP ---
            entity.HasOne(d => d.IdentityUser)
                  .WithOne(p => p.User) // <--- CRITICAL CHANGE: Connects back to ApplicationUser.User
                  .HasForeignKey<User>(d => d.AspNetUserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Keep the safe property mappings
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__UserRole__7335B03C9797F7E1");

            entity.Property(e => e.UserId).HasColumnName("userID");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.AssignedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__UserRoles__RoleI__4BAC3F29");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserRoles__userI__4AB81AF0");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
