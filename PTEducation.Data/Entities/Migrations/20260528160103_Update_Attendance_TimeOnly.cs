using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTEducation.Data.Entities.Migrations
{
    /// <inheritdoc />
    public partial class Update_Attendance_TimeOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add StartTime and EndTime if they don't already exist (safe for existing DBs)
            migrationBuilder.Sql(@"
IF COL_LENGTH('Attendance','StartTime') IS NULL
BEGIN
    ALTER TABLE [Attendance] ADD [StartTime] time NULL;
END
IF COL_LENGTH('Attendance','EndTime') IS NULL
BEGIN
    ALTER TABLE [Attendance] ADD [EndTime] time NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove columns if they exist
            migrationBuilder.Sql(@"
IF COL_LENGTH('Attendance','StartTime') IS NOT NULL
BEGIN
    ALTER TABLE [Attendance] DROP COLUMN [StartTime];
END
IF COL_LENGTH('Attendance','EndTime') IS NOT NULL
BEGIN
    ALTER TABLE [Attendance] DROP COLUMN [EndTime];
END
");
        }
    }
}
