using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Chibest.Repository.Models;

public class ChiBestDbContext : DbContext
{
    public ChiBestDbContext(DbContextOptions<ChiBestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; } = null!;
    public virtual DbSet<AccountRole> AccountRoles { get; set; } = null!;
    public virtual DbSet<Attendance> Attendances { get; set; } = null!;
    public virtual DbSet<Branch> Branches { get; set; } = null!;
    public virtual DbSet<BranchDebt> BranchDebts { get; set; } = null!;
    public virtual DbSet<BranchDebtHistory> BranchDebtHistories { get; set; } = null!;
    public virtual DbSet<BranchStock> BranchStocks { get; set; } = null!;
    public virtual DbSet<Category> Categories { get; set; } = null!;
    public virtual DbSet<Color> Colors { get; set; } = null!;
    public virtual DbSet<Commission> Commissions { get; set; } = null!;
    public virtual DbSet<Customer> Customers { get; set; } = null!;
    public virtual DbSet<CustomerVoucher> CustomerVouchers { get; set; } = null!;
    public virtual DbSet<Deduction> Deductions { get; set; } = null!;
    public virtual DbSet<Payroll> Payrolls { get; set; } = null!;
    public virtual DbSet<Permission> Permissions { get; set; } = null!;
    public virtual DbSet<Product> Products { get; set; } = null!;
    public virtual DbSet<ProductDetail> ProductDetails { get; set; } = null!;
    public virtual DbSet<ProductPlan> ProductPlans { get; set; } = null!;
    public virtual DbSet<ProductPriceHistory> ProductPriceHistories { get; set; } = null!;
    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; } = null!;
    public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = null!;
    public virtual DbSet<PurchaseReturn> PurchaseReturns { get; set; } = null!;
    public virtual DbSet<PurchaseReturnDetail> PurchaseReturnDetails { get; set; } = null!;
    public virtual DbSet<Role> Roles { get; set; } = null!;
    public virtual DbSet<SalaryConfig> SalaryConfigs { get; set; } = null!;
    public virtual DbSet<SalesOrder> SalesOrders { get; set; } = null!;
    public virtual DbSet<SalesOrderDetail> SalesOrderDetails { get; set; } = null!;
    public virtual DbSet<Size> Sizes { get; set; } = null!;
    public virtual DbSet<StockAdjustment> StockAdjustments { get; set; } = null!;
    public virtual DbSet<StockAdjustmentDetail> StockAdjustmentDetails { get; set; } = null!;
    public virtual DbSet<SupplierDebt> SupplierDebts { get; set; } = null!;
    public virtual DbSet<SupplierDebtHistory> SupplierDebtHistories { get; set; } = null!;
    public virtual DbSet<TransferOrder> TransferOrders { get; set; } = null!;
    public virtual DbSet<TransferOrderDetail> TransferOrderDetails { get; set; } = null!;
    public virtual DbSet<Voucher> Vouchers { get; set; } = null!;
    public virtual DbSet<WorkShift> WorkShifts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
 
