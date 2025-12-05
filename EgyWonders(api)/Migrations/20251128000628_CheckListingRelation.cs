using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EgyWonders.Migrations
{
    /// <inheritdoc />
    public partial class CheckListingRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- 1. REMOVED THE "DROP" LINES ---
            // We removed the code that drops 'ApplicationUserId' because 
            // it doesn't exist in your database, so dropping it would crash.

            // --- 2. ADD THE LISTING RELATIONSHIP ---
            // This is the important part. It creates the link between Booking and Listing.

            // Check if index exists first (Optional safety, or just try running it)
            migrationBuilder.CreateIndex(
                name: "IX_ListingBooking_ListingID",
                table: "ListingBooking",
                column: "ListingID");

            migrationBuilder.AddForeignKey(
                name: "FK_ListingBooking_Listing",
                table: "ListingBooking",
                column: "ListingID",
                principalTable: "Listing",
                principalColumn: "ListingID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListingBooking_Listing",
                table: "ListingBooking");

            migrationBuilder.DropIndex(
                name: "IX_ListingBooking_ListingID",
                table: "ListingBooking");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ApplicationUserId",
                table: "Users",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_AspNetUsers_ApplicationUserId",
                table: "Users",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
