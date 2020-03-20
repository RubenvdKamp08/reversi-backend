using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ReversiBackEnd.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Spels",
                columns: table => new
                {
                    GameId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Omschrijving = table.Column<string>(nullable: true),
                    Token = table.Column<string>(nullable: true),
                    spelerWitToken = table.Column<string>(nullable: true),
                    spelerZwartToken = table.Column<string>(nullable: true),
                    bord = table.Column<string>(nullable: true),
                    aanDeBeurt = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spels", x => x.GameId);
                });

            migrationBuilder.InsertData(
                table: "Spels",
                columns: new[] { "GameId", "Omschrijving", "Token", "aanDeBeurt", "bord", "spelerWitToken", "spelerZwartToken" },
                values: new object[] { 1, "test", "Hgju5pGMCUaYSE6GBW02dg==", 1, "[[0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0],[0,0,0,1,2,0,0,0],[0,0,0,2,1,0,0,0],[0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0]]", "11", "12" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Spels");
        }
    }
}
