using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alpinarc.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    from_lodge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_lodge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    distance_meters = table.Column<int>(type: "integer", nullable: false),
                    ascent_meters = table.Column<int>(type: "integer", nullable: false),
                    descent_meters = table.Column<int>(type: "integer", nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stages", x => x.id);
                    table.CheckConstraint("ck_stages_ascent_meters", "ascent_meters BETWEEN 0 AND 10000");
                    table.CheckConstraint("ck_stages_descent_meters", "descent_meters BETWEEN 0 AND 10000");
                    table.CheckConstraint("ck_stages_different_lodges", "from_lodge_id <> to_lodge_id");
                    table.CheckConstraint("ck_stages_distance_meters", "distance_meters BETWEEN 1 AND 100000");
                    table.CheckConstraint("ck_stages_duration_minutes", "duration_minutes BETWEEN 1 AND 1440");
                    table.ForeignKey(
                        name: "fk_stages_lodges_from_lodge_id",
                        column: x => x.from_lodge_id,
                        principalTable: "lodges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stages_lodges_to_lodge_id",
                        column: x => x.to_lodge_id,
                        principalTable: "lodges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_stages_from_lodge_id_to_lodge_id",
                table: "stages",
                columns: new[] { "from_lodge_id", "to_lodge_id" });

            migrationBuilder.CreateIndex(
                name: "ix_stages_to_lodge_id",
                table: "stages",
                column: "to_lodge_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stages");
        }
    }
}
