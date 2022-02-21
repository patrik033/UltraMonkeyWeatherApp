using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UltraMonkeyEFLibrary.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeatherDatas",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Temp = table.Column<float>(type: "real", nullable: false),
                    AirMoisture = table.Column<int>(type: "int", nullable: true),
                    MoldIndex = table.Column<int>(type: "int", nullable: true),
                    OpenTime = table.Column<double>(type: "float", nullable: true),
                    Diff = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherDatas", x => new { x.Date, x.Location });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherDatas");
        }
    }
}
