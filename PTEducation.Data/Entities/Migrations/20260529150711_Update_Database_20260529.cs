using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTEducation.Data.Entities.Migrations
{
    /// <inheritdoc />
    public partial class Update_Database_20260529 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Class",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                defaultValueSql: "(NULL)",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldDefaultValueSql: "(NULL)");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "Attendance",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "Attendance",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "Attendance",
                type: "date",
                nullable: false,
                defaultValueSql: "(CONVERT([date],getdate()))",
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Class",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValueSql: "(NULL)",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100,
                oldDefaultValueSql: "(NULL)");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "Attendance",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "Attendance",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "Attendance",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldDefaultValueSql: "(CONVERT([date],getdate()))");
        }
    }
}
