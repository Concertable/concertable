using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Concertable.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodIdToApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodId",
                table: "OpportunityApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "OpportunityApplications");
        }
    }
}
