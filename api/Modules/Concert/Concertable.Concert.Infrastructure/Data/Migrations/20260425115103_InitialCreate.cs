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
            migrationBuilder.EnsureSchema(
                name: "concert");

            migrationBuilder.CreateTable(
                name: "ArtistReadModels",
                schema: "concert",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
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
                schema: "concert",
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
                schema: "concert",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
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
                schema: "concert",
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
                        principalSchema: "concert",
                        principalTable: "ArtistReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistReadModelGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalSchema: "dbo",
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Opportunities",
                schema: "concert",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VenueId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opportunities_VenueReadModels_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "concert",
                        principalTable: "VenueReadModels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OpportunityApplications",
                schema: "concert",
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
                        principalSchema: "concert",
                        principalTable: "ArtistReadModels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OpportunityApplications_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "concert",
                        principalTable: "Opportunities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OpportunityGenres",
                schema: "concert",
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
                        principalSchema: "dbo",
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityGenres_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "concert",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConcertBookings",
                schema: "concert",
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
                        principalSchema: "concert",
                        principalTable: "OpportunityApplications",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Concerts",
                schema: "concert",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ArtistId = table.Column<int>(type: "int", nullable: false),
                    VenueId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    About = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BannerUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTickets = table.Column<int>(type: "int", nullable: false),
                    AvailableTickets = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatePosted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<Point>(type: "geography", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Concerts_ArtistReadModels_ArtistId",
                        column: x => x.ArtistId,
                        principalSchema: "concert",
                        principalTable: "ArtistReadModels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Concerts_ConcertBookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "concert",
                        principalTable: "ConcertBookings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Concerts_VenueReadModels_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "concert",
                        principalTable: "VenueReadModels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ConcertGenres",
                schema: "concert",
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
                        principalSchema: "concert",
                        principalTable: "Concerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConcertGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalSchema: "dbo",
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConcertImages",
                schema: "concert",
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
                        principalSchema: "concert",
                        principalTable: "Concerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                schema: "concert",
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
                schema: "concert",
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
                        principalSchema: "concert",
                        principalTable: "Concerts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalSchema: "concert",
                        principalTable: "Reviews",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtistReadModelGenres_GenreId",
                schema: "concert",
                table: "ArtistReadModelGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistReadModels_UserId",
                schema: "concert",
                table: "ArtistReadModels",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConcertBookings_ApplicationId",
                schema: "concert",
                table: "ConcertBookings",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConcertGenres_GenreId",
                schema: "concert",
                table: "ConcertGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_ConcertImages_ConcertId",
                schema: "concert",
                table: "ConcertImages",
                column: "ConcertId");

            migrationBuilder.CreateIndex(
                name: "IX_Concerts_ArtistId",
                schema: "concert",
                table: "Concerts",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Concerts_BookingId",
                schema: "concert",
                table: "Concerts",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Concerts_VenueId",
                schema: "concert",
                table: "Concerts",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_ContractId",
                schema: "concert",
                table: "Opportunities",
                column: "ContractId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_VenueId",
                schema: "concert",
                table: "Opportunities",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityApplications_ArtistId",
                schema: "concert",
                table: "OpportunityApplications",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityApplications_OpportunityId_ArtistId",
                schema: "concert",
                table: "OpportunityApplications",
                columns: new[] { "OpportunityId", "ArtistId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityGenres_GenreId",
                schema: "concert",
                table: "OpportunityGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_TicketId",
                schema: "concert",
                table: "Reviews",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ConcertId",
                schema: "concert",
                table: "Tickets",
                column: "ConcertId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ReviewId",
                schema: "concert",
                table: "Tickets",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueReadModels_UserId",
                schema: "concert",
                table: "VenueReadModels",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Tickets_TicketId",
                schema: "concert",
                table: "Reviews",
                column: "TicketId",
                principalSchema: "concert",
                principalTable: "Tickets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Concerts_ArtistReadModels_ArtistId",
                schema: "concert",
                table: "Concerts");

            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityApplications_ArtistReadModels_ArtistId",
                schema: "concert",
                table: "OpportunityApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_ConcertBookings_OpportunityApplications_ApplicationId",
                schema: "concert",
                table: "ConcertBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Concerts_ConcertId",
                schema: "concert",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Tickets_TicketId",
                schema: "concert",
                table: "Reviews");

            migrationBuilder.DropTable(
                name: "ArtistReadModelGenres",
                schema: "concert");

            migrationBuilder.DropTable(
                name: "ConcertGenres",
                schema: "concert");

            migrationBuilder.DropTable(
                name: "ConcertImages",
                schema: "concert");

            migrationBuilder.DropTable(
                name: "ConcertRatingProjections",
                schema: "concert");

            migrationBuilder.DropTable(
                name: "OpportunityGenres",
                schema: "concert");

            migrationBuilder.DropTable(
                name: "ArtistReadModels",
                schema: "concert");

            migrationBuilder.DropTable(
                name: "OpportunityApplications",
                schema: "concert");

            migrationBuilder.DropTable(
                name: "Opportunities",
                schema: "concert");

            migrationBuilder.DropTable(
                name: "Concerts",
                schema: "concert");

            migrationBuilder.DropTable(
                name: "ConcertBookings",
                schema: "concert");

            migrationBuilder.DropTable(
                name: "VenueReadModels",
                schema: "concert");

            migrationBuilder.DropTable(
                name: "Tickets",
                schema: "concert");

            migrationBuilder.DropTable(
                name: "Reviews",
                schema: "concert");
        }
    }
}
