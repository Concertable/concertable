using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Concertable.Concert.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArtistReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BannerUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    County = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Town = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConcertRatingProjections",
                columns: table => new
                {
                    ConcertId = table.Column<int>(type: "int", nullable: false),
                    AverageRating = table.Column<double>(type: "float", nullable: false),
                    ReviewCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcertRatingProjections", x => x.ConcertId);
                });

            migrationBuilder.CreateTable(
                name: "VenueReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    About = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    County = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Town = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<Point>(type: "geography", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArtistReadModelGenres",
                columns: table => new
                {
                    ArtistReadModelId = table.Column<int>(type: "int", nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistReadModelGenres", x => new { x.ArtistReadModelId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_ArtistReadModelGenres_ArtistReadModels_ArtistReadModelId",
                        column: x => x.ArtistReadModelId,
                        principalTable: "ArtistReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistReadModelGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Opportunities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VenueId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opportunities_VenueReadModels_VenueId",
                        column: x => x.VenueId,
                        principalTable: "VenueReadModels",
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
                    Status = table.Column<int>(type: "int", nullable: false),
                    OpportunityId = table.Column<int>(type: "int", nullable: false),
                    ArtistId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityApplications_ArtistReadModels_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "ArtistReadModels",
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
                    Guarantee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ArtistDoorPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
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
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Concerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    About = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BannerUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTickets = table.Column<int>(type: "int", nullable: false),
                    AvailableTickets = table.Column<int>(type: "int", nullable: false),
                    DatePosted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<Point>(type: "geography", nullable: true)
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
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stars = table.Column<byte>(type: "tinyint", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConcertId = table.Column<int>(type: "int", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QrCode = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ReviewId = table.Column<int>(type: "int", nullable: true)
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
                        name: "FK_Tickets_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtistReadModelGenres_GenreId",
                table: "ArtistReadModelGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistReadModels_UserId",
                table: "ArtistReadModels",
                column: "UserId",
                unique: true);

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
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ConcertId",
                table: "Tickets",
                column: "ConcertId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ReviewId",
                table: "Tickets",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueReadModels_UserId",
                table: "VenueReadModels",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Tickets_TicketId",
                table: "Reviews",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityApplications_ArtistReadModels_ArtistId",
                table: "OpportunityApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_ConcertBookings_OpportunityApplications_ApplicationId",
                table: "ConcertBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Concerts_ConcertId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Tickets_TicketId",
                table: "Reviews");

            migrationBuilder.DropTable(
                name: "ArtistReadModelGenres");

            migrationBuilder.DropTable(
                name: "ConcertGenres");

            migrationBuilder.DropTable(
                name: "ConcertImages");

            migrationBuilder.DropTable(
                name: "ConcertRatingProjections");

            migrationBuilder.DropTable(
                name: "DoorSplitContracts");

            migrationBuilder.DropTable(
                name: "FlatFeeContracts");

            migrationBuilder.DropTable(
                name: "OpportunityGenres");

            migrationBuilder.DropTable(
                name: "VenueHireContracts");

            migrationBuilder.DropTable(
                name: "VersusContracts");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "ArtistReadModels");

            migrationBuilder.DropTable(
                name: "OpportunityApplications");

            migrationBuilder.DropTable(
                name: "Opportunities");

            migrationBuilder.DropTable(
                name: "VenueReadModels");

            migrationBuilder.DropTable(
                name: "Concerts");

            migrationBuilder.DropTable(
                name: "ConcertBookings");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Reviews");
        }
    }
}
