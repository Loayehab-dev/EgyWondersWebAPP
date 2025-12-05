using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using MVC_front_End_.Models;
using MVC_front_End_.Services;
using System.Security.Claims;

namespace MVC_front_End_.Controllers
{
    public class BookingController : Controller
    {
        private readonly GuestBookingService _bookingService;

        public BookingController(GuestBookingService bookingService)
        {
            _bookingService = bookingService;
            // Ensure this key is correct!
            StripeConfiguration.ApiKey = "sk_test_51SaNB9FFAQLHixkKNWnabBxoYBB04FE2dLjal6VztlzM6qRGOsVlmEXvHtD7lYW5tdXOzlhnqpuPnvy0BfROMIUg00LHwGZGwX";
        }

        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession(int listingId, DateTime checkIn, DateTime checkOut, int guests)
        {
            try
            {
                // 1. Get Business ID using standard Claims
                var claim = User.FindFirst("BusinessId");

                if (claim == null || claim.Value == "0")
                {
                    // If ID is missing, redirect to Login to refresh cookie
                    return RedirectToAction("Login", "Auth");
                }

                int userId = int.Parse(claim.Value);

                // 2. Create Booking via API
                var bookingModel = new BookingCreateDTO
                {
                    ListingId = listingId,
                    UserId = userId,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    NumberOfGuests = guests
                };

                var createdBooking = await _bookingService.CreateBookingAsync(bookingModel);

                // 3. Create Stripe Session
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(createdBooking.TotalPrice * 100),
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"Booking #{createdBooking.BookId}: {createdBooking.ListingTitle}"
                                },
                            },
                            Quantity = 1,
                        },
                    },
                    Mode = "payment",
                    SuccessUrl = $"https://localhost:7183/Booking/Success?bookingId={createdBooking.BookId}&amount={createdBooking.TotalPrice}",
                    CancelUrl = $"https://localhost:7183/Listing/Details/{listingId}",
                };

                var service = new SessionService();
                Session session = service.Create(options);

                return Redirect(session.Url);
            }
            catch (Exception ex)
            {
                
                TempData["Error"] = "Booking Error: " + ex.Message;
                return RedirectToAction("Details", "Listing", new { id = listingId });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Success(int bookingId, decimal amount, string session_id)
        {
            if (bookingId == 0) return RedirectToAction("Index", "Home");

            var paymentDto = new PaymentCreateDto
            {
                BookingId = bookingId,
                Amount = amount,
                PaymentMethod = "Stripe Credit Card",
                TransactionId = session_id ?? "Test-Transaction"
            };

            // Call the service (which now returns a string)
            string result = await _bookingService.RecordPaymentAsync(paymentDto);

            if (result == "Success")
            {
                ViewBag.Status = "Confirmed";
            }
            else
            {
                // Put the actual API error into the ViewBag
                ViewBag.Status = "Error";
                ViewBag.Error = result;
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var success = await _bookingService.CancelBookingAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Booking cancelled successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to cancel booking. It may already be cancelled.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }

            // Redirect back to My Trips so the user sees the status change
            return RedirectToAction("Trips", "Guest");
        }
    }
}