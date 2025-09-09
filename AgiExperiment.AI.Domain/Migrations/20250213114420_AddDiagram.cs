using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgiExperiment.AI.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddDiagram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Diagrams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diagrams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiagramNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionX = table.Column<double>(type: "float", nullable: false),
                    PositionY = table.Column<double>(type: "float", nullable: false),
                    PositionZ = table.Column<double>(type: "float", nullable: false),
                    Template = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiagramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagramNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiagramNodes_Diagrams_DiagramId",
                        column: x => x.DiagramId,
                        principalTable: "Diagrams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiagramNodePorts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInput = table.Column<bool>(type: "bit", nullable: false),
                    PositionX = table.Column<double>(type: "float", nullable: false),
                    PositionY = table.Column<double>(type: "float", nullable: false),
                    PositionZ = table.Column<double>(type: "float", nullable: false),
                    Template = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiagramNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagramNodePorts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiagramNodePorts_DiagramNodes_DiagramNodeId",
                        column: x => x.DiagramNodeId,
                        principalTable: "DiagramNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiagramNodeLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Template = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourcePositionX = table.Column<double>(type: "float", nullable: false),
                    SourcePositionY = table.Column<double>(type: "float", nullable: false),
                    SourcePositionZ = table.Column<double>(type: "float", nullable: false),
                    TargetPositionX = table.Column<double>(type: "float", nullable: false),
                    TargetPositionY = table.Column<double>(type: "float", nullable: false),
                    TargetPositionZ = table.Column<double>(type: "float", nullable: false),
                    SourceNodePortId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetNodePortId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiagramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiagramNodeLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiagramNodeLinks_DiagramNodePorts_SourceNodePortId",
                        column: x => x.SourceNodePortId,
                        principalTable: "DiagramNodePorts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiagramNodeLinks_DiagramNodePorts_TargetNodePortId",
                        column: x => x.TargetNodePortId,
                        principalTable: "DiagramNodePorts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiagramNodeLinks_Diagrams_DiagramId",
                        column: x => x.DiagramId,
                        principalTable: "Diagrams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiagramNodeLinks_DiagramId",
                table: "DiagramNodeLinks",
                column: "DiagramId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagramNodeLinks_SourceNodePortId",
                table: "DiagramNodeLinks",
                column: "SourceNodePortId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagramNodeLinks_TargetNodePortId",
                table: "DiagramNodeLinks",
                column: "TargetNodePortId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagramNodePorts_DiagramNodeId",
                table: "DiagramNodePorts",
                column: "DiagramNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_DiagramNodes_DiagramId",
                table: "DiagramNodes",
                column: "DiagramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiagramNodeLinks");

            migrationBuilder.DropTable(
                name: "DiagramNodePorts");

            migrationBuilder.DropTable(
                name: "DiagramNodes");

            migrationBuilder.DropTable(
                name: "Diagrams");
        }
    }
}
