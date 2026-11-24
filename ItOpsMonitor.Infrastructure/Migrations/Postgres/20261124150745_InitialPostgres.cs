using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ItOpsMonitor.Infrastructure.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:ci_hostname", "und-u-ks-level2,und-u-ks-level2,icu,False");

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Hostname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "ci_hostname"),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    Environment = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CpuAlertThreshold = table.Column<int>(type: "integer", nullable: false, defaultValue: 90),
                    CurrentStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Maintenance"),
                    LastHeartbeat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                    table.CheckConstraint("CK_Servers_CpuAlertThreshold", "\"CpuAlertThreshold\" BETWEEN 1 AND 100");
                    table.CheckConstraint("CK_Servers_CurrentStatus", "\"CurrentStatus\" IN ('Healthy', 'Degraded', 'Down', 'Maintenance')");
                    table.CheckConstraint("CK_Servers_Environment", "\"Environment\" IN ('Production', 'Staging', 'Development')");
                    table.CheckConstraint("CK_Servers_Hostname", "LENGTH(\"Hostname\") BETWEEN 1 AND 100");
                });

            migrationBuilder.CreateTable(
                name: "MetricSamples",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CpuUsage = table.Column<double>(type: "double precision", nullable: false),
                    MemoryUsage = table.Column<double>(type: "double precision", nullable: false),
                    Temperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Humidity = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    CollectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricSamples", x => x.Id);
                    table.CheckConstraint("CK_MetricSamples_CpuUsage", "\"CpuUsage\" BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_MetricSamples_Humidity", "\"Humidity\" BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_MetricSamples_MemoryUsage", "\"MemoryUsage\" BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_MetricSamples_Temperature", "\"Temperature\" BETWEEN -20 AND 120");
                    table.ForeignKey(
                        name: "FK_MetricSamples_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetricSamples_ServerId_CollectedAt",
                table: "MetricSamples",
                columns: new[] { "ServerId", "CollectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Servers_Hostname",
                table: "Servers",
                column: "Hostname",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetricSamples");

            migrationBuilder.DropTable(
                name: "Servers");
        }
    }
}
