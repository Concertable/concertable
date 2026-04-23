using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Concertable.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropConcertTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SettlementTransactions_ConcertBookings_BookingId",
                table: "SettlementTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketTransactions_Concerts_ConcertId",
                table: "TicketTransactions");

            migrationBuilder.DropTable(
                name: "ConcertGenres");

            migrationBuilder.DropTable(
                name: "ConcertImages");

            migrationBuilder.DropTable(
                name: "DoorSplitContracts");

            migrationBuilder.DropTable(
                name: "FlatFeeContracts");

            migrationBuilder.DropTable(
                name: "OpportunityGenres");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "VenueHireContracts");

            migrationBuilder.DropTable(
                name: "VersusContracts");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "Concerts");

            migrationBuilder.DropTable(
                name: "ConcertBookings");

            migrationBuilder.DropTable(
                name: "OpportunityApplications");

            migrationBuilder.DropTable(
                name: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_TicketTransactions_ConcertId",
                table: "TicketTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SettlementTransactions_BookingId",
                table: "SettlementTransactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Opportunities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VenueId = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opportunities_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Opportunities_Id",
                        column: x => x.Id,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArtistId = table.Column<int>(type: "int", nullable: false),
                    OpportunityId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityApplications_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OpportunityApplications_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OpportunityGenres",
                columns: table => new
                {
                    OpportunityId = table.Column<int>(type: "int", nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityGenres", x => new { x.OpportunityId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_OpportunityGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityGenres_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoorSplitContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ArtistDoorPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoorSplitContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoorSplitContracts_Contracts_Id",
                        column: x => x.Id,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlatFeeContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlatFeeContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlatFeeContracts_Contracts_Id",
                        column: x => x.Id,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VenueHireContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    HireFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueHireContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VenueHireContracts_Contracts_Id",
                        column: x => x.Id,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VersusContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ArtistDoorPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Guarantee = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersusContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VersusContracts_Contracts_Id",
                        column: x => x.Id,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConcertBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethodId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcertBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConcertBookings_OpportunityApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "OpportunityApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Concerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    About = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableTickets = table.Column<int>(type: "int", nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BannerUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatePosted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTickets = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Concerts_ConcertBookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "ConcertBookings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ConcertGenres",
                columns: table => new
                {
                    ConcertId = table.Column<int>(type: "int", nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcertGenres", x => new { x.ConcertId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_ConcertGenres_Concerts_ConcertId",
                        column: x => x.ConcertId,
                        principalTable: "Concerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConcertGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConcertImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConcertId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcertImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConcertImages_Concerts_ConcertId",
                        column: x => x.ConcertId,
                        principalTable: "Concerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcertId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QrCode = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Concerts_ConcertId",
                        column: x => x.ConcertId,
                        principalTable: "Concerts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Stars = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketTransactions_ConcertId",
                table: "TicketTransactions",
                column: "ConcertId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementTransactions_BookingId",
                table: "SettlementTransactions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ConcertBookings_ApplicationId",
                table: "ConcertBookings",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConcertGenres_GenreId",
                table: "ConcertGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_ConcertImages_ConcertId",
                table: "ConcertImages",
                column: "ConcertId");

            migrationBuilder.CreateIndex(
                name: "IX_Concerts_BookingId",
                table: "Concerts",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_VenueId",
                table: "Opportunities",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityApplications_ArtistId",
                table: "OpportunityApplications",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityApplications_OpportunityId_ArtistId",
                table: "OpportunityApplications",
                columns: new[] { "OpportunityId", "ArtistId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityGenres_GenreId",
                table: "OpportunityGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_TicketId",
                table: "Reviews",
                column: "TicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ConcertId",
                table: "Tickets",
                column: "ConcertId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementTransactions_ConcertBookings_BookingId",
                table: "SettlementTransactions",
                column: "BookingId",
                principalTable: "ConcertBookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketTransactions_Concerts_ConcertId",
                table: "TicketTransactions",
                column: "ConcertId",
                principalTable: "Concerts",
                principalColumn: "Id");
        }
    }
}
