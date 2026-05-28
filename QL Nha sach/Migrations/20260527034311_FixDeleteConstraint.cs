using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QL_Nha_sach.Migrations
{
    /// <inheritdoc />
    public partial class FixDeleteConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportDetails_Books_BookId",
                table: "ImportDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceDetails_Books_BookId",
                table: "InvoiceDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionTargets_Books_BookId",
                table: "PromotionTargets");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportDetails_Books_BookId",
                table: "ImportDetails",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceDetails_Books_BookId",
                table: "InvoiceDetails",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionTargets_Books_BookId",
                table: "PromotionTargets",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportDetails_Books_BookId",
                table: "ImportDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceDetails_Books_BookId",
                table: "InvoiceDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionTargets_Books_BookId",
                table: "PromotionTargets");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportDetails_Books_BookId",
                table: "ImportDetails",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceDetails_Books_BookId",
                table: "InvoiceDetails",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionTargets_Books_BookId",
                table: "PromotionTargets",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
