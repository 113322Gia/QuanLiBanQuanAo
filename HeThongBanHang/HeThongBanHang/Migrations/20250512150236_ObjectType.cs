using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongBanHang.Migrations
{
    /// <inheritdoc />
    public partial class ObjectType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ObjectTypeId",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ObjectType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectType", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ObjectTypeId",
                table: "Categories",
                column: "ObjectTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_ObjectType_ObjectTypeId",
                table: "Categories",
                column: "ObjectTypeId",
                principalTable: "ObjectType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_ObjectType_ObjectTypeId",
                table: "Categories");

            migrationBuilder.DropTable(
                name: "ObjectType");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ObjectTypeId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ObjectTypeId",
                table: "Categories");
        }
    }
}
