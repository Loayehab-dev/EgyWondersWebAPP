using EgyWonders.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Payment
{
    [Key]
    public int PaymentId { get; set; }

    
    [Column("BookID")]
    public int BookingId { get; set; }

    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string Status { get; set; }
    public DateTime PaymentDate { get; set; }

    // This MUST match the column you added in SQL
    public string TransactionId { get; set; }

    // Navigation Property
    [ForeignKey("BookingId")]
    public virtual ListingBooking ListingBooking { get; set; }
}