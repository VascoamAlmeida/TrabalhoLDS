using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LojaMaquinas.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "E_Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "varchar(50)", nullable: false),
                    Password = table.Column<string>(type: "varchar(250)", nullable: false),
                    UserType = table.Column<string>(type: "varchar(20)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_E_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "P_Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_P_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "E_Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(20)", nullable: false),
                    LastName = table.Column<string>(type: "varchar(20)", nullable: false),
                    Address = table.Column<string>(type: "varchar(50)", nullable: false),
                    PostalCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(9)", nullable: false),
                    NIF = table.Column<string>(type: "varchar(9)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_E_Clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_E_Clients_E_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "E_Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "E_Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Brand = table.Column<string>(type: "varchar(30)", nullable: false),
                    Model = table.Column<string>(type: "varchar(30)", nullable: false),
                    YearManufacture = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Image = table.Column<string>(type: "varchar(100)", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_E_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_E_Products_P_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "P_Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "E_Stocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ref = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_E_Stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_E_Stocks_E_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "E_Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "E_Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_E_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_E_Orders_E_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "E_Clients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_E_Orders_E_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "E_Stocks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "E_Maintenances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    Obs = table.Column<string>(type: "varchar(250)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_E_Maintenances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_E_Maintenances_E_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "E_Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "E_OrderRents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InitialDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyImage = table.Column<string>(type: "varchar(100)", nullable: false),
                    UserImage = table.Column<string>(type: "varchar(100)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_E_OrderRents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_E_OrderRents_E_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "E_Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "E_Contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderRentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TermsConditions = table.Column<string>(type: "varchar(250)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_E_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_E_Contracts_E_OrderRents_OrderRentId",
                        column: x => x.OrderRentId,
                        principalTable: "E_OrderRents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_E_Clients_UserId",
                table: "E_Clients",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_E_Contracts_OrderRentId",
                table: "E_Contracts",
                column: "OrderRentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_E_Maintenances_OrderId",
                table: "E_Maintenances",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_E_OrderRents_OrderId",
                table: "E_OrderRents",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_E_Orders_ClientId",
                table: "E_Orders",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_E_Orders_StockId",
                table: "E_Orders",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_E_Products_CategoryId",
                table: "E_Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_E_Stocks_ProductId",
                table: "E_Stocks",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "E_Contracts");

            migrationBuilder.DropTable(
                name: "E_Maintenances");

            migrationBuilder.DropTable(
                name: "E_OrderRents");

            migrationBuilder.DropTable(
                name: "E_Orders");

            migrationBuilder.DropTable(
                name: "E_Clients");

            migrationBuilder.DropTable(
                name: "E_Stocks");

            migrationBuilder.DropTable(
                name: "E_Users");

            migrationBuilder.DropTable(
                name: "E_Products");

            migrationBuilder.DropTable(
                name: "P_Categories");
        }
    }
}
