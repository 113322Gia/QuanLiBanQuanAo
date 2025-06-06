﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongBanHang.Migrations
{
    /// <inheritdoc />
    public partial class FixBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "InfoShop");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "InfoShop",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
