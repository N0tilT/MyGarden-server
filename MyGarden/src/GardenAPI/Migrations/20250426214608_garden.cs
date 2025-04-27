using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GardenAPI.Migrations
{
    /// <inheritdoc />
    public partial class garden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          
            migrationBuilder.CreateTable(
                name: "GrowStage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrowStage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LightNeed",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LightNeed", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlantType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WateringNeed",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WateringNeed", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Group",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Group", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Group_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlantVariety",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlantTypeId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantVariety", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantVariety_PlantType_PlantTypeId",
                        column: x => x.PlantTypeId,
                        principalTable: "PlantType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Plant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    PlantTypeId = table.Column<int>(type: "integer", nullable: false),
                    PlantVarietyId = table.Column<int>(type: "integer", nullable: false),
                    WateringNeedId = table.Column<int>(type: "integer", nullable: false),
                    LightNeedId = table.Column<int>(type: "integer", nullable: false),
                    StageId = table.Column<int>(type: "integer", nullable: false),
                    ImageId = table.Column<int>(type: "integer", nullable: true),
                    RipeningPeriod = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BiologyTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Fertilization = table.Column<string>(type: "character varying(10240)", maxLength: 10240, nullable: true),
                    Toxicity = table.Column<string>(type: "character varying(10240)", maxLength: 10240, nullable: true),
                    Replacing = table.Column<string>(type: "character varying(10240)", maxLength: 10240, nullable: true),
                    Description = table.Column<string>(type: "character varying(10240)", maxLength: 10240, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "current_timestamp"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plant_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plant_GrowStage_StageId",
                        column: x => x.StageId,
                        principalTable: "GrowStage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plant_LightNeed_LightNeedId",
                        column: x => x.LightNeedId,
                        principalTable: "LightNeed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plant_PlantType_PlantTypeId",
                        column: x => x.PlantTypeId,
                        principalTable: "PlantType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plant_PlantVariety_PlantVarietyId",
                        column: x => x.PlantVarietyId,
                        principalTable: "PlantVariety",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plant_WateringNeed_WateringNeedId",
                        column: x => x.WateringNeedId,
                        principalTable: "WateringNeed",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Group_UserId",
                table: "Group",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Plant_GroupId",
                table: "Plant",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Plant_LightNeedId",
                table: "Plant",
                column: "LightNeedId");

            migrationBuilder.CreateIndex(
                name: "IX_Plant_PlantTypeId",
                table: "Plant",
                column: "PlantTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Plant_PlantVarietyId",
                table: "Plant",
                column: "PlantVarietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Plant_StageId",
                table: "Plant",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_Plant_UserId",
                table: "Plant",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Plant_WateringNeedId",
                table: "Plant",
                column: "WateringNeedId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantVariety_PlantTypeId",
                table: "PlantVariety",
                column: "PlantTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plant");

            migrationBuilder.DropTable(
                name: "Group");

            migrationBuilder.DropTable(
                name: "GrowStage");

            migrationBuilder.DropTable(
                name: "LightNeed");

            migrationBuilder.DropTable(
                name: "PlantVariety");

            migrationBuilder.DropTable(
                name: "WateringNeed");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "PlantType");
        }
    }
}
