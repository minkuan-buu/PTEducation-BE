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
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Attendance_ClassSchedule' AND parent_object_id = OBJECT_ID('Attendance'))
                    ALTER TABLE [Attendance] DROP CONSTRAINT [FK_Attendance_ClassSchedule];
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Att_CS' AND parent_object_id = OBJECT_ID('Attendance'))
                    ALTER TABLE [Attendance] DROP CONSTRAINT [FK_Att_CS];
                """);

            migrationBuilder.Sql(
                """
                DECLARE @pkName sysname;
                SELECT @pkName = kc.name
                FROM sys.key_constraints kc
                INNER JOIN sys.tables t ON kc.parent_object_id = t.object_id
                WHERE kc.[type] = 'PK'
                  AND t.name = 'ClassSchedule';

                IF @pkName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [ClassSchedule] DROP CONSTRAINT [' + @pkName + ']');
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('AttendanceDetail', 'CreatedAt') IS NULL
                BEGIN
                    ALTER TABLE [AttendanceDetail]
                    ADD [CreatedAt] datetime NOT NULL DEFAULT '0001-01-01T00:00:00.000';
                END
                """);

            migrationBuilder.AddPrimaryKey(
                name: "PK__ClassSch__3214EC077CA59604",
                table: "ClassSchedule",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Att_CS",
                table: "Attendance",
                column: "ClassScheduleId",
                principalTable: "ClassSchedule",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Attendance_ClassSchedule' AND parent_object_id = OBJECT_ID('Attendance'))
                    ALTER TABLE [Attendance] DROP CONSTRAINT [FK_Attendance_ClassSchedule];
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Att_CS' AND parent_object_id = OBJECT_ID('Attendance'))
                    ALTER TABLE [Attendance] DROP CONSTRAINT [FK_Att_CS];
                """);

            migrationBuilder.Sql(
                """
                DECLARE @pkName sysname;
                SELECT @pkName = kc.name
                FROM sys.key_constraints kc
                INNER JOIN sys.tables t ON kc.parent_object_id = t.object_id
                WHERE kc.[type] = 'PK'
                  AND t.name = 'ClassSchedule';

                IF @pkName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE [ClassSchedule] DROP CONSTRAINT [' + @pkName + ']');
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('AttendanceDetail', 'CreatedAt') IS NOT NULL
                BEGIN
                    ALTER TABLE [AttendanceDetail] DROP COLUMN [CreatedAt];
                END
                """);

            migrationBuilder.AddPrimaryKey(
                name: "PK__ClassSch__3214EC07C7AAA2EF",
                table: "ClassSchedule",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Att_CS",
                table: "Attendance",
                column: "ClassScheduleId",
                principalTable: "ClassSchedule",
                principalColumn: "Id");
        }
    }
}
