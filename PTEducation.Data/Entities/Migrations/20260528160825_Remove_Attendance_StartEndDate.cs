using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTEducation.Data.Entities.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Attendance_StartEndDate : Migration
    {
        /// <inheritdoc />
                protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @dc_name sysname;
                -- Drop default constraint on StartDate if exists
                SELECT @dc_name = dc.name
                FROM sys.default_constraints dc
                JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
                WHERE dc.parent_object_id = OBJECT_ID('Attendance') AND c.name = 'StartDate';
                IF @dc_name IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [Attendance] DROP CONSTRAINT [' + @dc_name + ']');
                END

                -- Drop StartDate column if exists
                IF COL_LENGTH('Attendance','StartDate') IS NOT NULL
                BEGIN
                    ALTER TABLE [Attendance] DROP COLUMN [StartDate];
                END

                -- Drop default constraint on EndDate if exists
                SELECT @dc_name = dc.name
                FROM sys.default_constraints dc
                JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
                WHERE dc.parent_object_id = OBJECT_ID('Attendance') AND c.name = 'EndDate';
                IF @dc_name IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [Attendance] DROP CONSTRAINT [' + @dc_name + ']');
                END

                -- Drop EndDate column if exists
                IF COL_LENGTH('Attendance','EndDate') IS NOT NULL
                BEGIN
                    ALTER TABLE [Attendance] DROP COLUMN [EndDate];
                END
            ");
        }
        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Attendance','StartDate') IS NULL
                BEGIN
                    ALTER TABLE [Attendance] ADD [StartDate] datetime NULL;
                END
                IF COL_LENGTH('Attendance','EndDate') IS NULL
                BEGIN
                    ALTER TABLE [Attendance] ADD [EndDate] datetime NULL;
                END
            ");
        }
    }
}
