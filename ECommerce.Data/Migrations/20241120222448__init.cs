using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ECommerce.Data.Migrations
{
    /// <inheritdoc />
    public partial class _init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ISBN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ListPrice = table.Column<double>(type: "float", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Price50 = table.Column<double>(type: "float", nullable: false),
                    Price100 = table.Column<double>(type: "float", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "DisplayOrder", "IsDeleted", "Name" },
                values: new object[,]
                {
                    { 1, 1, false, "Mobile" },
                    { 2, 2, false, "Electronics" },
                    { 3, 3, false, "Laptops&Accessories" },
                    { 4, 4, false, "SmartWatch" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Author", "CategoryId", "Description", "ISBN", "IsDeleted", "ListPrice", "Price", "Price100", "Price50", "Title" },
                values: new object[,]
                {
                    { 1, "Samsung", 1, "Experience epic performance with the Galaxy S23 Ultra. Features a 200MP camera, long battery life, and sleek design.", "MOB123456", false, 1200.0, 1100.0, 1000.0, 1050.0, "Galaxy S23 Ultra" },
                    { 2, "Sony", 2, "Industry-leading noise-canceling headphones with superior sound quality, 30-hour battery life, and a comfortable design.", "ELEC789012", false, 400.0, 350.0, 330.0, 340.0, "Sony WH-1000XM5" },
                    { 3, "Dell", 3, "The Dell XPS 15 offers a stunning InfinityEdge display, powerful performance with Intel i9, and sleek aluminum chassis.", "LAP345678", false, 2000.0, 1900.0, 1800.0, 1850.0, "Dell XPS 15" },
                    { 4, "Apple", 4, "Track your health and fitness with the Apple Watch Series 9. Features an always-on display, advanced sensors, and seamless integration with iPhone.", "SWATCH901234", false, 500.0, 470.0, 430.0, 450.0, "Apple Watch Series 9" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
