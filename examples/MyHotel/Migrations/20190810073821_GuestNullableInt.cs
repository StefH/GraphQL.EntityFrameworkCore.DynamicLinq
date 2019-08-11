using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyHotel.Migrations
{
    public partial class GuestNullableInt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NullableInt",
                table: "Guests",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegisterDate",
                value: new DateTime(2019, 7, 31, 9, 38, 20, 747, DateTimeKind.Local).AddTicks(2192));

            migrationBuilder.UpdateData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 2,
                column: "RegisterDate",
                value: new DateTime(2019, 8, 5, 9, 38, 20, 756, DateTimeKind.Local).AddTicks(9171));

            migrationBuilder.UpdateData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 3,
                column: "RegisterDate",
                value: new DateTime(2019, 8, 9, 9, 38, 20, 756, DateTimeKind.Local).AddTicks(9366));

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CheckinDate", "CheckoutDate" },
                values: new object[] { new DateTime(2019, 8, 8, 9, 38, 20, 758, DateTimeKind.Local).AddTicks(380), new DateTime(2019, 8, 13, 9, 38, 20, 758, DateTimeKind.Local).AddTicks(399) });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CheckinDate", "CheckoutDate" },
                values: new object[] { new DateTime(2019, 8, 9, 9, 38, 20, 758, DateTimeKind.Local).AddTicks(5151), new DateTime(2019, 8, 14, 9, 38, 20, 758, DateTimeKind.Local).AddTicks(5167) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NullableInt",
                table: "Guests");

            migrationBuilder.UpdateData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegisterDate",
                value: new DateTime(2019, 7, 24, 19, 50, 2, 480, DateTimeKind.Local).AddTicks(1311));

            migrationBuilder.UpdateData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 2,
                column: "RegisterDate",
                value: new DateTime(2019, 7, 29, 19, 50, 2, 481, DateTimeKind.Local).AddTicks(9390));

            migrationBuilder.UpdateData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 3,
                column: "RegisterDate",
                value: new DateTime(2019, 8, 2, 19, 50, 2, 481, DateTimeKind.Local).AddTicks(9565));

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CheckinDate", "CheckoutDate" },
                values: new object[] { new DateTime(2019, 8, 1, 19, 50, 2, 482, DateTimeKind.Local).AddTicks(3414), new DateTime(2019, 8, 6, 19, 50, 2, 482, DateTimeKind.Local).AddTicks(3423) });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CheckinDate", "CheckoutDate" },
                values: new object[] { new DateTime(2019, 8, 2, 19, 50, 2, 482, DateTimeKind.Local).AddTicks(5052), new DateTime(2019, 8, 7, 19, 50, 2, 482, DateTimeKind.Local).AddTicks(5059) });
        }
    }
}
