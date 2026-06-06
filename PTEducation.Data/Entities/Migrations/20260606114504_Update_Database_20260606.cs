using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTEducation.Data.Entities.Migrations
{
    /// <inheritdoc />
    public partial class Update_Database_20260606 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK__ClassSch__3214EC07C7AAA2EF",
                table: "ClassSchedule");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AttendanceDetail",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK__ClassSch__3214EC077CA59604",
                table: "ClassSchedule",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK__ClassSch__3214EC077CA59604",
                table: "ClassSchedule");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AttendanceDetail");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ClassSch__3214EC07C7AAA2EF",
                table: "ClassSchedule",
                column: "Id");
        }
    }
}
