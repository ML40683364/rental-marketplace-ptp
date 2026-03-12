// using System;
// using Microsoft.EntityFrameworkCore.Migrations;
// using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

// #nullable disable

// namespace StarterApp.Database.Migrations
// {
//     /// <inheritdoc />
//     public partial class AddItemsTable : Migration
//     {
//         /// <inheritdoc />
//         protected override void Up(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.CreateTable(
//                 name: "items",
//                 columns: table => new
//                 {
//                     Id = table.Column<int>(type: "integer", nullable: false)
//                         .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
//                     Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
//                     Description = table.Column<string>(type: "text", nullable: false),
//                     DailyRate = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
//                     Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
//                     Location = table.Column<string>(type: "text", nullable: false),
//                     IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
//                     CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
//                     UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
//                     OwnerId = table.Column<int>(type: "integer", nullable: false)
//                 },
//                 constraints: table =>
//                 {
//                     table.PrimaryKey("PK_items", x => x.Id);
//                     table.ForeignKey(
//                         name: "FK_items_users_OwnerId",
//                         column: x => x.OwnerId,
//                         principalTable: "users",
//                         principalColumn: "Id",
//                         onDelete: ReferentialAction.Cascade);
//                 });

//             migrationBuilder.CreateIndex(
//                 name: "IX_items_OwnerId",
//                 table: "items",
//                 column: "OwnerId");
//         }

//         /// <inheritdoc />
//         protected override void Down(MigrationBuilder migrationBuilder)
//         {
//             migrationBuilder.DropTable(
//                 name: "items");
//         }
//     }
// }
