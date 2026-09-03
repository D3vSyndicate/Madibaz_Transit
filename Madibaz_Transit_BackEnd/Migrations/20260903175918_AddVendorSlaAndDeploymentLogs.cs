using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Madibaz_Transit_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorSlaAndDeploymentLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TransitRoute",
                table: "TransitRoute");
            migrationBuilder.DropColumn(
                name: "Id",
                table: "TransitRoute");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DriverShifts",
                table: "DriverShifts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Drivers",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DriverShifts");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Drivers");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Buses",
                newName: "BusId");

            migrationBuilder.AddColumn<Guid>(
                name: "TransitRouteId",
                table: "TransitRoute",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "ShiftStatus",
                table: "DriverShifts",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "DriverShiftId",
                table: "DriverShifts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DriverId",
                table: "Drivers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Drivers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransitRoute",
                table: "TransitRoute",
                column: "TransitRouteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DriverShifts",
                table: "DriverShifts",
                column: "DriverShiftId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Drivers",
                table: "Drivers",
                column: "DriverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TransitRoute",
                table: "TransitRoute");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DriverShifts",
                table: "DriverShifts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Drivers",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "TransitRouteId",
                table: "TransitRoute");

            migrationBuilder.DropColumn(
                name: "DriverShiftId",
                table: "DriverShifts");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Drivers");

            migrationBuilder.RenameColumn(
                name: "BusId",
                table: "Buses",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "TransitRoute",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "ShiftStatus",
                table: "DriverShifts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "DriverShifts",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Drivers",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransitRoute",
                table: "TransitRoute",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DriverShifts",
                table: "DriverShifts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Drivers",
                table: "Drivers",
                column: "Id");
        }
    }
}
