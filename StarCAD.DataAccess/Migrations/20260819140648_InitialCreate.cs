using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StarCAD.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlateCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CodeGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GasDistributionGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GasDistributionGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InsuranceCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceCompanies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectFirms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyType = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    NationalIdNumber = table.Column<byte[]>(type: "varbinary(256)", maxLength: 256, nullable: true),
                    NationalIdNumberHash = table.Column<byte[]>(type: "varbinary(32)", maxLength: 32, nullable: true),
                    AccountingCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Phone2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectFirms", x => x.Id);
                    table.CheckConstraint("CK_ProjectFirm_TypeIdentity", "([CompanyType] = 1 AND [NationalIdNumber] IS NOT NULL) OR ([CompanyType] = 2 AND [TaxNumber] IS NOT NULL)");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Codes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CodeValue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Codes_CodeGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "CodeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GasDistributionFirms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CompanyNumber = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GasDistributionFirms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GasDistributionFirms_GasDistributionGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "GasDistributionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    DistrictId = table.Column<int>(type: "int", nullable: true),
                    AddressLine = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FloorCount = table.Column<byte>(type: "tinyint", nullable: true),
                    BasementCount = table.Column<byte>(type: "tinyint", nullable: true),
                    BlockLotParcel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buildings_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Buildings_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectFirmAuthorizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectFirmId = table.Column<int>(type: "int", nullable: false),
                    GasDistributionFirmId = table.Column<int>(type: "int", nullable: false),
                    CertificateNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectFirmAuthorizations", x => x.Id);
                    table.CheckConstraint("CK_ProjectFirmAuthorization_Date", "[ValidTo] IS NULL OR [ValidTo] > [ValidFrom]");
                    table.ForeignKey(
                        name: "FK_ProjectFirmAuthorizations_GasDistributionFirms_GasDistributionFirmId",
                        column: x => x.GasDistributionFirmId,
                        principalTable: "GasDistributionFirms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectFirmAuthorizations_ProjectFirms_ProjectFirmId",
                        column: x => x.ProjectFirmId,
                        principalTable: "ProjectFirms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectFirmId = table.Column<int>(type: "int", nullable: true),
                    GasDistributionFirmId = table.Column<int>(type: "int", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    TokenVersion = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.CheckConstraint("CK_Users_SingleFirmReference", "[ProjectFirmId] IS NULL OR [GasDistributionFirmId] IS NULL");
                    table.ForeignKey(
                        name: "FK_Users_GasDistributionFirms_GasDistributionFirmId",
                        column: x => x.GasDistributionFirmId,
                        principalTable: "GasDistributionFirms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_ProjectFirms_ProjectFirmId",
                        column: x => x.ProjectFirmId,
                        principalTable: "ProjectFirms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectFirmAuthorizationId = table.Column<int>(type: "int", nullable: false),
                    OwnerUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    BuildingId = table.Column<int>(type: "int", nullable: true),
                    StatusCodeId = table.Column<int>(type: "int", nullable: true),
                    ProjectTypeCodeId = table.Column<int>(type: "int", nullable: true),
                    HeatingTypeCodeId = table.Column<int>(type: "int", nullable: true),
                    BuildingUsageTypeCodeId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TotalArea = table.Column<int>(type: "int", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    IsOfficial = table.Column<bool>(type: "bit", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsPermitProject = table.Column<bool>(type: "bit", nullable: false),
                    ApartmentCount = table.Column<int>(type: "int", nullable: true),
                    WorkplaceCount = table.Column<int>(type: "int", nullable: true),
                    ServiceBoxPressureMbar = table.Column<int>(type: "int", nullable: true),
                    CoverNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConnectionObject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Codes_BuildingUsageTypeCodeId",
                        column: x => x.BuildingUsageTypeCodeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Codes_HeatingTypeCodeId",
                        column: x => x.HeatingTypeCodeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Codes_ProjectTypeCodeId",
                        column: x => x.ProjectTypeCodeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Codes_StatusCodeId",
                        column: x => x.StatusCodeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_ProjectFirmAuthorizations_ProjectFirmAuthorizationId",
                        column: x => x.ProjectFirmAuthorizationId,
                        principalTable: "ProjectFirmAuthorizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Docs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocTypeCodeId = table.Column<int>(type: "int", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    ProjectFirmId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Docs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Docs_Codes_DocTypeCodeId",
                        column: x => x.DocTypeCodeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Docs_ProjectFirms_ProjectFirmId",
                        column: x => x.ProjectFirmId,
                        principalTable: "ProjectFirms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Docs_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Docs_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OperationHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OperationCodeId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RoleSnapshot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationHistories_Codes_OperationCodeId",
                        column: x => x.OperationCodeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationHistories_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperationHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    UnitNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SubscriberNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Area = table.Column<int>(type: "int", nullable: true),
                    SourceMeterElementId = table.Column<int>(type: "int", nullable: true),
                    SubscriberName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MeterClassLabel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FlowCubicMeterPerHour = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    PressureMbar = table.Column<int>(type: "int", nullable: true),
                    PipeTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectUnits_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    JsonToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContentHash = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectVersions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectVersions_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectUnitId = table.Column<int>(type: "int", nullable: false),
                    DeviceTypeCodeId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_Codes_DeviceTypeCodeId",
                        column: x => x.DeviceTypeCodeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Devices_ProjectUnits_ProjectUnitId",
                        column: x => x.ProjectUnitId,
                        principalTable: "ProjectUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocProjectUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocId = table.Column<int>(type: "int", nullable: false),
                    ProjectUnitId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ProjectFirmId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocProjectUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocProjectUnits_Docs_DocId",
                        column: x => x.DocId,
                        principalTable: "Docs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocProjectUnits_ProjectFirms_ProjectFirmId",
                        column: x => x.ProjectFirmId,
                        principalTable: "ProjectFirms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocProjectUnits_ProjectUnits_ProjectUnitId",
                        column: x => x.ProjectUnitId,
                        principalTable: "ProjectUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocProjectUnits_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectUnitId = table.Column<int>(type: "int", nullable: false),
                    InsuranceCompanyId = table.Column<int>(type: "int", nullable: true),
                    PolicyNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPrinted = table.Column<bool>(type: "bit", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Policies_InsuranceCompanies_InsuranceCompanyId",
                        column: x => x.InsuranceCompanyId,
                        principalTable: "InsuranceCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Policies_ProjectUnits_ProjectUnitId",
                        column: x => x.ProjectUnitId,
                        principalTable: "ProjectUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CodeGroups",
                columns: new[] { "Id", "GroupName", "IsActive" },
                values: new object[,]
                {
                    { 1, "ArchitecturalPaletteTool", true },
                    { 2, "ProjectStatus", true },
                    { 3, "InstallationPaletteTool", true },
                    { 5, "DocumentType", true },
                    { 7, "ProjectType", true },
                    { 8, "HeatingType", true },
                    { 9, "BuildingUsageType", true },
                    { 32, "ServiceBoxModel", true }
                });

            migrationBuilder.InsertData(
                table: "Codes",
                columns: new[] { "Id", "CodeValue", "GroupId", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1001, "Switch", 1, true, "Ana Kesme Şalteri Ekle" },
                    { 1002, "Wall", 1, true, "Duvar Çiz" },
                    { 1003, "Panel", 1, true, "Pano Ekle" },
                    { 1004, "Room", 1, true, "Oda Çiz" },
                    { 1005, "Lighting", 1, true, "Aydınlatma Ekle" },
                    { 1006, "Door", 1, true, "Kapı Ekle" },
                    { 1007, "FireExtinguisher", 1, true, "Yangın Söndürücü Ekle" },
                    { 1008, "Window", 1, true, "Pencere Ekle" },
                    { 1009, "Eraser", 1, true, "Silgi" },
                    { 1010, "Staircase", 1, true, "Merdiven Ekle" },
                    { 1011, "BulkDelete", 1, true, "Toplu Silme" },
                    { 1012, "Column", 1, true, "Kolon Ekle" },
                    { 1013, "Text", 1, true, "Metin Ekle" },
                    { 1014, "Beam", 1, true, "Kiriş Ekle" },
                    { 1015, "Measure", 1, true, "Ölçüm" },
                    { 1016, "Vent", 1, true, "Menfez Ekle" },
                    { 1017, "FreeDraw", 1, true, "Serbest Çizim Araçları" },
                    { 1018, "RoofVent", 1, true, "Kolon Havalandırması Ekle" },
                    { 1019, "Alarm", 1, true, "Alarm Cihazı Ekle" },
                    { 1020, "EarthquakeSensor", 1, true, "Deprem Sensörü Ekle" },
                    { 1021, "FlueShaft", 1, true, "Baca Şaftı Ekle" },
                    { 2001, "Draft", 2, true, "Taslak" },
                    { 2002, "Approved", 2, true, "Onaylanan" },
                    { 2003, "PendingApproval", 2, true, "Onay Bekleyen" },
                    { 2004, "Rejected", 2, true, "Reddedilen" },
                    { 3001, "Regulator", 3, true, "Regülatör Ekle" },
                    { 3002, "Pipe", 3, true, "Boru Ekle" },
                    { 3003, "FluePoint", 3, true, "Baca Ekle" },
                    { 3004, "BranchPipe", 3, true, "Branşman Ekle" },
                    { 3005, "Isolation", 3, true, "İzolasyon Ekle" },
                    { 3006, "GasMeter", 3, true, "Sayaç Ekle" },
                    { 3007, "Manometer", 3, true, "Manometre Ekle" },
                    { 3008, "ServiceBox", 3, true, "Servis Kutusu Ekle" },
                    { 3009, "Filter", 3, true, "Filtre / Kit Ekle" },
                    { 3010, "Valve", 3, true, "Vana Ekle" },
                    { 3011, "DumbGasMeter", 3, true, "Süzme Sayaç Ekle" },
                    { 3012, "SolenoidValve", 3, true, "Selenoid Vana Ekle" },
                    { 3013, "VentilationPoint", 3, true, "Havalandırma Kanalı Ekle" },
                    { 3014, "Cooker", 3, true, "Ocak Ekle" },
                    { 3015, "Measure", 3, true, "Ölçüm" },
                    { 3201, "CES200", 32, true, "CES 200" },
                    { 3202, "S200", 32, true, "S 200" },
                    { 3203, "S300", 32, true, "S 300" },
                    { 5001, "BacaReport", 5, true, "Baca Raporu" },
                    { 5002, "DeviceFlueExhaustCertificate", 5, true, "Cihaz Baca Atış Belgesi" },
                    { 5003, "MinimumConsumptionDeclaration", 5, true, "Cihaz Minimum Tüketim Beyanı" },
                    { 5004, "DeviceServiceReport", 5, true, "Cihaz Servis Kontrol Raporu" },
                    { 5005, "DeviceStandardCertificate", 5, true, "Cihaz Standart Belgesi" },
                    { 5006, "DeviceComplianceCertificate", 5, true, "Cihaz Uygunluk Belgesi" },
                    { 5007, "DaskPolicy", 5, true, "DASK Poliçesi" },
                    { 5008, "NaturalGasCompliance", 5, true, "Doğalgaz Uygunluk Belgesi" },
                    { 5009, "FlexibleInstallationTraining", 5, true, "Esnek Tesisat Eğitim Belgesi" },
                    { 5010, "FlexibleInstallationMYK", 5, true, "Esnek Tesisat MYK Belgesi" },
                    { 5011, "FavoriteDocument", 5, true, "Favori Evrak" },
                    { 5012, "GasCompetencyCertificate", 5, true, "Gaz Yeterlilik Belgesi" },
                    { 5013, "GeneralDocument", 5, true, "Genel Evrak" },
                    { 5014, "LocationComplianceCertificate", 5, true, "Mahal Uygunluk Belgesi" },
                    { 5015, "CustomerAgreement", 5, true, "Müşteri Sözleşmesi" },
                    { 5016, "AddressNumbering", 5, true, "Numarataj" },
                    { 5017, "InsurancePolicy", 5, true, "Poliçe" },
                    { 5018, "Image", 5, true, "Resim" },
                    { 5019, "License", 5, true, "Ruhsat" },
                    { 7001, "ILAVE", 7, true, "İlave" },
                    { 7002, "ILAVE_TADILAT", 7, true, "İlave Tadilat" },
                    { 7003, "KOLON", 7, true, "Kolon" },
                    { 7004, "KOLON_TADILAT", 7, true, "Kolon Tadilat" },
                    { 8001, "bireysel", 8, true, "Bireysel" },
                    { 8002, "merkezi", 8, true, "Merkezi" },
                    { 9001, "coklu", 9, true, "Çoklu" },
                    { 9002, "mustakil", 9, true, "Müstakil" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_CityId",
                table: "Buildings",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_Code",
                table: "Buildings",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_DistrictId",
                table: "Buildings",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name",
                table: "Cities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodeGroups_GroupName",
                table: "CodeGroups",
                column: "GroupName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Codes_GroupId_CodeValue",
                table: "Codes",
                columns: new[] { "GroupId", "CodeValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceTypeCodeId",
                table: "Devices",
                column: "DeviceTypeCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_ProjectUnitId",
                table: "Devices",
                column: "ProjectUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_CityId_Name",
                table: "Districts",
                columns: new[] { "CityId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocProjectUnits_DocId_ProjectUnitId",
                table: "DocProjectUnits",
                columns: new[] { "DocId", "ProjectUnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocProjectUnits_ProjectFirmId",
                table: "DocProjectUnits",
                column: "ProjectFirmId");

            migrationBuilder.CreateIndex(
                name: "IX_DocProjectUnits_ProjectId",
                table: "DocProjectUnits",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DocProjectUnits_ProjectUnitId",
                table: "DocProjectUnits",
                column: "ProjectUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Docs_DocTypeCodeId",
                table: "Docs",
                column: "DocTypeCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Docs_ProjectFirmId",
                table: "Docs",
                column: "ProjectFirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Docs_ProjectId",
                table: "Docs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Docs_UploadedByUserId",
                table: "Docs",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GasDistributionFirms_CompanyNumber",
                table: "GasDistributionFirms",
                column: "CompanyNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GasDistributionFirms_GroupId",
                table: "GasDistributionFirms",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GasDistributionGroups_Name",
                table: "GasDistributionGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperationHistories_OperationCodeId",
                table: "OperationHistories",
                column: "OperationCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationHistories_ProjectId",
                table: "OperationHistories",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationHistories_UserId",
                table: "OperationHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_InsuranceCompanyId",
                table: "Policies",
                column: "InsuranceCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_ProjectUnitId",
                table: "Policies",
                column: "ProjectUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFirmAuthorizations_GasDistributionFirmId",
                table: "ProjectFirmAuthorizations",
                column: "GasDistributionFirmId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFirmAuthorizations_ProjectFirmId_GasDistributionFirmId",
                table: "ProjectFirmAuthorizations",
                columns: new[] { "ProjectFirmId", "GasDistributionFirmId" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFirms_NationalIdNumberHash",
                table: "ProjectFirms",
                column: "NationalIdNumberHash",
                unique: true,
                filter: "[NationalIdNumberHash] IS NOT NULL AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_BuildingId",
                table: "Projects",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_BuildingUsageTypeCodeId",
                table: "Projects",
                column: "BuildingUsageTypeCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedByUserId",
                table: "Projects",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_HeatingTypeCodeId",
                table: "Projects",
                column: "HeatingTypeCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OwnerUserId",
                table: "Projects",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectFirmAuthorizationId",
                table: "Projects",
                column: "ProjectFirmAuthorizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectTypeCodeId",
                table: "Projects",
                column: "ProjectTypeCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_StatusCodeId",
                table: "Projects",
                column: "StatusCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UpdatedByUserId",
                table: "Projects",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectUnits_ProjectId",
                table: "ProjectUnits",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectUnits_ProjectId_SourceMeterElementId",
                table: "ProjectUnits",
                columns: new[] { "ProjectId", "SourceMeterElementId" },
                unique: true,
                filter: "[SourceMeterElementId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersions_CreatedByUserId",
                table: "ProjectVersions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVersions_ProjectId",
                table: "ProjectVersions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Code",
                table: "Roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GasDistributionFirmId",
                table: "Users",
                column: "GasDistributionFirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProjectFirmId",
                table: "Users",
                column: "ProjectFirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "DocProjectUnits");

            migrationBuilder.DropTable(
                name: "OperationHistories");

            migrationBuilder.DropTable(
                name: "Policies");

            migrationBuilder.DropTable(
                name: "ProjectVersions");

            migrationBuilder.DropTable(
                name: "Docs");

            migrationBuilder.DropTable(
                name: "InsuranceCompanies");

            migrationBuilder.DropTable(
                name: "ProjectUnits");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Buildings");

            migrationBuilder.DropTable(
                name: "Codes");

            migrationBuilder.DropTable(
                name: "ProjectFirmAuthorizations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "CodeGroups");

            migrationBuilder.DropTable(
                name: "GasDistributionFirms");

            migrationBuilder.DropTable(
                name: "ProjectFirms");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "GasDistributionGroups");
        }
    }
}
