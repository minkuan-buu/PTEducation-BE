using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTEducation.Data.Entities.Migrations
{
    /// <inheritdoc />
    public partial class Add_Attendance_Date : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Attendance','Date') IS NULL
                BEGIN
                    ALTER TABLE [Attendance]
                    ADD [Date] date NOT NULL
                        CONSTRAINT [DF_Attendance_Date] DEFAULT (CONVERT(date, GETDATE())) WITH VALUES;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @dc_name sysname;

                SELECT @dc_name = dc.name
                FROM sys.default_constraints dc
                JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
                WHERE dc.parent_object_id = OBJECT_ID('Attendance') AND c.name = 'Date';

                IF @dc_name IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [Attendance] DROP CONSTRAINT [' + @dc_name + ']');
                END

                IF COL_LENGTH('Attendance','Date') IS NOT NULL
                BEGIN
                    ALTER TABLE [Attendance] DROP COLUMN [Date];
                END
            ");
        }
    }
}
