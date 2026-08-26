using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Madibaz_Transit_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingAttendanceConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AttendanceConfirmed",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendanceConfirmed",
                table: "Bookings");
        }
    }
}
