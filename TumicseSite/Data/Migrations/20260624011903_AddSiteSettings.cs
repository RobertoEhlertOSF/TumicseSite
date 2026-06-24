using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TumicseSite.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Key);
                });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Key", "Value" },
                values: new object[,]
                {
                    { "Address", "Endereco a confirmar." },
                    { "GoogleMapsUrl", "" },
                    { "InstagramUrl", "" },
                    { "SiteName", "TUMICSE" },
                    { "WhatsAppDefaultMessage", "Ola! Gostaria de mais informacoes sobre o terreiro." },
                    { "WhatsAppNumber", "" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteSettings");
        }
    }
}
