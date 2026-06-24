using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TumicseSite.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase5CalendarEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartsAt",
                table: "Events",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "EndsAt",
                table: "Events",
                newName: "EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_Events_StartsAt",
                table: "Events",
                newName: "IX_Events_StartDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsAllDay",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                """
                UPDATE [Events]
                SET [EventType] = CASE [EventType]
                    WHEN 'Gira de Umbanda' THEN 'Gira'
                    WHEN 'Estudo' THEN 'Study'
                    WHEN 'Desenvolvimento Mediunico' THEN 'Development'
                    WHEN 'Reuniao' THEN 'PrivateWork'
                    WHEN 'Evento Especial' THEN 'Other'
                    WHEN 'Atendimento' THEN 'PublicWork'
                    WHEN 'Outros' THEN 'Other'
                    ELSE [EventType]
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAllDay",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Events",
                newName: "StartsAt");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Events",
                newName: "EndsAt");

            migrationBuilder.RenameIndex(
                name: "IX_Events_StartDate",
                table: "Events",
                newName: "IX_Events_StartsAt");

            migrationBuilder.Sql(
                """
                UPDATE [Events]
                SET [EventType] = CASE [EventType]
                    WHEN 'Gira' THEN 'Gira de Umbanda'
                    WHEN 'Study' THEN 'Estudo'
                    WHEN 'Development' THEN 'Desenvolvimento Mediunico'
                    WHEN 'PrivateWork' THEN 'Reuniao'
                    WHEN 'PublicWork' THEN 'Atendimento'
                    WHEN 'Lecture' THEN 'Outros'
                    WHEN 'Feast' THEN 'Evento Especial'
                    WHEN 'Birthday' THEN 'Outros'
                    WHEN 'Maintenance' THEN 'Reuniao'
                    WHEN 'Other' THEN 'Outros'
                    ELSE [EventType]
                END
                """);
        }
    }
}
