using Concertable.B2B.Artist.Domain;
using Concertable.B2B.Concert.Domain.Entities;
using Concertable.B2B.Concert.Domain.Factories;
using Concertable.B2B.Contract.Domain.Entities;
using Concertable.B2B.Contract.Domain.Factories;
using Concertable.B2B.Seeding.Fakers;
using Concertable.B2B.User.Domain;
using Concertable.B2B.User.Domain.Factories;
using Concertable.B2B.Venue.Domain;
using Concertable.Contracts;
using Concertable.Kernel;
using Concertable.Kernel.Identity;
using Concertable.Seeding.Identity;
using NetTopologySuite.Geometries;

namespace Concertable.B2B.Seeding;

public sealed class SeedData
{
    public const string TestPassword = "Password11!";

    public UserEntity ArtistManager1 { get; }
    public UserEntity ArtistManagerNoArtist { get; }
    public UserEntity VenueManager1 { get; }
    public UserEntity VenueManager2 { get; }
    public UserEntity Admin { get; }

    public IReadOnlyList<UserEntity> ArtistManagers { get; }
    public IReadOnlyList<UserEntity> VenueManagers { get; }
    public IReadOnlyList<UserEntity> Users { get; }

    public ArtistEntity Artist { get; }
    public VenueEntity Venue { get; }

    public IReadOnlyList<ArtistEntity> Artists { get; }
    public IReadOnlyList<VenueEntity> Venues { get; }

    public IReadOnlyList<ContractEntity> Contracts { get; }
    public IReadOnlyList<OpportunityEntity> Opportunities { get; }
    public IReadOnlyList<BookingEntity> Bookings { get; }
    public IReadOnlyList<ApplicationEntity> Applications { get; }
    public IReadOnlyList<ConcertEntity> Concerts { get; }

    public FlatFeeContractEntity FlatFeeAppContract { get; }
    public FlatFeeContractEntity ConfirmedAppContract { get; }
    public FlatFeeContractEntity AwaitingPaymentAppContract { get; }
    public VersusContractEntity VersusAppContract { get; }
    public DoorSplitContractEntity DoorSplitAppContract { get; }
    public VenueHireContractEntity VenueHireAppContract { get; }
    public FlatFeeContractEntity PostedFlatFeeAppContract { get; }
    public DoorSplitContractEntity PostedDoorSplitAppContract { get; }
    public VersusContractEntity PostedVersusAppContract { get; }
    public VenueHireContractEntity PostedVenueHireAppContract { get; }
    public VersusContractEntity PastVersusAppContract { get; }
    public FlatFeeContractEntity PastFlatFeeAppContract { get; }
    public VenueHireContractEntity PastVenueHireAppContract { get; }
    public DoorSplitContractEntity PastDoorSplitAppContract { get; }

    public OpportunityEntity FreshVenueHireOpportunity { get; }

    public ApplicationEntity FlatFeeApp { get; }
    public ApplicationEntity VersusApp { get; }
    public ApplicationEntity DoorSplitApp { get; }
    public ApplicationEntity VenueHireApp { get; }

    public ApplicationEntity ConfirmedApp { get; }
    public BookingEntity ConfirmedBooking { get; }

    public ApplicationEntity AwaitingPaymentApp { get; }
    public BookingEntity AwaitingPaymentBooking { get; }

    public ApplicationEntity PostedFlatFeeApp { get; }
    public BookingEntity PostedFlatFeeBooking { get; }

    public ApplicationEntity PostedDoorSplitApp { get; }
    public BookingEntity PostedDoorSplitBooking { get; }

    public ApplicationEntity PostedVersusApp { get; }
    public BookingEntity PostedVersusBooking { get; }

    public ApplicationEntity PostedVenueHireApp { get; }
    public BookingEntity PostedVenueHireBooking { get; }

    public ApplicationEntity FinishedDoorSplitApp { get; }
    public BookingEntity FinishedDoorSplitBooking { get; }

    public ApplicationEntity FinishedVersusApp { get; }
    public BookingEntity FinishedVersusBooking { get; }

    public ApplicationEntity PastVersusApp { get; }
    public BookingEntity PastVersusBooking { get; }

    public ApplicationEntity PastFlatFeeApp { get; }
    public BookingEntity PastFlatFeeBooking { get; }

    public ApplicationEntity PastVenueHireApp { get; }
    public BookingEntity PastVenueHireBooking { get; }

    public ApplicationEntity PastDoorSplitApp { get; }
    public BookingEntity PastDoorSplitBooking { get; }

    public ApplicationEntity UpcomingFlatFeeApp { get; }
    public BookingEntity UpcomingFlatFeeBooking { get; }

    public ApplicationEntity UpcomingVenueHireApp { get; }
    public BookingEntity UpcomingVenueHireBooking { get; }

    public SeedData()
    {
        var now = DateTime.UtcNow;

        ArtistManager1 = UserFactory.FromRegistration(
            SeedUsers.ArtistManagerId(1), SeedUsers.ArtistManagerEmail(1), Role.ArtistManager);
        ArtistManagerNoArtist = UserFactory.FromRegistration(
            SeedUsers.ArtistManagerId(SeedUsers.ManagerCount),
            SeedUsers.ArtistManagerEmail(SeedUsers.ManagerCount),
            Role.ArtistManager);
        VenueManager1 = UserFactory.FromRegistration(
            SeedUsers.VenueManagerId(1), SeedUsers.VenueManagerEmail(1), Role.VenueManager);
        VenueManager2 = UserFactory.FromRegistration(
            SeedUsers.VenueManagerId(2), SeedUsers.VenueManagerEmail(2), Role.VenueManager);

        var artistManagers = new List<UserEntity> { ArtistManager1 };
        for (int i = 2; i < SeedUsers.ManagerCount; i++)
            artistManagers.Add(UserFactory.FromRegistration(
                SeedUsers.ArtistManagerId(i), SeedUsers.ArtistManagerEmail(i), Role.ArtistManager));
        artistManagers.Add(ArtistManagerNoArtist);
        ArtistManagers = artistManagers;

        var venueManagers = new List<UserEntity> { VenueManager1, VenueManager2 };
        for (int i = 3; i <= SeedUsers.ManagerCount; i++)
            venueManagers.Add(UserFactory.FromRegistration(
                SeedUsers.VenueManagerId(i), SeedUsers.VenueManagerEmail(i), Role.VenueManager));
        VenueManagers = venueManagers;

        Admin = UserFactory.FromRegistration(SeedUsers.Admin, SeedUsers.AdminEmail, Role.Admin,
            new Point(-0.5, 51.0) { SRID = 4326 },
            new Address("Leicestershire", "Loughborough"),
            "avatar.jpg");

        Users = [Admin, .. ArtistManagers, .. VenueManagers];

        var bands = new (string Name, string Banner, Genre[] Genres)[]
        {
            ("The Rockers", "rockers.jpg", [Genre.Rock, Genre.Pop, Genre.Jazz]),
            ("Indie Vibes", "indievibes.jpg", [Genre.Rock, Genre.Electronic, Genre.HipHop]),
            ("Electronic Pulse", "electronicpulse.jpg", [Genre.Electronic, Genre.Jazz]),
            ("Hip-Hop Flow", "hiphopflow.jpg", [Genre.HipHop]),
            ("Jazz Masters", "jazzmaster.jpg", [Genre.Indie, Genre.Jazz]),
            ("Always Punks", "alwayspunks.jpg", [Genre.Rock, Genre.Indie]),
            ("The Hollow Frequencies", "hollowfrequencies.jpg", [Genre.Pop]),
            ("Neon Foxes", "neonfoxes.jpg", [Genre.HipHop, Genre.Pop]),
            ("Velvet Static", "velvetstatic.jpg", [Genre.Electronic, Genre.Jazz]),
            ("Echo Bloom", "echobloom.jpg", [Genre.Rock, Genre.DnB]),
            ("The Wild Chords", "wildchords.jpg", [Genre.Indie, Genre.Rock]),
            ("Glitch & Glow", "glitchandglow.jpg", [Genre.Pop]),
            ("Sonic Mirage", "sonicmirage.jpg", [Genre.Indie, Genre.Electronic]),
            ("Neon Echoes", "neonechoes.jpg", [Genre.HipHop]),
            ("Dreamwave Collective", "dreamwavecollective.jpg", [Genre.DnB]),
            ("Synth Pulse", "synthpulse.jpg", [Genre.Rock]),
            ("The Brass Poets", "brasspoets.jpg", [Genre.Jazz]),
            ("Groove Alchemy", "groovealchemy.jpg", [Genre.Indie]),
            ("Velvet Rhymes", "velvetrhymes.jpg", [Genre.HipHop]),
            ("The Lo-Fi Syndicate", "lofisyndicate.jpg", [Genre.DnB]),
            ("Beats & Blue Notes", "beatsbluenotes.jpg", [Genre.House]),
            ("Bass Pilots", "basspilots.jpg", [Genre.Rock]),
            ("The Digital Prophets", "digitalprophets.jpg", [Genre.Electronic]),
            ("Neon Bass Theory", "neonbasstheory.jpg", [Genre.Indie]),
            ("Wavelength 303", "wavelength303.jpg", [Genre.Pop]),
            ("Gravity Loops", "gravityloops.jpg", [Genre.Rock]),
            ("The Golden Reverie", "goldenreverie.jpg", [Genre.House]),
            ("Fable Sound", "fablesound.jpg", [Genre.Electronic]),
            ("Moonlight Static", "moonlightstatic.jpg", [Genre.DnB]),
            ("The Chromatics", "thechromatics.jpg", [Genre.Jazz]),
            ("Echo Reverberation", "echoreverberation.jpg", [Genre.Indie]),
            ("Midnight Reverie", "midnightreverie.jpg", [Genre.Rock]),
            ("Static Wolves", "staticwolves.jpg", [Genre.HipHop]),
            ("Echo Collapse", "echocollapse.jpg", [Genre.Pop]),
            ("Violet Sundown", "violetsundown.jpg", [Genre.House])
        };

        int locIndex = 0;
        Artists = bands.Select((b, i) =>
        {
            var loc = Locations[locIndex++ % Locations.Length];
            return ArtistFaker.Create(
                i + 1,
                ArtistManagers[i].Id,
                b.Name,
                b.Banner,
                "avatar.jpg",
                new Point(loc.Longitude, loc.Latitude) { SRID = 4326 },
                new Address(loc.County, loc.Town),
                $"{b.Name.ToLowerInvariant().Replace(" ", "")}@test.com",
                b.Genres);
        }).ToArray();
        Artist = Artists[0];

        var venueData = new (string Name, string Banner)[]
        {
            ("Redhill Hall", "redhillhall.jpg"),
            ("Weybridge Pavilion", "weybridgepavilon.jpg"),
            ("Cobham Arts Centre", "cobhamarts.jpg"),
            ("Chertsey Arena", "chertseyarena.jpg"),
            ("Camden Electric Ballroom", "camdenballroom.jpg"),
            ("Manchester Night & Day Café", "manchesternightday.jpg"),
            ("Birmingham O2 Institute", "birminghamo2.jpg"),
            ("Edinburgh Usher Hall", "edinburghusher.jpg"),
            ("Liverpool Philharmonic Hall", "liverpoolphilharmonic.jpg"),
            ("Leeds Brudenell Social Club", "leedsbrudenell.jpg"),
            ("Glasgow Barrowland Ballroom", "glasgowbarrowland.jpg"),
            ("Sheffield Leadmill", "sheffieldleadmill.jpg"),
            ("Nottingham Rock City", "nottinghamrockcity.jpg"),
            ("Bristol Thekla", "bristolthekla.jpg"),
            ("Brighton Concorde 2", "brightonconcorde2.jpg"),
            ("Cardiff Tramshed", "cardifftramshed.jpg"),
            ("Newcastle O2 Academy", "newcastleo2.jpg"),
            ("Oxford O2 Academy", "oxfordo2.jpg"),
            ("Cambridge Corn Exchange", "cambridgecornexchange.jpg"),
            ("Bath Komedia", "bathkomedia.jpg"),
            ("Aberdeen The Lemon Tree", "aberdeenlemontree.jpg"),
            ("York Barbican", "yorkbarbican.jpg"),
            ("Belfast Limelight", "belfastlimelight.jpg"),
            ("Dublin Vicar Street", "dublinvicarstreet.jpg"),
            ("Norwich Waterfront", "norwichwaterfront.jpg"),
            ("Exeter Phoenix", "exeterphoenix.jpg"),
            ("Southampton Engine Rooms", "southamptonengine.jpg"),
            ("Hull The Welly Club", "hullwellyclub.jpg"),
            ("Plymouth Junction", "plymouthjunction.jpg"),
            ("Swansea Sin City", "swanseasincity.jpg"),
            ("Inverness Ironworks", "invernessironworks.jpg"),
            ("Stirling Albert Halls", "stirlingalberthalls.jpg"),
            ("Dundee Fat Sams", "dundeefatsams.jpg"),
            ("Coventry Empire", "coventryempire.jpg")
        };

        var venuesList = new List<VenueEntity>
        {
            VenueFaker.Create(
                1,
                VenueManager1.Id,
                "The Grand Venue",
                "grandvenue.jpg",
                "avatar.jpg",
                new Point(0, 51) { SRID = 4326 },
                new Address("Test County", "Test Town"),
                VenueManager1.Email)
        };
        for (int i = 0; i < venueData.Length; i++)
        {
            var loc = Locations[locIndex++ % Locations.Length];
            var v = venueData[i];
            venuesList.Add(VenueFaker.Create(
                i + 2,
                VenueManagers[i + 1].Id,
                v.Name,
                v.Banner,
                "avatar.jpg",
                new Point(loc.Longitude, loc.Latitude) { SRID = 4326 },
                new Address(loc.County, loc.Town),
                $"{v.Name.ToLowerInvariant().Replace(" ", "").Replace("&", "and").Replace("é", "e")}@test.com"));
        }
        Venues = venuesList;
        Venue = Venues[0];

        ConfirmedAppContract = FlatFeeContractFactory.Create(6, 200m);
        PostedVenueHireAppContract = VenueHireContractFactory.Create(21, 300m);
        PostedFlatFeeAppContract = FlatFeeContractFactory.Create(31, 200m);
        AwaitingPaymentAppContract = FlatFeeContractFactory.Create(33, 150m);
        DoorSplitAppContract = DoorSplitContractFactory.Create(50, 70m);
        VersusAppContract = VersusContractFactory.Create(51, 100m, 70m);
        PostedDoorSplitAppContract = DoorSplitContractFactory.Create(53, 65m);
        PostedVersusAppContract = VersusContractFactory.Create(54, 120m, 60m);
        FlatFeeAppContract = FlatFeeContractFactory.Create(58, 150m);
        VenueHireAppContract = VenueHireContractFactory.Create(59, 300m);
        PastVersusAppContract = VersusContractFactory.Create(64, 100m, 70m);
        PastFlatFeeAppContract = FlatFeeContractFactory.Create(65, 200m);
        PastVenueHireAppContract = VenueHireContractFactory.Create(66, 300m);
        PastDoorSplitAppContract = DoorSplitContractFactory.Create(67, 70m);

        Contracts =
        [
            FlatFeeContractFactory.Create(1, 150m),
            FlatFeeContractFactory.Create(2, 120m),
            DoorSplitContractFactory.Create(3, 60m),
            VersusContractFactory.Create(4, 80m, 50m),
            FlatFeeContractFactory.Create(5, 180m),
            ConfirmedAppContract,                                                       // 6
            FlatFeeContractFactory.Create(7, 160m),
            FlatFeeContractFactory.Create(8, 140m),
            DoorSplitContractFactory.Create(9, 70m),
            VenueHireContractFactory.Create(10, 250m),
            FlatFeeContractFactory.Create(11, 170m),
            VersusContractFactory.Create(12, 100m, 60m),
            FlatFeeContractFactory.Create(13, 150m),
            DoorSplitContractFactory.Create(14, 65m),
            FlatFeeContractFactory.Create(15, 190m),
            VenueHireContractFactory.Create(16, 220m),
            FlatFeeContractFactory.Create(17, 155m),
            VersusContractFactory.Create(18, 90m, 55m),
            DoorSplitContractFactory.Create(19, 60m),
            FlatFeeContractFactory.Create(20, 165m),
            PostedVenueHireAppContract,                                                 // 21
            FlatFeeContractFactory.Create(22, 175m),
            DoorSplitContractFactory.Create(23, 70m),
            VersusContractFactory.Create(24, 110m, 60m),
            FlatFeeContractFactory.Create(25, 185m),
            FlatFeeContractFactory.Create(26, 195m),
            DoorSplitContractFactory.Create(27, 65m),
            VenueHireContractFactory.Create(28, 280m),
            VersusContractFactory.Create(29, 95m, 55m),
            FlatFeeContractFactory.Create(30, 160m),
            PostedFlatFeeAppContract,                                                   // 31
            FlatFeeContractFactory.Create(32, 140m),
            AwaitingPaymentAppContract,                                                 // 33
            DoorSplitContractFactory.Create(34, 70m),
            VersusContractFactory.Create(35, 100m, 60m),
            FlatFeeContractFactory.Create(36, 170m),
            VenueHireContractFactory.Create(37, 240m),
            DoorSplitContractFactory.Create(38, 60m),
            FlatFeeContractFactory.Create(39, 180m),
            VersusContractFactory.Create(40, 120m, 65m),
            FlatFeeContractFactory.Create(41, 155m),
            DoorSplitContractFactory.Create(42, 70m),
            VenueHireContractFactory.Create(43, 260m),
            FlatFeeContractFactory.Create(44, 190m),
            VersusContractFactory.Create(45, 105m, 55m),
            FlatFeeContractFactory.Create(46, 165m),
            DoorSplitContractFactory.Create(47, 65m),
            VenueHireContractFactory.Create(48, 290m),
            VersusContractFactory.Create(49, 85m, 50m),
            DoorSplitAppContract,                                                       // 50
            VersusAppContract,                                                          // 51
            VenueHireContractFactory.Create(52, 170m),
            PostedDoorSplitAppContract,                                                 // 53
            PostedVersusAppContract,                                                    // 54
            FlatFeeContractFactory.Create(55, 180m),
            DoorSplitContractFactory.Create(56, 70m),
            VersusContractFactory.Create(57, 110m, 65m),
            FlatFeeAppContract,                                                         // 58
            VenueHireAppContract,                                                       // 59
            FlatFeeContractFactory.Create(60, 200m),
            DoorSplitContractFactory.Create(61, 70m),
            VersusContractFactory.Create(62, 100m, 60m),
            VenueHireContractFactory.Create(63, 250m),
            PastVersusAppContract,                                                      // 64
            PastFlatFeeAppContract,                                                     // 65
            PastVenueHireAppContract,                                                   // 66
            PastDoorSplitAppContract,                                                   // 67
        ];

        var opps = new List<OpportunityEntity>();
        var oppSpecs = new (int VenueId, int DaysOffset)[]
        {
            (1, -60), (2, -55), (3, -50), (4, -45), (5, -40),
            (6, -35), (7, -30), (8, -25), (9, -20), (10, -15),
            (1, -10), (2, -5), (3, 0), (4, 5), (5, 10),
            (6, 15), (7, 20), (8, 25), (9, 30), (10, 35),
            (1, -40), (2, 45), (3, 50), (4, 55), (5, 60),
            (6, 65), (7, 70), (8, 75), (9, 80), (10, 85),
            (1, -85), (1, 85), (1, 2), (1, 4), (1, 6),
            (2, 8), (2, 10), (2, 12), (3, 14), (3, 16),
            (3, 18), (4, 22), (5, 24), (6, 26), (1, 30),
            (1, 32), (1, 34), (1, 36), (1, 38), (1, -60),
            (1, -90), (1, 120), (1, 150), (1, 180), (1, 200),
            (1, 210), (1, 220), (1, 15), (1, 20), (1, 40),
            (1, 42), (1, 44), (1, 46), (1, -120), (1, -85),
            (1, -40), (1, -60),
        };
        for (int i = 0; i < oppSpecs.Length; i++)
        {
            var (venueId, days) = oppSpecs[i];
            var hours = i == 31 ? 5 : 3;
            opps.Add(OpportunityFactory.Create(
                i + 1,
                venueId,
                new DateRange(now.AddDays(days), now.AddDays(days).AddHours(hours)),
                contractId: Contracts[i].Id));
        }
        Opportunities = opps;
        FreshVenueHireOpportunity = opps[62];

        ConfirmedBooking = BookingFactory.Complete(1);
        PostedDoorSplitBooking = BookingFactory.ConfirmedDeferred(2);
        PostedVersusBooking = BookingFactory.ConfirmedDeferred(3);
        PostedFlatFeeBooking = BookingFactory.Complete(4);
        PostedVenueHireBooking = BookingFactory.Complete(5);
        AwaitingPaymentBooking = BookingFactory.AwaitingPayment(6);
        FinishedDoorSplitBooking = BookingFactory.CompleteDeferred(7);
        FinishedVersusBooking = BookingFactory.CompleteDeferred(8);
        PastVersusBooking = BookingFactory.ConfirmedDeferred(9);
        PastFlatFeeBooking = BookingFactory.Confirmed(10);
        PastVenueHireBooking = BookingFactory.Confirmed(11);
        PastDoorSplitBooking = BookingFactory.ConfirmedDeferred(12);
        UpcomingFlatFeeBooking = BookingFactory.Confirmed(13);
        UpcomingVenueHireBooking = BookingFactory.Confirmed(14);

        Bookings =
        [
            ConfirmedBooking,
            PostedDoorSplitBooking,
            PostedVersusBooking,
            PostedFlatFeeBooking,
            PostedVenueHireBooking,
            AwaitingPaymentBooking,
            FinishedDoorSplitBooking,
            FinishedVersusBooking,
            PastVersusBooking,
            PastFlatFeeBooking,
            PastVenueHireBooking,
            PastDoorSplitBooking,
            UpcomingFlatFeeBooking,
            UpcomingVenueHireBooking,
            BookingFactory.Complete(15), BookingFactory.Complete(16), BookingFactory.Complete(17), BookingFactory.Complete(18),
            BookingFactory.Complete(19), BookingFactory.Complete(20), BookingFactory.Complete(21), BookingFactory.Complete(22),
            BookingFactory.Complete(23), BookingFactory.Complete(24), BookingFactory.Complete(25), BookingFactory.Complete(26),
            BookingFactory.Complete(27), BookingFactory.Complete(28), BookingFactory.Complete(29), BookingFactory.Complete(30),
            BookingFactory.Complete(31), BookingFactory.Complete(32), BookingFactory.Complete(33), BookingFactory.Complete(34),
            BookingFactory.Complete(35), BookingFactory.Complete(36), BookingFactory.Complete(37), BookingFactory.Complete(38),
            BookingFactory.Complete(39), BookingFactory.Confirmed(40), BookingFactory.Confirmed(41), BookingFactory.Confirmed(42),
            BookingFactory.Confirmed(43), BookingFactory.Confirmed(44), BookingFactory.Confirmed(45), BookingFactory.Confirmed(46),
            BookingFactory.Confirmed(47),
        ];

        ConfirmedApp = ApplicationFactory.Accepted(1, 6, Bookings[0]);
        PostedDoorSplitApp = ApplicationFactory.Accepted(1, 53, Bookings[1]);
        PostedVersusApp = ApplicationFactory.Accepted(2, 54, Bookings[2]);
        PostedFlatFeeApp = ApplicationFactory.Accepted(2, 31, Bookings[3]);
        PostedVenueHireApp = ApplicationFactory.AcceptedPrepaid(1, 21, Bookings[4]);
        AwaitingPaymentApp = ApplicationFactory.Accepted(1, 33, Bookings[5]);
        FinishedDoorSplitApp = ApplicationFactory.Accepted(1, 50, Bookings[6]);
        FinishedVersusApp = ApplicationFactory.Accepted(1, 51, Bookings[7]);
        PastVersusApp = ApplicationFactory.Accepted(1, Opportunities[63].Id, Bookings[8]);
        PastFlatFeeApp = ApplicationFactory.Accepted(1, Opportunities[64].Id, Bookings[9]);
        PastVenueHireApp = ApplicationFactory.AcceptedPrepaid(1, Opportunities[65].Id, Bookings[10]);
        PastDoorSplitApp = ApplicationFactory.Accepted(1, Opportunities[66].Id, Bookings[11]);
        UpcomingFlatFeeApp = ApplicationFactory.Accepted(2, 58, Bookings[12]);
        UpcomingVenueHireApp = ApplicationFactory.AcceptedPrepaid(1, 59, Bookings[13]);

        DoorSplitApp = ApplicationFactory.Create(1, Opportunities[55].Id, Contracts[55].ContractType);
        VersusApp = ApplicationFactory.Create(1, Opportunities[56].Id, Contracts[56].ContractType);
        VenueHireApp = ApplicationFactory.CreatePrepaid(1, Opportunities[51].Id, Contracts[51].ContractType);
        FlatFeeApp = ApplicationFactory.Create(1, Opportunities[54].Id, Contracts[54].ContractType);

        Applications =
        [
            ApplicationFactory.Accepted(1, 1, Bookings[14]),
            ApplicationFactory.Accepted(2, 1, Bookings[15]),
            ApplicationFactory.Accepted(3, 1, Bookings[16]),
            ApplicationFactory.Accepted(4, 1, Bookings[17]),
            ApplicationFactory.Accepted(1, 2, Bookings[18]),
            ApplicationFactory.Accepted(2, 2, Bookings[19]),
            ApplicationFactory.Accepted(5, 2, Bookings[20]),
            ApplicationFactory.Accepted(6, 2, Bookings[21]),
            ApplicationFactory.Accepted(1, 3, Bookings[22]),
            ApplicationFactory.Accepted(2, 3, Bookings[23]),
            ApplicationFactory.Accepted(7, 3, Bookings[24]),
            ApplicationFactory.Accepted(8, 3, Bookings[25]),
            ApplicationFactory.Accepted(1, 4, Bookings[26]),
            ApplicationFactory.Accepted(2, 4, Bookings[27]),
            ApplicationFactory.Accepted(9, 4, Bookings[28]),
            ApplicationFactory.Accepted(10, 4, Bookings[29]),
            ApplicationFactory.Accepted(1, 5, Bookings[30]),
            ApplicationFactory.Accepted(2, 5, Bookings[31]),
            ApplicationFactory.Accepted(11, 5, Bookings[32]),
            ApplicationFactory.Accepted(12, 5, Bookings[33]),
            ConfirmedApp,
            ApplicationFactory.Accepted(2, 6, Bookings[34]),
            ApplicationFactory.Accepted(13, 6, Bookings[35]),
            ApplicationFactory.Accepted(14, 6, Bookings[36]),
            ApplicationFactory.Accepted(1, 7, Bookings[37]),
            ApplicationFactory.Accepted(2, 7, Bookings[38]),
            ApplicationFactory.Create(15, 7),
            ApplicationFactory.Create(16, 7),
            ApplicationFactory.Create(1, 8),
            ApplicationFactory.Create(2, 8),
            ApplicationFactory.Create(17, 8),
            ApplicationFactory.Create(18, 8),
            ApplicationFactory.Create(17, 40),
            ApplicationFactory.Create(18, 41),
            ApplicationFactory.Accepted(1, 14, Bookings[39]),
            ApplicationFactory.Create(2, 14),
            ApplicationFactory.Create(3, 14),
            ApplicationFactory.Create(4, 14),
            PostedDoorSplitApp,
            DoorSplitApp,
            ApplicationFactory.Create(7, 15),
            ApplicationFactory.Accepted(8, 15, Bookings[40]),
            ApplicationFactory.CreatePrepaid(9, 16),
            ApplicationFactory.CreatePrepaid(10, 16),
            ApplicationFactory.AcceptedPrepaid(11, 16, Bookings[41]),
            ApplicationFactory.CreatePrepaid(12, 16),
            VersusApp,
            ApplicationFactory.Create(14, 17),
            PostedVersusApp,
            ApplicationFactory.Create(16, 17),
            ApplicationFactory.Create(1, 34),
            ApplicationFactory.Create(2, 34),
            ApplicationFactory.Create(19, 34),
            ApplicationFactory.Create(20, 34),
            ApplicationFactory.Create(1, 38),
            ApplicationFactory.Create(2, 38),
            ApplicationFactory.Create(12, 38),
            ApplicationFactory.Create(4, 38),
            ApplicationFactory.Create(1, 45),
            ApplicationFactory.Create(2, 46),
            ApplicationFactory.Create(3, 47),
            ApplicationFactory.CreatePrepaid(4, 48),
            ApplicationFactory.Create(5, 49),
            ApplicationFactory.Create(2, 50),
            ApplicationFactory.Create(2, 51),
            VenueHireApp,
            ApplicationFactory.CreatePrepaid(2, 52),
            FlatFeeApp,
            PostedFlatFeeApp,
            ApplicationFactory.Create(3, 31),
            ApplicationFactory.Create(1, 32),
            ApplicationFactory.Create(2, 32),
            ApplicationFactory.Create(3, 32),
            AwaitingPaymentApp,
            PostedVenueHireApp,
            FinishedDoorSplitApp,
            FinishedVersusApp,
            PastVersusApp,
            PastFlatFeeApp,
            PastVenueHireApp,
            PastDoorSplitApp,
            UpcomingFlatFeeApp,
            UpcomingVenueHireApp,
            ApplicationFactory.Accepted(3, 34, Bookings[42]),
            ApplicationFactory.Create(4, 34),
            ApplicationFactory.Create(5, 34),
            ApplicationFactory.Accepted(1, 35, Bookings[43]),
            ApplicationFactory.Create(2, 35),
            ApplicationFactory.Create(4, 35),
            ApplicationFactory.Create(5, 35),
            ApplicationFactory.Accepted(4, 46, Bookings[44]),
            ApplicationFactory.Create(5, 46),
            ApplicationFactory.Create(6, 46),
            ApplicationFactory.Accepted(5, 47, Bookings[45]),
            ApplicationFactory.Create(6, 47),
            ApplicationFactory.Create(7, 47),
            ApplicationFactory.AcceptedPrepaid(6, 48, Bookings[46]),
            ApplicationFactory.CreatePrepaid(7, 48),
            ApplicationFactory.CreatePrepaid(8, 48),
        ];

        Concerts =
        [
            ConcertFaker.Post(1,  Bookings[0].Id,  "Ultimate Dance Party",       27m, 160, 1,  opps[5].VenueId,  opps[5].Period,  now.AddDays(2)),
            ConcertFaker.Post(2,  Bookings[1].Id,  "Boogie Wonderland",          25m, 120, 1,  opps[52].VenueId, opps[52].Period, now, [Genre.Rock, Genre.Indie]),
            ConcertFaker.Post(3,  Bookings[2].Id,  "Funk it up",                 20m, 150, 2,  opps[53].VenueId, opps[53].Period, now, [Genre.Rock, Genre.Indie]),
            ConcertFaker.Post(4,  Bookings[3].Id,  "Boogie it up!",              20m, 150, 2,  opps[30].VenueId, opps[30].Period, now.AddDays(-85)),
            ConcertFaker.Post(5,  Bookings[4].Id,  "VenueHire Spectacular",      30m, 200, 1,  opps[20].VenueId, opps[20].Period, now.AddDays(-40)),
            ConcertFaker.Post(6,  Bookings[5].Id,  "Awaiting Show",              15m, 100, 1,  opps[32].VenueId, opps[32].Period, now.AddDays(3)),
            ConcertFaker.Post(7,  Bookings[6].Id,  "DoorSplit Settlement Show",  20m, 100, 1,  opps[49].VenueId, opps[49].Period, now.AddDays(-60)),
            ConcertFaker.Post(8,  Bookings[7].Id,  "Versus Settlement Show",     20m, 100, 1,  opps[50].VenueId, opps[50].Period, now.AddDays(-90)),
            ConcertFaker.Post(9,  Bookings[8].Id,  "Past Versus Show",           20m, 100, 1,  opps[63].VenueId, opps[63].Period, now.AddDays(-120)),
            ConcertFaker.Post(10, Bookings[9].Id,  "Past FlatFee Show",          20m, 100, 1,  opps[64].VenueId, opps[64].Period, now.AddDays(-85)),
            ConcertFaker.Post(11, Bookings[10].Id, "Past VenueHire Show",        30m, 100, 1,  opps[65].VenueId, opps[65].Period, now.AddDays(-40)),
            ConcertFaker.Post(12, Bookings[11].Id, "Past DoorSplit Show",        20m, 100, 1,  opps[66].VenueId, opps[66].Period, now.AddDays(-60)),
            ConcertFaker.Post(13, Bookings[12].Id, "Upcoming FlatFee Show",      20m, 150, 2,  opps[57].VenueId, opps[57].Period, now, [Genre.Rock, Genre.Indie]),
            ConcertFaker.Post(14, Bookings[13].Id, "Upcoming VenueHire Show",    30m, 200, 1,  opps[58].VenueId, opps[58].Period, now, [Genre.Rock, Genre.Indie]),
            ConcertFaker.Post(15, Bookings[14].Id, "Rockin' all Night",          15m, 120, 1,  opps[0].VenueId,  opps[0].Period,  now.AddDays(-58)),
            ConcertFaker.Post(16, Bookings[15].Id, "Non Stop Party",             12m, 110, 2,  opps[0].VenueId,  opps[0].Period,  now.AddDays(-55)),
            ConcertFaker.Post(17, Bookings[16].Id, "Super Mix",                  18m, 130, 3,  opps[0].VenueId,  opps[0].Period,  now.AddDays(-52)),
            ConcertFaker.Post(18, Bookings[17].Id, "Hip-Hop till you flip-flop", 10m, 100, 4,  opps[0].VenueId,  opps[0].Period,  now.AddDays(-49)),
            ConcertFaker.Post(19, Bookings[18].Id, "Dance the night away",       25m, 140, 1,  opps[1].VenueId,  opps[1].Period,  now.AddDays(-46)),
            ConcertFaker.Post(20, Bookings[19].Id, "Dizzy One",                  20m, 150, 2,  opps[1].VenueId,  opps[1].Period,  now.AddDays(-43)),
            ConcertFaker.Post(21, Bookings[20].Id, "Beers and Boombox",          30m, 170, 5,  opps[1].VenueId,  opps[1].Period,  now.AddDays(-40)),
            ConcertFaker.Post(22, Bookings[21].Id, "Rockin' Tonight!",           16m, 130, 6,  opps[1].VenueId,  opps[1].Period,  now.AddDays(-37)),
            ConcertFaker.Post(23, Bookings[22].Id, "Groovin' All Night",         14m, 115, 1,  opps[2].VenueId,  opps[2].Period,  now.AddDays(-34)),
            ConcertFaker.Post(24, Bookings[23].Id, "Nonstop Vibes",              22m, 135, 2,  opps[2].VenueId,  opps[2].Period,  now.AddDays(-31)),
            ConcertFaker.Post(25, Bookings[24].Id, "Electric Dreams",            13m, 125, 7,  opps[2].VenueId,  opps[2].Period,  now.AddDays(-28)),
            ConcertFaker.Post(26, Bookings[25].Id, "Beat Drop Frenzy",           11m, 120, 8,  opps[2].VenueId,  opps[2].Period,  now.AddDays(-25)),
            ConcertFaker.Post(27, Bookings[26].Id, "Summer Jam",                 19m, 140, 1,  opps[3].VenueId,  opps[3].Period,  now.AddDays(-22)),
            ConcertFaker.Post(28, Bookings[27].Id, "Midnight Madness",           17m, 135, 2,  opps[3].VenueId,  opps[3].Period,  now.AddDays(-19)),
            ConcertFaker.Post(29, Bookings[28].Id, "Like a Boss",                21m, 145, 9,  opps[3].VenueId,  opps[3].Period,  now.AddDays(-16)),
            ConcertFaker.Post(30, Bookings[29].Id, "Lights and Sound",           18m, 140, 10, opps[3].VenueId,  opps[3].Period,  now.AddDays(-13)),
            ConcertFaker.Post(31, Bookings[30].Id, "Rhythm Nation",              26m, 155, 1,  opps[4].VenueId,  opps[4].Period,  now.AddDays(-10)),
            ConcertFaker.Post(32, Bookings[31].Id, "Bass Drop Party",            15m, 120, 2,  opps[4].VenueId,  opps[4].Period,  now.AddDays(-7)),
            ConcertFaker.Post(33, Bookings[32].Id, "Chill & Thrill",             28m, 160, 11, opps[4].VenueId,  opps[4].Period,  now.AddDays(-4)),
            ConcertFaker.Post(34, Bookings[33].Id, "Vibin' till Night",          24m, 150, 12, opps[4].VenueId,  opps[4].Period,  now.AddDays(-1)),
            ConcertFaker.Post(35, Bookings[34].Id, "Rock Your Soul",             23m, 130, 2,  opps[5].VenueId,  opps[5].Period,  now.AddDays(5)),
            ConcertFaker.Post(36, Bookings[35].Id, "Danceaway",                  29m, 155, 13, opps[5].VenueId,  opps[5].Period,  now.AddDays(8)),
            ConcertFaker.Post(37, Bookings[36].Id, "Bassline Groove Beats",      10m, 110, 14, opps[5].VenueId,  opps[5].Period,  now.AddDays(11)),
            ConcertFaker.Post(38, Bookings[37].Id, "Once in a Lifetime!",        15m, 125, 1,  opps[6].VenueId,  opps[6].Period,  now.AddDays(14)),
            ConcertFaker.Post(39, Bookings[38].Id, "Jungle Fever",               30m, 180, 2,  opps[6].VenueId,  opps[6].Period,  now.AddDays(17)),
            ConcertFaker.Post(40, Bookings[39].Id, "Boogie Nights",              20m, 100, 1,  opps[13].VenueId, opps[13].Period, now.AddDays(6)),
            ConcertFaker.Post(41, Bookings[40].Id, "Bass in the Air",            30m, 140, 8,  opps[14].VenueId, opps[14].Period, now.AddDays(18)),
            ConcertFaker.Post(42, Bookings[41].Id, "Jumpin and thumpin",         15m, 100, 11, opps[15].VenueId, opps[15].Period, now.AddDays(22)),
            ConcertFaker.Post(43, Bookings[42].Id, "Groove Night",               18m, 130, 3,  opps[33].VenueId, opps[33].Period, now.AddDays(-1)),
            ConcertFaker.Post(44, Bookings[43].Id, "Electric Midnight",          22m, 140, 1,  opps[34].VenueId, opps[34].Period, now),
            ConcertFaker.Post(45, Bookings[44].Id, "Summer Haze",                20m, 150, 4,  opps[45].VenueId, opps[45].Period, now.AddDays(10)),
            ConcertFaker.Post(46, Bookings[45].Id, "Night Drive",                25m, 160, 5,  opps[46].VenueId, opps[46].Period, now.AddDays(12)),
            ConcertFaker.Post(47, Bookings[46].Id, "Weekend Rush",               15m, 120, 6,  opps[47].VenueId, opps[47].Period, now.AddDays(14)),
        ];
    }

    private sealed record SeedLocation(string County, string Town, double Latitude, double Longitude);

    private static readonly SeedLocation[] Locations =
    [
        new("Leicestershire", "Loughborough", 52.7721, -1.2062),
        new("Greater London", "London", 51.5074, -0.1278),
        new("Greater Manchester", "Manchester", 53.4808, -2.2426),
        new("Surrey", "Guildford", 51.2362, -0.5704),
        new("West Yorkshire", "Leeds", 53.8008, -1.5491),
        new("West Midlands", "Birmingham", 52.4862, -1.8904),
        new("Tyne and Wear", "Newcastle", 54.9783, -1.6178),
        new("South Yorkshire", "Sheffield", 53.3811, -1.4701),
        new("Merseyside", "Liverpool", 53.4084, -2.9916),
        new("Bristol", "Bristol", 51.4545, -2.5879),
        new("Nottinghamshire", "Nottingham", 52.9548, -1.1581),
        new("Hampshire", "Southampton", 50.9097, -1.4043),
        new("Lancashire", "Preston", 53.7632, -2.7031),
        new("Cambridgeshire", "Cambridge", 52.2053, 0.1218),
        new("Oxfordshire", "Oxford", 51.7520, -1.2577),
    ];
}
