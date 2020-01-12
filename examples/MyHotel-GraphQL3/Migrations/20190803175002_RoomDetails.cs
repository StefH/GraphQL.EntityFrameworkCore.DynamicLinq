using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyHotel.Migrations
{
    public partial class RoomDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoomDetailId",
                table: "Rooms",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RoomDetails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Windows = table.Column<int>(nullable: false),
                    Beds = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomDetails", x => x.Id);
                });

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

            migrationBuilder.InsertData(
                table: "RoomDetails",
                columns: new[] { "Id", "Beds", "Windows" },
                values: new object[,]
                {
                    { 100, 1, 2 },
                    { 101, 1, 4 },
                    { 102, 2, 3 },
                    { 103, 2, 0 }
                });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1,
                column: "RoomDetailId",
                value: 100);

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2,
                column: "RoomDetailId",
                value: 101);

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3,
                column: "RoomDetailId",
                value: 102);

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 4,
                column: "RoomDetailId",
                value: 103);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomDetailId",
                table: "Rooms",
                column: "RoomDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_RoomDetails_RoomDetailId",
                table: "Rooms",
                column: "RoomDetailId",
                principalTable: "RoomDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_RoomDetails_RoomDetailId",
                table: "Rooms");

            migrationBuilder.DropTable(
                name: "RoomDetails");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_RoomDetailId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "RoomDetailId",
                table: "Rooms");

            migrationBuilder.UpdateData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegisterDate",
                value: new DateTime(2019, 2, 1, 17, 24, 17, 42, DateTimeKind.Local).AddTicks(4605));

            migrationBuilder.UpdateData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 2,
                column: "RegisterDate",
                value: new DateTime(2019, 2, 6, 17, 24, 17, 45, DateTimeKind.Local).AddTicks(7666));

            migrationBuilder.UpdateData(
                table: "Guests",
                keyColumn: "Id",
                keyValue: 3,
                column: "RegisterDate",
                value: new DateTime(2019, 2, 10, 17, 24, 17, 45, DateTimeKind.Local).AddTicks(7851));

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CheckinDate", "CheckoutDate" },
                values: new object[] { new DateTime(2019, 2, 11, 17, 24, 17, 46, DateTimeKind.Local).AddTicks(3759), new DateTime(2019, 2, 21, 17, 24, 17, 46, DateTimeKind.Local).AddTicks(3849) });

            migrationBuilder.UpdateData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CheckinDate", "CheckoutDate" },
                values: new object[] { new DateTime(2019, 2, 11, 17, 24, 17, 46, DateTimeKind.Local).AddTicks(9405), new DateTime(2019, 2, 21, 17, 24, 17, 46, DateTimeKind.Local).AddTicks(9540) });
        }
    }
}
