using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstateCRM.Migrations
{
    /// <inheritdoc />
    public partial class DocumentGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TemplateType = table.Column<int>(type: "int", nullable: false),
                    JsonSchema = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileTemplatePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedEntityType = table.Column<int>(type: "int", nullable: true),
                    JsonData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedDocuments_DocumentTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "DocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplates_Category_IsActive",
                table: "DocumentTemplates",
                columns: new[] { "Category", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTemplates_TemplateType",
                table: "DocumentTemplates",
                column: "TemplateType");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_CreatedAtUtc",
                table: "GeneratedDocuments",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_RelatedEntityType_RelatedEntityId",
                table: "GeneratedDocuments",
                columns: new[] { "RelatedEntityType", "RelatedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_TemplateId",
                table: "GeneratedDocuments",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneratedDocuments");

            migrationBuilder.DropTable(
                name: "DocumentTemplates");
        }
    }
}
