using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chibest.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseAndFranchiseInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseInvoiceId",
                table: "PurchaseOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FranchiseInvoice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalMoney = table.Column<decimal>(type: "money", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValueSql: "'Draft'::character varying"),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("FranchiseInvoice_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "FranchiseInvoice_BranchId_fkey",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalMoney = table.Column<decimal>(type: "money", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValueSql: "'Draft'::character varying"),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PurchaseInvoice_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "PurchaseInvoice_SupplierId_fkey",
                        column: x => x.SupplierId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FranchiseOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    InvoiceCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalMoney = table.Column<decimal>(type: "money", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValueSql: "'Draft'::character varying"),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    FranchiseInvoiceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("FranchiseOrder_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "FranchiseOrder_BranchId_fkey",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FranchiseOrder_FranchiseInvoiceId_fkey",
                        column: x => x.FranchiseInvoiceId,
                        principalTable: "FranchiseInvoice",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FranchiseOrderDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    ActualQuantity = table.Column<int>(type: "integer", nullable: true),
                    CommissionFee = table.Column<decimal>(type: "money", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "money", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    FranchiseOrderId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("FranchiseOrderDetail_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "FranchiseOrderDetail_FranchiseOrderId_fkey",
                        column: x => x.FranchiseOrderId,
                        principalTable: "FranchiseOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FranchiseOrderDetail_ProductId_fkey",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_purchaseorder_invoiceid",
                table: "PurchaseOrder",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "FranchiseInvoice_Code_key",
                table: "FranchiseInvoice",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FranchiseInvoice_BranchId",
                table: "FranchiseInvoice",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "FranchiseOrder_InvoiceCode_key",
                table: "FranchiseOrder",
                column: "InvoiceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_franchiseorder_branchid",
                table: "FranchiseOrder",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "ix_franchiseorder_invoiceid",
                table: "FranchiseOrder",
                column: "FranchiseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "ix_franchiseorder_orderdate",
                table: "FranchiseOrder",
                column: "OrderDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_franchiseorder_status",
                table: "FranchiseOrder",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "ix_franchiseorderdetail_orderid",
                table: "FranchiseOrderDetail",
                column: "FranchiseOrderId");

            migrationBuilder.CreateIndex(
                name: "ix_franchiseorderdetail_productid",
                table: "FranchiseOrderDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoice_SupplierId",
                table: "PurchaseInvoice",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "PurchaseInvoice_Code_key",
                table: "PurchaseInvoice",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "PurchaseOrder_PurchaseInvoiceId_fkey",
                table: "PurchaseOrder",
                column: "PurchaseInvoiceId",
                principalTable: "PurchaseInvoice",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "PurchaseOrder_PurchaseInvoiceId_fkey",
                table: "PurchaseOrder");

            migrationBuilder.DropTable(
                name: "FranchiseOrderDetail");

            migrationBuilder.DropTable(
                name: "PurchaseInvoice");

            migrationBuilder.DropTable(
                name: "FranchiseOrder");

            migrationBuilder.DropTable(
                name: "FranchiseInvoice");

            migrationBuilder.DropIndex(
                name: "ix_purchaseorder_invoiceid",
                table: "PurchaseOrder");

            migrationBuilder.DropColumn(
                name: "PurchaseInvoiceId",
                table: "PurchaseOrder");
        }
    }
}
