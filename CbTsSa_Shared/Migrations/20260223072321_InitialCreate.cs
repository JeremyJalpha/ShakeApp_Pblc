using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CbTsSa_Shared.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SocialMedia = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    POPIConsent = table.Column<bool>(type: "boolean", nullable: true),
                    DtTmJoined = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    CellNumber = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    UserIndicatedCell = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    CurrentOrderJson = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Businesses",
                columns: table => new
                {
                    BusinessID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cellnumber = table.Column<long>(type: "bigint", nullable: false),
                    BusinessName = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    PricelistPreamble = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    NewUserGreeting_Cold = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    NewUserGreeting_Warm = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    RegisteredName = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    Industry = table.Column<byte>(type: "smallint", nullable: false),
                    TradingName = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    VATNumber = table.Column<long>(type: "bigint", nullable: true),
                    StreetAddress = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    PostalSameAsStreet = table.Column<bool>(type: "boolean", nullable: true),
                    PostalAddress = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    Email = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Website = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Facebook = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Twitter = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TradingHours = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Bank = table.Column<byte>(type: "smallint", nullable: true),
                    BranchNumber = table.Column<int>(type: "integer", nullable: true),
                    AccountNumber = table.Column<long>(type: "bigint", nullable: true),
                    AccountType = table.Column<byte>(type: "smallint", nullable: true),
                    GPSLat = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    GPSLong = table.Column<decimal>(type: "numeric(9,6)", nullable: true),
                    LocationLastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AverageRating = table.Column<byte>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Businesses", x => x.BusinessID);
                });

            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    DeliveryID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DtTmCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DtTmDriverAccepted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DriverAcceptedLocation = table.Column<string>(type: "text", nullable: true),
                    DtTmDriverOnScene = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DtTmClosed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.DeliveryID);
                });

            migrationBuilder.CreateTable(
                name: "Goods",
                columns: table => new
                {
                    GoodID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Barcode = table.Column<long>(type: "bigint", nullable: true),
                    ManufacturerID = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    Image = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goods", x => x.GoodID);
                });

            migrationBuilder.CreateTable(
                name: "OfferTypes",
                columns: table => new
                {
                    OfferTypeID = table.Column<byte>(type: "smallint", nullable: false),
                    TypeName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferTypes", x => x.OfferTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Statuses",
                columns: table => new
                {
                    StatusID = table.Column<byte>(type: "smallint", nullable: false),
                    StatusName = table.Column<string>(type: "text", nullable: false),
                    StatusDescription = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statuses", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    DttmIssued = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    UserID = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LastOnline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GPSLat = table.Column<double>(type: "double precision", nullable: true),
                    GPSLong = table.Column<double>(type: "double precision", nullable: true),
                    LocationLastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Drivers_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodPoisoningReports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    IncidentAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FoodItem = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PeopleAffected = table.Column<int>(type: "integer", nullable: false),
                    SeverityLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DateTimeReported = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodPoisoningReports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_FoodPoisoningReports_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    SaleID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    DontPoolWithStrangers = table.Column<bool>(type: "boolean", nullable: false),
                    DtTmRequestedCheckout = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ItemCount = table.Column<int>(type: "integer", nullable: true),
                    IsAmtManual = table.Column<bool>(type: "boolean", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DtTmCompleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.SaleID);
                    table.ForeignKey(
                        name: "FK_Sales_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Specials",
                columns: table => new
                {
                    SpecialID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedByUserID = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    SpecialName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SpecialDescription = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SpecialDtTmStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SpecialDtTmEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specials", x => x.SpecialID);
                    table.ForeignKey(
                        name: "FK_Specials_AspNetUsers_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserIdImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ImageType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    MediaHandle = table.Column<string>(type: "text", nullable: true),
                    StoragePath = table.Column<string>(type: "text", nullable: true),
                    StorageType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    Platform = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    OriginalFilename = table.Column<string>(type: "text", nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIdImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserIdImages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSignUps",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EmergencyContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EmergencyContactNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FirstSignedUpWith = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DateTimeSubmitted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSignUps", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserSignUps_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignImages",
                columns: table => new
                {
                    CampaignImageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessId = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    MediaHandle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    BusinessMediaHandle = table.Column<string>(type: "text", maxLength: 255, nullable: true),
                    Caption = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Platform = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UploadedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LocalStorageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignImages", x => x.CampaignImageId);
                    table.ForeignKey(
                        name: "FK_CampaignImages_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignImages_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Catalogs",
                columns: table => new
                {
                    CatalogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessID = table.Column<long>(type: "bigint", nullable: false),
                    CreatorUserID = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    Description = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalogs", x => x.CatalogID);
                    table.ForeignKey(
                        name: "FK_Catalogs_AspNetUsers_CreatorUserID",
                        column: x => x.CreatorUserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Catalogs_Businesses_BusinessID",
                        column: x => x.BusinessID,
                        principalTable: "Businesses",
                        principalColumn: "BusinessID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessID = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.ServiceID);
                    table.ForeignKey(
                        name: "FK_Services_Businesses_BusinessID",
                        column: x => x.BusinessID,
                        principalTable: "Businesses",
                        principalColumn: "BusinessID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignedUpWith",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    BusinessId = table.Column<long>(type: "bigint", nullable: false),
                    DateTimeSignedUp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignedUpWith", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignedUpWith_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SignedUpWith_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessID = table.Column<long>(type: "bigint", nullable: false),
                    GoodID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                    table.ForeignKey(
                        name: "FK_Products_Businesses_BusinessID",
                        column: x => x.BusinessID,
                        principalTable: "Businesses",
                        principalColumn: "BusinessID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Products_Goods_GoodID",
                        column: x => x.GoodID,
                        principalTable: "Goods",
                        principalColumn: "GoodID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Offers",
                columns: table => new
                {
                    OfferID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OfferTypeID = table.Column<byte>(type: "smallint", nullable: false),
                    CreatedByUserID = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    OfferDtTmStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OfferDtTmEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.OfferID);
                    table.ForeignKey(
                        name: "FK_Offers_AspNetUsers_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Offers_OfferTypes_OfferTypeID",
                        column: x => x.OfferTypeID,
                        principalTable: "OfferTypes",
                        principalColumn: "OfferTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryLegs",
                columns: table => new
                {
                    DeliveryLegID = table.Column<long>(type: "bigint", nullable: false),
                    SaleID = table.Column<long>(type: "bigint", nullable: false),
                    DtTmDriverOnScene = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DtTmCollected = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DtTmArrivedAtCust = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DtTmOTPPassed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DtTmPaid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DtTmDisputed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisputedReason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DtTmResolved = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Resolution = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryLegs", x => new { x.DeliveryLegID, x.SaleID });
                    table.ForeignKey(
                        name: "FK_DeliveryLegs_Sales_SaleID",
                        column: x => x.SaleID,
                        principalTable: "Sales",
                        principalColumn: "SaleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleID = table.Column<long>(type: "bigint", nullable: true),
                    SenderEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CardID = table.Column<long>(type: "bigint", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    StatusID = table.Column<byte>(type: "smallint", nullable: true),
                    InitiationDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PayGateTransactionID = table.Column<int>(type: "integer", nullable: true),
                    PayGateBankAuthID = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SuccesfulDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentID);
                    table.ForeignKey(
                        name: "FK_Payments_Sales_SaleID",
                        column: x => x.SaleID,
                        principalTable: "Sales",
                        principalColumn: "SaleID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SaleStatuses",
                columns: table => new
                {
                    SaleID = table.Column<long>(type: "bigint", nullable: false),
                    ChangedByUserID = table.Column<long>(type: "bigint", nullable: false),
                    DtTmStatusChanged = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NewStatusID = table.Column<byte>(type: "smallint", nullable: false),
                    ChangeReason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleStatuses", x => new { x.SaleID, x.ChangedByUserID, x.DtTmStatusChanged, x.NewStatusID });
                    table.ForeignKey(
                        name: "FK_SaleStatuses_Sales_SaleID",
                        column: x => x.SaleID,
                        principalTable: "Sales",
                        principalColumn: "SaleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleStatuses_Statuses_NewStatusID",
                        column: x => x.NewStatusID,
                        principalTable: "Statuses",
                        principalColumn: "StatusID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BroadcastTemplates",
                columns: table => new
                {
                    TemplateId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    HeaderText = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    BodyText = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    FooterText = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ButtonsJson = table.Column<string>(type: "text", nullable: false),
                    WhatsAppTemplateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CampaignImageId = table.Column<int>(type: "integer", nullable: true),
                    HeaderType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BroadcastTemplates", x => x.TemplateId);
                    table.ForeignKey(
                        name: "FK_BroadcastTemplates_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BroadcastTemplates_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BroadcastTemplates_CampaignImages_CampaignImageId",
                        column: x => x.CampaignImageId,
                        principalTable: "CampaignImages",
                        principalColumn: "CampaignImageId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Saleables",
                columns: table => new
                {
                    SaleableID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServiceID = table.Column<long>(type: "bigint", nullable: true),
                    ProductID = table.Column<long>(type: "bigint", nullable: true),
                    IsService = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Saleables", x => x.SaleableID);
                    table.ForeignKey(
                        name: "FK_Saleables_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Saleables_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryDriverLegs",
                columns: table => new
                {
                    DeliveryDriverLegID = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryLegID = table.Column<long>(type: "bigint", nullable: false),
                    SaleID = table.Column<long>(type: "bigint", nullable: false),
                    DriverID = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    DeliveryID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryDriverLegs", x => new { x.DeliveryDriverLegID, x.DeliveryLegID, x.SaleID, x.DriverID });
                    table.ForeignKey(
                        name: "FK_DeliveryDriverLegs_Deliveries_DeliveryID",
                        column: x => x.DeliveryID,
                        principalTable: "Deliveries",
                        principalColumn: "DeliveryID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryDriverLegs_DeliveryLegs_DeliveryLegID_SaleID",
                        columns: x => new { x.DeliveryLegID, x.SaleID },
                        principalTable: "DeliveryLegs",
                        principalColumns: new[] { "DeliveryLegID", "SaleID" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryDriverLegs_Drivers_DriverID",
                        column: x => x.DriverID,
                        principalTable: "Drivers",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FailedPayments",
                columns: table => new
                {
                    FailedID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentID = table.Column<long>(type: "bigint", nullable: true),
                    IsRefund = table.Column<bool>(type: "boolean", nullable: false),
                    FailureDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PayGateTransactionID = table.Column<int>(type: "integer", nullable: true),
                    PayGateBankAuthID = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PayGateTransactionCode = table.Column<byte>(type: "smallint", nullable: true),
                    PayGateResultCode = table.Column<int>(type: "integer", nullable: true),
                    PayGateValidationErrorCode = table.Column<int>(type: "integer", nullable: true),
                    MopaErrorCode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedPayments", x => x.FailedID);
                    table.ForeignKey(
                        name: "FK_FailedPayments_Payments_PaymentID",
                        column: x => x.PaymentID,
                        principalTable: "Payments",
                        principalColumn: "PaymentID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    RefundID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentID = table.Column<long>(type: "bigint", nullable: true),
                    InitiationDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PayGateTransactionID = table.Column<int>(type: "integer", nullable: true),
                    PayGateBankAuthID = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    SuccesfulDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.RefundID);
                    table.ForeignKey(
                        name: "FK_Refunds_Payments_PaymentID",
                        column: x => x.PaymentID,
                        principalTable: "Payments",
                        principalColumn: "PaymentID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BroadcastCampaigns",
                columns: table => new
                {
                    CampaignId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemplateId = table.Column<int>(type: "integer", nullable: false),
                    BusinessId = table.Column<long>(type: "bigint", nullable: false),
                    InitiatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TotalRecipients = table.Column<int>(type: "integer", nullable: false),
                    SentCount = table.Column<int>(type: "integer", nullable: false),
                    DeliveredCount = table.Column<int>(type: "integer", nullable: false),
                    ReadCount = table.Column<int>(type: "integer", nullable: false),
                    FailedCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BroadcastCampaigns", x => x.CampaignId);
                    table.CheckConstraint("CK_BroadcastCampaign_RecipientCounts", "\"TotalRecipients\" >= 0 AND \"SentCount\" >= 0 AND \"DeliveredCount\" >= 0 AND \"ReadCount\" >= 0 AND \"FailedCount\" >= 0");
                    table.ForeignKey(
                        name: "FK_BroadcastCampaigns_AspNetUsers_InitiatedByUserId",
                        column: x => x.InitiatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BroadcastCampaigns_BroadcastTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "BroadcastTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BroadcastCampaigns_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    SaleableID = table.Column<long>(type: "bigint", nullable: false),
                    CommentText = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Rating = table.Column<byte>(type: "smallint", nullable: true),
                    ResponseToCommentID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentID);
                    table.ForeignKey(
                        name: "FK_Comments_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ResponseToCommentID",
                        column: x => x.ResponseToCommentID,
                        principalTable: "Comments",
                        principalColumn: "CommentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Saleables_SaleableID",
                        column: x => x.SaleableID,
                        principalTable: "Saleables",
                        principalColumn: "SaleableID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Purchasables",
                columns: table => new
                {
                    PurchasableID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleableID = table.Column<long>(type: "bigint", nullable: false),
                    OfferID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchasables", x => x.PurchasableID);
                    table.ForeignKey(
                        name: "FK_Purchasables_Offers_OfferID",
                        column: x => x.OfferID,
                        principalTable: "Offers",
                        principalColumn: "OfferID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Purchasables_Saleables_SaleableID",
                        column: x => x.SaleableID,
                        principalTable: "Saleables",
                        principalColumn: "SaleableID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BroadcastMessages",
                columns: table => new
                {
                    MessageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    RecipientUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    RecipientPhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WhatsAppMessageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BroadcastMessages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_BroadcastMessages_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BroadcastMessages_BroadcastCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "BroadcastCampaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchasableID = table.Column<long>(type: "bigint", nullable: false),
                    SpecialID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_Items_Purchasables_PurchasableID",
                        column: x => x.PurchasableID,
                        principalTable: "Purchasables",
                        principalColumn: "PurchasableID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Items_Specials_SpecialID",
                        column: x => x.SpecialID,
                        principalTable: "Specials",
                        principalColumn: "SpecialID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Bundles",
                columns: table => new
                {
                    BundleID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bundles", x => x.BundleID);
                    table.ForeignKey(
                        name: "FK_Bundles_Items_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Items",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogItems",
                columns: table => new
                {
                    CatalogID = table.Column<long>(type: "bigint", nullable: false),
                    ItemID = table.Column<long>(type: "bigint", nullable: false),
                    MenuCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogItems", x => new { x.CatalogID, x.ItemID });
                    table.ForeignKey(
                        name: "FK_CatalogItems_Catalogs_CatalogID",
                        column: x => x.CatalogID,
                        principalTable: "Catalogs",
                        principalColumn: "CatalogID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogItems_Items_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Items",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GatheredBaskets",
                columns: table => new
                {
                    GatheredBasketID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BundleID = table.Column<long>(type: "bigint", nullable: false),
                    BundleMultiplier = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatheredBaskets", x => x.GatheredBasketID);
                    table.ForeignKey(
                        name: "FK_GatheredBaskets_Bundles_BundleID",
                        column: x => x.BundleID,
                        principalTable: "Bundles",
                        principalColumn: "BundleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdditionalDiscounts",
                columns: table => new
                {
                    GatheredBasketID = table.Column<long>(type: "bigint", nullable: false),
                    AuthorisedByUserID = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountReason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalDiscounts", x => x.GatheredBasketID);
                    table.ForeignKey(
                        name: "FK_AdditionalDiscounts_AspNetUsers_AuthorisedByUserID",
                        column: x => x.AuthorisedByUserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdditionalDiscounts_GatheredBaskets_GatheredBasketID",
                        column: x => x.GatheredBasketID,
                        principalTable: "GatheredBaskets",
                        principalColumn: "GatheredBasketID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EffectiveBaskets",
                columns: table => new
                {
                    EffectiveBasketID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GatheredBasketID = table.Column<long>(type: "bigint", nullable: false),
                    EffectiveDiscount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AuthorisedByUserID = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EffectiveBaskets", x => x.EffectiveBasketID);
                    table.ForeignKey(
                        name: "FK_EffectiveBaskets_AspNetUsers_AuthorisedByUserID",
                        column: x => x.AuthorisedByUserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EffectiveBaskets_GatheredBaskets_GatheredBasketID",
                        column: x => x.GatheredBasketID,
                        principalTable: "GatheredBaskets",
                        principalColumn: "GatheredBasketID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleBaskets",
                columns: table => new
                {
                    SaleID = table.Column<long>(type: "bigint", nullable: false),
                    EffectiveBasketID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleBaskets", x => new { x.SaleID, x.EffectiveBasketID });
                    table.ForeignKey(
                        name: "FK_SaleBaskets_EffectiveBaskets_EffectiveBasketID",
                        column: x => x.EffectiveBasketID,
                        principalTable: "EffectiveBaskets",
                        principalColumn: "EffectiveBasketID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleBaskets_Sales_SaleID",
                        column: x => x.SaleID,
                        principalTable: "Sales",
                        principalColumn: "SaleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalDiscounts_AuthorisedByUserID",
                table: "AdditionalDiscounts",
                column: "AuthorisedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CellNumber",
                table: "AspNetUsers",
                column: "CellNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastCampaigns_BusinessId_Status_CreatedDateTime",
                table: "BroadcastCampaigns",
                columns: new[] { "BusinessId", "Status", "CreatedDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastCampaigns_InitiatedByUserId_CreatedDateTime",
                table: "BroadcastCampaigns",
                columns: new[] { "InitiatedByUserId", "CreatedDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastCampaigns_TemplateId",
                table: "BroadcastCampaigns",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastMessages_CampaignId_Status",
                table: "BroadcastMessages",
                columns: new[] { "CampaignId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastMessages_RecipientUserId",
                table: "BroadcastMessages",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastMessages_Status_CreatedDateTime",
                table: "BroadcastMessages",
                columns: new[] { "Status", "CreatedDateTime" },
                filter: "\"Status\" = 'Queued'");

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastMessages_WhatsAppMessageId",
                table: "BroadcastMessages",
                column: "WhatsAppMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastTemplate_OnePerUser",
                table: "BroadcastTemplates",
                columns: new[] { "BusinessId", "CreatedByUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastTemplates_BusinessId_Name",
                table: "BroadcastTemplates",
                columns: new[] { "BusinessId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastTemplates_BusinessId_Status_CreatedDateTime",
                table: "BroadcastTemplates",
                columns: new[] { "BusinessId", "Status", "CreatedDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastTemplates_CampaignImageId",
                table: "BroadcastTemplates",
                column: "CampaignImageId");

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastTemplates_CreatedByUserId",
                table: "BroadcastTemplates",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BroadcastTemplates_WhatsAppTemplateId",
                table: "BroadcastTemplates",
                column: "WhatsAppTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Bundles_BundleID",
                table: "Bundles",
                column: "BundleID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bundles_ItemID",
                table: "Bundles",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_Cellnumber",
                table: "Businesses",
                column: "Cellnumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_Industry_TradingName",
                table: "Businesses",
                columns: new[] { "Industry", "TradingName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_RegisteredName",
                table: "Businesses",
                column: "RegisteredName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignImage_Business_MediaHandle",
                table: "CampaignImages",
                columns: new[] { "BusinessId", "MediaHandle" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignImage_Business_User_Active",
                table: "CampaignImages",
                columns: new[] { "BusinessId", "UploadedByUserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignImage_OnePerUser",
                table: "CampaignImages",
                columns: new[] { "BusinessId", "UploadedByUserId", "IsActive" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignImages_BusinessId_UploadedDateTime",
                table: "CampaignImages",
                columns: new[] { "BusinessId", "UploadedDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignImages_BusinessMediaHandle",
                table: "CampaignImages",
                column: "BusinessMediaHandle",
                filter: "\"BusinessMediaHandle\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignImages_MediaHandle",
                table: "CampaignImages",
                column: "MediaHandle");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignImages_UploadedByUserId",
                table: "CampaignImages",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItem_Catalog_MenuCode",
                table: "CatalogItems",
                columns: new[] { "CatalogID", "MenuCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_ItemID",
                table: "CatalogItems",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_Catalogs_BusinessID",
                table: "Catalogs",
                column: "BusinessID");

            migrationBuilder.CreateIndex(
                name: "IX_Catalogs_CreatorUserID",
                table: "Catalogs",
                column: "CreatorUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ResponseToCommentID",
                table: "Comments",
                column: "ResponseToCommentID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_SaleableID",
                table: "Comments",
                column: "SaleableID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserID",
                table: "Comments",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryDriverLegs_DeliveryID",
                table: "DeliveryDriverLegs",
                column: "DeliveryID");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryDriverLegs_DeliveryLegID_SaleID",
                table: "DeliveryDriverLegs",
                columns: new[] { "DeliveryLegID", "SaleID" });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryDriverLegs_DriverID",
                table: "DeliveryDriverLegs",
                column: "DriverID");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryLegs_SaleID",
                table: "DeliveryLegs",
                column: "SaleID");

            migrationBuilder.CreateIndex(
                name: "IX_EffectiveBaskets_AuthorisedByUserID",
                table: "EffectiveBaskets",
                column: "AuthorisedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_EffectiveBaskets_GatheredBasketID",
                table: "EffectiveBaskets",
                column: "GatheredBasketID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FailedPayments_PaymentID",
                table: "FailedPayments",
                column: "PaymentID");

            migrationBuilder.CreateIndex(
                name: "IX_FoodPoisoningReports_UserId",
                table: "FoodPoisoningReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GatheredBaskets_BundleID",
                table: "GatheredBaskets",
                column: "BundleID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemID_PurchasableID_SpecialID",
                table: "Items",
                columns: new[] { "ItemID", "PurchasableID", "SpecialID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_PurchasableID",
                table: "Items",
                column: "PurchasableID");

            migrationBuilder.CreateIndex(
                name: "IX_Items_SpecialID",
                table: "Items",
                column: "SpecialID");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_CreatedByUserID",
                table: "Offers",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_OfferTypeID",
                table: "Offers",
                column: "OfferTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SaleID",
                table: "Payments",
                column: "SaleID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BusinessID",
                table: "Products",
                column: "BusinessID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_GoodID",
                table: "Products",
                column: "GoodID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductID",
                table: "Products",
                column: "ProductID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductID_BusinessID",
                table: "Products",
                columns: new[] { "ProductID", "BusinessID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchasables_OfferID",
                table: "Purchasables",
                column: "OfferID");

            migrationBuilder.CreateIndex(
                name: "IX_Purchasables_PurchasableID_SaleableID_OfferID",
                table: "Purchasables",
                columns: new[] { "PurchasableID", "SaleableID", "OfferID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchasables_SaleableID",
                table: "Purchasables",
                column: "SaleableID");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_PaymentID",
                table: "Refunds",
                column: "PaymentID");

            migrationBuilder.CreateIndex(
                name: "IX_Saleables_ProductID",
                table: "Saleables",
                column: "ProductID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Saleables_ServiceID",
                table: "Saleables",
                column: "ServiceID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleBaskets_EffectiveBasketID",
                table: "SaleBaskets",
                column: "EffectiveBasketID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_UserID",
                table: "Sales",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_SaleStatuses_NewStatusID",
                table: "SaleStatuses",
                column: "NewStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Services_BusinessID_Name",
                table: "Services",
                columns: new[] { "BusinessID", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignedUpWith_BusinessId",
                table: "SignedUpWith",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_SignedUpWith_UserBusiness",
                table: "SignedUpWith",
                columns: new[] { "UserId", "BusinessId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Specials_CreatedByUserID",
                table: "Specials",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserIdImage_UserType",
                table: "UserIdImages",
                columns: new[] { "UserId", "ImageType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserIdImages_Status",
                table: "UserIdImages",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdditionalDiscounts");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BroadcastMessages");

            migrationBuilder.DropTable(
                name: "CatalogItems");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "DeliveryDriverLegs");

            migrationBuilder.DropTable(
                name: "FailedPayments");

            migrationBuilder.DropTable(
                name: "FoodPoisoningReports");

            migrationBuilder.DropTable(
                name: "Refunds");

            migrationBuilder.DropTable(
                name: "SaleBaskets");

            migrationBuilder.DropTable(
                name: "SaleStatuses");

            migrationBuilder.DropTable(
                name: "SignedUpWith");

            migrationBuilder.DropTable(
                name: "UserIdImages");

            migrationBuilder.DropTable(
                name: "UserSignUps");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "BroadcastCampaigns");

            migrationBuilder.DropTable(
                name: "Catalogs");

            migrationBuilder.DropTable(
                name: "Deliveries");

            migrationBuilder.DropTable(
                name: "DeliveryLegs");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "EffectiveBaskets");

            migrationBuilder.DropTable(
                name: "Statuses");

            migrationBuilder.DropTable(
                name: "BroadcastTemplates");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "GatheredBaskets");

            migrationBuilder.DropTable(
                name: "CampaignImages");

            migrationBuilder.DropTable(
                name: "Bundles");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Purchasables");

            migrationBuilder.DropTable(
                name: "Specials");

            migrationBuilder.DropTable(
                name: "Offers");

            migrationBuilder.DropTable(
                name: "Saleables");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "OfferTypes");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Goods");

            migrationBuilder.DropTable(
                name: "Businesses");
        }
    }
}
