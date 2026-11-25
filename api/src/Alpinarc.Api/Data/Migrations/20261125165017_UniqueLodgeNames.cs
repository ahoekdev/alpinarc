using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alpinarc.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class UniqueLodgeNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The API trims names on create from now on; trim the existing ones to match, before
            // the unique index compares them. \s matches the whitespace .NET's Trim() removes.
            // This runs first: Postgres doesn't allow regular expressions on a column with a
            // non-deterministic collation.
            migrationBuilder.Sql("""
                UPDATE lodges SET name = regexp_replace(name, '^\s+|\s+$', '', 'g')
                WHERE name ~ '^\s|\s$';
                """);

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:case_insensitive", "und-u-ks-level2,und-u-ks-level2,icu,False");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "lodges",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "ix_lodges_name",
                table: "lodges",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_lodges_name",
                table: "lodges");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "lodges",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldCollation: "case_insensitive");

            // After the column: the collation can't be dropped while the column uses it.
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:CollationDefinition:case_insensitive", "und-u-ks-level2,und-u-ks-level2,icu,False");
        }
    }
}
