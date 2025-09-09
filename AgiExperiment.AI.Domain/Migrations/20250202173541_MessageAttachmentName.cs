using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgiExperiment.AI.Domain.Migrations
{
    /// <inheritdoc />
    public partial class MessageAttachmentName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "MessageAttachment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "MessageAttachment");
        }
    }
}
