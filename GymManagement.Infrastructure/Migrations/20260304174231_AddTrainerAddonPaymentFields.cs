using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerAddonPaymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TrainerAddonAmountPaid",
                table: "Memberships",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "TrainerAddonPaid",
                table: "Memberships",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrainerAddonPaidAt",
                table: "Memberships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TrainerAddonFee",
                table: "MembershipPlans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrainerAddonAmountPaid",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "TrainerAddonPaid",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "TrainerAddonPaidAt",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "TrainerAddonFee",
                table: "MembershipPlans");
        }
    }
}
