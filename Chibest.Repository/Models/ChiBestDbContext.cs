using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Chibest.Repository.Models;

public partial class ChiBestDbContext : DbContext
{
    public ChiBestDbContext()
    {
    }

    public ChiBestDbContext(DbContextOptions<ChiBestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountRole> AccountRoles { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<BranchDebt> BranchDebts { get; set; }

    public virtual DbSet<BranchDebtHistory> BranchDebtHistories { get; set; }

    public virtual DbSet<BranchStock> BranchStocks { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Commission> Commissions { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerVoucher> CustomerVouchers { get; set; }

    public virtual DbSet<Deduction> Deductions { get; set; }

    public virtual DbSet<Payroll> Payrolls { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductDetail> ProductDetails { get; set; }

    public virtual DbSet<ProductPriceHistory> ProductPriceHistories { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }

    public virtual DbSet<PurchaseReturn> PurchaseReturns { get; set; }

    public virtual DbSet<PurchaseReturnDetail> PurchaseReturnDetails { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SalaryConfig> SalaryConfigs { get; set; }

    public virtual DbSet<SalesOrder> SalesOrders { get; set; }

    public virtual DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }

    public virtual DbSet<StockAdjustment> StockAdjustments { get; set; }

    public virtual DbSet<StockAdjustmentDetail> StockAdjustmentDetails { get; set; }

    public virtual DbSet<SupplierDebt> SupplierDebts { get; set; }

    public virtual DbSet<SupplierDebtHistory> SupplierDebtHistories { get; set; }

    public virtual DbSet<SystemLog> SystemLogs { get; set; }

    public virtual DbSet<TransferOrder> TransferOrders { get; set; }

    public virtual DbSet<TransferOrderDetail> TransferOrderDetails { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    public virtual DbSet<WorkShift> WorkShifts { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC0790BD8EB5");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Email, "IX_Account_Email");

            entity.HasIndex(e => e.PhoneNumber, "IX_Account_PhoneNumber");

            entity.HasIndex(e => e.Code, "UQ__Account__A25C5AA74C65649B").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Account__A9D10534690E5782").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AvatarUrl).HasColumnName("AvatarURL");
            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .HasColumnName("CCCD");
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FaxNumber).HasMaxLength(15);
            entity.Property(e => e.FcmToken)
                .HasMaxLength(255)
                .HasColumnName("fcmToken");
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.RefreshTokenExpiryTime).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Hoạt Động");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<AccountRole>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.RoleId, e.StartDate });

            entity.ToTable("AccountRole");

            entity.HasIndex(e => new { e.AccountId, e.StartDate }, "IX_AccountRole_AccountId").IsDescending(false, true);

            entity.HasIndex(e => new { e.BranchId, e.RoleId }, "IX_AccountRole_BranchId_RoleId");

            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK_AccountRole_Account");

            entity.HasOne(d => d.Branch).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AccountRole_Branch");

            entity.HasOne(d => d.Role).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_AccountRole_Role");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC0786B2BBCB");

            entity.ToTable("Attendance");

            entity.HasIndex(e => new { e.BranchId, e.WorkDate }, "IX_Attendance_BranchId").IsDescending(false, true);

            entity.HasIndex(e => new { e.EmployeeId, e.WorkDate }, "IX_Attendance_EmployeeId").IsDescending(false, true);

            entity.HasIndex(e => new { e.EmployeeId, e.WorkDate }, "UQ_Attendance_Employee_Date").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AttendanceStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Có Mặt");
            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.CheckOutTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DayType)
                .HasMaxLength(50)
                .HasDefaultValue("Ngày Thường");
            entity.Property(e => e.OvertimeHours).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.WorkHours).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Branch).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Branc__3FD07829");

            entity.HasOne(d => d.Employee).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Emplo__3EDC53F0");

            entity.HasOne(d => d.WorkShift).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.WorkShiftId)
                .HasConstraintName("FK__Attendanc__WorkS__40C49C62");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Branch__3214EC0734AFBD3B");

            entity.ToTable("Branch");

            entity.HasIndex(e => e.Code, "UQ__Branch__A25C5AA794713A0E").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.OwnerName).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Hoạt Động");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<BranchDebt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BranchDe__3214EC07F0E88BC4");

            entity.ToTable("BranchDebt");

            entity.HasIndex(e => e.BranchId, "UQ_BranchDebt_Branch").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.LastTransactionDate).HasColumnType("datetime");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaidAmount).HasColumnType("money");
            entity.Property(e => e.RemainingDebt)
                .HasComputedColumnSql("([TotalDebt]-[PaidAmount])", true)
                .HasColumnType("money");
            entity.Property(e => e.TotalDebt).HasColumnType("money");

            entity.HasOne(d => d.Branch).WithOne(p => p.BranchDebt)
                .HasForeignKey<BranchDebt>(d => d.BranchId)
                .HasConstraintName("FK__BranchDeb__Branc__1C873BEC");
        });

        modelBuilder.Entity<BranchDebtHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BranchDe__3214EC075A074A57");

            entity.ToTable("BranchDebtHistory");

            entity.HasIndex(e => new { e.BranchDebtId, e.TransactionDate }, "IX_BranchDebtHistory_Branch").IsDescending(false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.BalanceAfter).HasColumnType("money");
            entity.Property(e => e.BalanceBefore).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TransactionType).HasMaxLength(50);

            entity.HasOne(d => d.BranchDebt).WithMany(p => p.BranchDebtHistories)
                .HasForeignKey(d => d.BranchDebtId)
                .HasConstraintName("FK__BranchDeb__Branc__2334397B");
        });

        modelBuilder.Entity<BranchStock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BranchSt__3214EC0745DDD36B");

            entity.ToTable("BranchStock");

            entity.HasIndex(e => new { e.BranchId, e.AvailableQty }, "IX_BranchStock_BranchId");

            entity.HasIndex(e => e.ProductId, "IX_BranchStock_ProductId");

            entity.HasIndex(e => new { e.ProductId, e.BranchId, e.WarehouseId }, "UQ_BranchStock_Product_Branch").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CurrentSellingPrice).HasColumnType("money");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalQty).HasComputedColumnSql("((([AvailableQty]+[ReservedQty])+[InTransitQty])+[DefectiveQty])", true);

            entity.HasOne(d => d.Branch).WithMany(p => p.BranchStocks)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__BranchSto__Branc__7A672E12");

            entity.HasOne(d => d.Product).WithMany(p => p.BranchStocks)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__BranchSto__Produ__797309D9");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.BranchStocks)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__BranchSto__Wareh__7B5B524B");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC077A27DE6F");

            entity.ToTable("Category");

            entity.HasIndex(e => e.ParentId, "IX_Category_ParentId");

            entity.HasIndex(e => new { e.Type, e.Name }, "IX_Category_Type_Name");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK__Category__Parent__628FA481");
        });

        modelBuilder.Entity<Commission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Commissi__3214EC077C3CD5A5");

            entity.ToTable("Commission");

            entity.HasIndex(e => new { e.EmployeeId, e.PeriodYear, e.PeriodMonth }, "IX_Commission_EmployeeId").IsDescending(false, true, true);

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.CalculationBase).HasColumnType("money");
            entity.Property(e => e.CommissionRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CommissionType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReferenceType).HasMaxLength(50);

            entity.HasOne(d => d.Employee).WithMany(p => p.Commissions)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Commissio__Emplo__4A4E069C");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC0759C8B5BC");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.Email, "IX_Customer_Email");

            entity.HasIndex(e => e.GroupId, "IX_Customer_GroupId");

            entity.HasIndex(e => e.PhoneNumber, "IX_Customer_PhoneNumber");

            entity.HasIndex(e => e.Code, "UQ__Customer__A25C5AA74B446B56").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AvatarUrl).HasColumnName("AvatarURL");
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DateOfBirth).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.LastActive)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Hoạt Động");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Group).WithMany(p => p.InverseGroup)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__Customer__GroupI__5EBF139D");
        });

        modelBuilder.Entity<CustomerVoucher>(entity =>
        {
            entity.HasKey(e => new { e.VoucherId, e.CustomerId }).HasName("PK__Customer__A0A49F6C54259BB7");

            entity.ToTable("CustomerVoucher");

            entity.Property(e => e.CollectedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Đã Nhận");
            entity.Property(e => e.UsedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerV__Custo__6CD828CA");

            entity.HasOne(d => d.Voucher).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerV__Vouch__6BE40491");
        });

        modelBuilder.Entity<Deduction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Deductio__3214EC0748A8940C");

            entity.ToTable("Deduction");

            entity.HasIndex(e => new { e.EmployeeId, e.PeriodYear, e.PeriodMonth }, "IX_Deduction_EmployeeId").IsDescending(false, true, true);

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeductionType).HasMaxLength(50);

            entity.HasOne(d => d.Employee).WithMany(p => p.Deductions)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Deduction__Emplo__4F12BBB9");
        });

        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payroll__3214EC07FC0BCC78");

            entity.ToTable("Payroll");

            entity.HasIndex(e => new { e.EmployeeId, e.PeriodYear, e.PeriodMonth }, "IX_Payroll_EmployeeId").IsDescending(false, true, true);

            entity.HasIndex(e => new { e.PaymentStatus, e.PeriodYear, e.PeriodMonth }, "IX_Payroll_Status").IsDescending(false, true, true);

            entity.HasIndex(e => new { e.EmployeeId, e.PeriodYear, e.PeriodMonth }, "UQ_Payroll_Employee_Period").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ActualBaseSalary).HasColumnType("money");
            entity.Property(e => e.BaseSalary).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.GrossSalary)
                .HasComputedColumnSql("(((([ActualBaseSalary]+[TotalAllowance])+[OvertimeSalary])+[TotalCommission])+[TotalBonus])", true)
                .HasColumnType("money");
            entity.Property(e => e.HealthInsurance).HasColumnType("money");
            entity.Property(e => e.NetSalary)
                .HasComputedColumnSql("(((((((([ActualBaseSalary]+[TotalAllowance])+[OvertimeSalary])+[TotalCommission])+[TotalBonus])-[TotalDeduction])-[SocialInsurance])-[HealthInsurance])-[UnemploymentInsurance])", true)
                .HasColumnType("money");
            entity.Property(e => e.OvertimeSalary).HasColumnType("money");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasDefaultValue("Chuyển Khoản");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Chờ Thanh Toán");
            entity.Property(e => e.SocialInsurance).HasColumnType("money");
            entity.Property(e => e.StandardWorkDays).HasDefaultValue(26);
            entity.Property(e => e.TotalAllowance).HasColumnType("money");
            entity.Property(e => e.TotalBonus).HasColumnType("money");
            entity.Property(e => e.TotalCommission).HasColumnType("money");
            entity.Property(e => e.TotalDeduction).HasColumnType("money");
            entity.Property(e => e.TotalOvertimeHours).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalWorkHours).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnemploymentInsurance).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Branch).WithMany(p => p.Payrolls)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payroll__BranchI__55BFB948");

            entity.HasOne(d => d.Employee).WithMany(p => p.Payrolls)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payroll__Employe__54CB950F");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3214EC07A3157F15");

            entity.ToTable("Product");

            entity.HasIndex(e => e.CategoryId, "IX_Product_CategoryId");

            entity.HasIndex(e => e.Name, "IX_Product_Name");

            entity.HasIndex(e => e.ParentSku, "IX_Product_ParentSKU");

            entity.HasIndex(e => e.Sku, "UQ__Product__CA1ECF0DECE7A616").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AvatarUrl).HasColumnName("AvatarURL");
            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsMaster).HasDefaultValue(true);
            entity.Property(e => e.Material).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.ParentSku)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ParentSKU");
            entity.Property(e => e.Size).HasMaxLength(100);
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SKU");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Khả Dụng");
            entity.Property(e => e.Style).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Product__Categor__6C190EBB");

            entity.HasOne(d => d.ParentSkuNavigation).WithMany(p => p.InverseParentSkuNavigation)
                .HasPrincipalKey(p => p.Sku)
                .HasForeignKey(d => d.ParentSku)
                .HasConstraintName("FK__Product__ParentS__6D0D32F4");
        });

        modelBuilder.Entity<ProductDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductD__3214EC0719A4A623");

            entity.ToTable("ProductDetail");

            entity.HasIndex(e => new { e.BranchId, e.WarehouseId }, "IX_ProductDetail_BranchId_WarehouseId");

            entity.HasIndex(e => e.ChipCode, "IX_ProductDetail_ChipCode");

            entity.HasIndex(e => new { e.ProductId, e.Status }, "IX_ProductDetail_ProductId_Status");

            entity.HasIndex(e => e.ChipCode, "UQ__ProductD__D7FC520A60B5BC37").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ChipCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImportDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LastTransactionDate).HasColumnType("datetime");
            entity.Property(e => e.LastTransactionType).HasMaxLength(50);
            entity.Property(e => e.PurchasePrice).HasColumnType("money");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Khả Dụng");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Branch).WithMany(p => p.ProductDetails)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__ProductDe__Branc__0A9D95DB");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductDe__Produ__09A971A2");

            entity.HasOne(d => d.Supplier).WithMany(p => p.ProductDetails)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__ProductDe__Suppl__0E6E26BF");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.ProductDetails)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__ProductDe__Wareh__0B91BA14");
        });

        modelBuilder.Entity<ProductPriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductP__3214EC073238A0B0");

            entity.ToTable("ProductPriceHistory");

            entity.HasIndex(e => new { e.ProductId, e.BranchId, e.EffectiveDate }, "IX_ProductPriceHistory_Product_Branch").IsDescending(false, false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CostPrice).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EffectiveDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.SellingPrice).HasColumnType("money");

            entity.HasOne(d => d.Branch).WithMany(p => p.ProductPriceHistories)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__ProductPr__Branc__74AE54BC");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ProductPriceHistories)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__ProductPr__Creat__72C60C4A");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductPriceHistories)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductPr__Produ__73BA3083");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Purchase__3214EC0723231B0A");

            entity.ToTable("PurchaseOrder");

            entity.HasIndex(e => e.InvoiceCode, "IX_PurchaseOrder_InvoiceCode");

            entity.HasIndex(e => e.OrderDate, "IX_TransactionOrder_OrderDate").IsDescending();

            entity.HasIndex(e => e.Status, "IX_TransactionOrder_Status");

            entity.HasIndex(e => e.InvoiceCode, "UQ__Purchase__0D9D7FF37F007872").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount).HasColumnType("money");
            entity.Property(e => e.InvoiceCode).HasMaxLength(100);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Paid).HasColumnType("money");
            entity.Property(e => e.PayMethod)
                .HasMaxLength(40)
                .HasDefaultValue("Tiền Mặt");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Ch? X? Lý");
            entity.Property(e => e.SubTotal).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.PurchaseOrderEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__PurchaseO__Emplo__1EA48E88");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseOrderSuppliers)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__PurchaseO__Suppl__1F98B2C1");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__PurchaseO__Wareh__1DB06A4F");
        });

        modelBuilder.Entity<PurchaseOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Purchase__3214EC071B203675");

            entity.ToTable("PurchaseOrderDetail");

            entity.HasIndex(e => e.PurchaseOrderId, "IX_TransactionOrderDetail_OrderId");

            entity.HasIndex(e => e.ProductId, "IX_TransactionOrderDetail_ProductId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Discount).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ReFee).HasColumnType("money");
            entity.Property(e => e.UnitPrice).HasColumnType("money");

            entity.HasOne(d => d.Product).WithMany(p => p.PurchaseOrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__PurchaseO__Produ__25518C17");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.PurchaseOrderDetails)
                .HasForeignKey(d => d.PurchaseOrderId)
                .HasConstraintName("FK__PurchaseO__Purch__245D67DE");
        });

        modelBuilder.Entity<PurchaseReturn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Purchase__3214EC0727267B34");

            entity.ToTable("PurchaseReturn");

            entity.HasIndex(e => e.OrderDate, "IX_PurchaseReturn_OrderDate").IsDescending();

            entity.HasIndex(e => e.Status, "IX_PurchaseReturn_Status");

            entity.HasIndex(e => e.InvoiceCode, "UQ__Purchase__0D9D7FF3B75C0F39").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.InvoiceCode).HasMaxLength(100);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Chờ Xử Lý");
            entity.Property(e => e.SubTotal).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.PurchaseReturnEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_PurchaseReturn_Employee");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseReturnSuppliers)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK_PurchaseReturn_Supplier");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.PurchaseReturns)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK_PurchaseReturn_Warehouse");
        });

        modelBuilder.Entity<PurchaseReturnDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Purchase__3214EC0724E73066");

            entity.ToTable("PurchaseReturnDetail");

            entity.HasIndex(e => e.PurchaseReturnId, "IX_PurchaseReturnDetail_OrderId");

            entity.HasIndex(e => e.ProductId, "IX_PurchaseReturnDetail_ProductId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ReturnPrice).HasColumnType("money");
            entity.Property(e => e.UnitPrice).HasColumnType("money");

            entity.HasOne(d => d.Product).WithMany(p => p.PurchaseReturnDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__PurchaseR__Produ__498EEC8D");

            entity.HasOne(d => d.PurchaseReturn).WithMany(p => p.PurchaseReturnDetails)
                .HasForeignKey(d => d.PurchaseReturnId)
                .HasConstraintName("FK__PurchaseR__Purch__489AC854");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Role__3214EC0722D423F7");

            entity.ToTable("Role");

            entity.HasIndex(e => e.Name, "IX_Role_Name");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<SalaryConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalaryCo__3214EC07C108B787");

            entity.ToTable("SalaryConfig");

            entity.HasIndex(e => new { e.EmployeeId, e.EffectiveDate }, "IX_SalaryConfig_EmployeeId").IsDescending(false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.BaseSalary).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.HolidayCoefficient)
                .HasDefaultValue(2.0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.HourlyRate).HasColumnType("money");
            entity.Property(e => e.HousingAllowance).HasColumnType("money");
            entity.Property(e => e.MealAllowance).HasColumnType("money");
            entity.Property(e => e.OvertimeCoefficient)
                .HasDefaultValue(1.5m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PhoneAllowance).HasColumnType("money");
            entity.Property(e => e.PositionAllowance).HasColumnType("money");
            entity.Property(e => e.SalaryType)
                .HasMaxLength(50)
                .HasDefaultValue("Theo Tháng");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Đang Áp Dụng");
            entity.Property(e => e.TransportAllowance).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.WeekendCoefficient)
                .HasDefaultValue(1.3m)
                .HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Branch).WithMany(p => p.SalaryConfigs)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalaryCon__Branc__2EA5EC27");

            entity.HasOne(d => d.Employee).WithMany(p => p.SalaryConfigs)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalaryCon__Emplo__2DB1C7EE");
        });

        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalesOrd__3214EC071BC4A9FD");

            entity.ToTable("SalesOrder");

            entity.HasIndex(e => new { e.BranchId, e.OrderDate }, "IX_SalesOrder_BranchId").IsDescending(false, true);

            entity.HasIndex(e => new { e.CustomerId, e.OrderDate }, "IX_SalesOrder_CustomerId").IsDescending(false, true);

            entity.HasIndex(e => e.PaymentStatus, "IX_SalesOrder_PaymentStatus");

            entity.HasIndex(e => new { e.Status, e.OrderDate }, "IX_SalesOrder_Status").IsDescending(false, true);

            entity.HasIndex(e => e.OrderCode, "UQ__SalesOrd__999B52292809F092").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ActualDeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerEmail).HasMaxLength(100);
            entity.Property(e => e.CustomerName).HasMaxLength(250);
            entity.Property(e => e.CustomerPhone).HasMaxLength(15);
            entity.Property(e => e.DeliveryMethod)
                .HasMaxLength(50)
                .HasDefaultValue("Tại Cửa Hàng");
            entity.Property(e => e.DiscountAmount).HasColumnType("money");
            entity.Property(e => e.ExpectedDeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.FinalAmount)
                .HasComputedColumnSql("((([SubTotal]-[DiscountAmount])-[VoucherAmount])+[ShippingFee])", true)
                .HasColumnType("money");
            entity.Property(e => e.OrderCode).HasMaxLength(100);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaidAmount).HasColumnType("money");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasDefaultValue("Tiền Mặt");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Chờ Thanh Toán");
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);
            entity.Property(e => e.ShippingFee).HasColumnType("money");
            entity.Property(e => e.ShippingPhone).HasMaxLength(15);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Đặt Trước");
            entity.Property(e => e.SubTotal).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.VoucherAmount).HasColumnType("money");

            entity.HasOne(d => d.Branch).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Branc__756D6ECB");

            entity.HasOne(d => d.Customer).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Custo__74794A92");

            entity.HasOne(d => d.Employee).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Emplo__7755B73D");

            entity.HasOne(d => d.Voucher).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("FK__SalesOrde__Vouch__7D0E9093");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__SalesOrde__Wareh__76619304");
        });

        modelBuilder.Entity<SalesOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalesOrd__3214EC07F716DDD0");

            entity.ToTable("SalesOrderDetail");

            entity.HasIndex(e => e.SalesOrderId, "IX_SalesOrderDetail_OrderId");

            entity.HasIndex(e => e.ProductId, "IX_SalesOrderDetail_ProductId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DiscountAmount).HasColumnType("money");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ItemName).HasMaxLength(250);
            entity.Property(e => e.ProductSku)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ProductSKU");
            entity.Property(e => e.TotalPrice)
                .HasComputedColumnSql("(([Quantity]*[UnitPrice]-[DiscountAmount])+([Quantity]*[UnitPrice])/(100))", true)
                .HasColumnType("money");
            entity.Property(e => e.UnitPrice).HasColumnType("money");

            entity.HasOne(d => d.ProductDetail).WithMany(p => p.SalesOrderDetails)
                .HasForeignKey(d => d.ProductDetailId)
                .HasConstraintName("FK__SalesOrde__Produ__0880433F");

            entity.HasOne(d => d.Product).WithMany(p => p.SalesOrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Produ__078C1F06");

            entity.HasOne(d => d.SalesOrder).WithMany(p => p.SalesOrderDetails)
                .HasForeignKey(d => d.SalesOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Sales__0697FACD");
        });

        modelBuilder.Entity<StockAdjustment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StockAdj__3214EC073D605202");

            entity.ToTable("StockAdjustment");

            entity.HasIndex(e => new { e.BranchId, e.AdjustmentDate }, "IX_StockAdjustment_BranchId").IsDescending(false, true);

            entity.HasIndex(e => new { e.AdjustmentType, e.AdjustmentDate }, "IX_StockAdjustment_Type_Date").IsDescending(false, true);

            entity.HasIndex(e => e.AdjustmentCode, "UQ__StockAdj__292CC6CF40D01F3F").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AdjustmentCode).HasMaxLength(100);
            entity.Property(e => e.AdjustmentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.AdjustmentType).HasMaxLength(50);
            entity.Property(e => e.ApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Lưu tạm");
            entity.Property(e => e.TotalValueChange).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.StockAdjustmentApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK__StockAdju__Appro__55F4C372");

            entity.HasOne(d => d.Branch).WithMany(p => p.StockAdjustments)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StockAdju__Branc__4F47C5E3");

            entity.HasOne(d => d.Employee).WithMany(p => p.StockAdjustmentEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StockAdju__Emplo__51300E55");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.StockAdjustments)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__StockAdju__Wareh__503BEA1C");
        });

        modelBuilder.Entity<StockAdjustmentDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StockAdj__3214EC079D2FACA6");

            entity.ToTable("StockAdjustmentDetail");

            entity.HasIndex(e => e.StockAdjustmentId, "IX_StockAdjustmentDetail_AdjustmentId");

            entity.HasIndex(e => e.ProductId, "IX_StockAdjustmentDetail_ProductId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DifferenceQty).HasComputedColumnSql("([ActualQty]-[SystemQty])", true);
            entity.Property(e => e.TotalValueChange)
                .HasComputedColumnSql("(([ActualQty]-[SystemQty])*[UnitCost])", true)
                .HasColumnType("money");
            entity.Property(e => e.UnitCost).HasColumnType("money");

            entity.HasOne(d => d.Product).WithMany(p => p.StockAdjustmentDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__StockAdju__Produ__5AB9788F");

            entity.HasOne(d => d.StockAdjustment).WithMany(p => p.StockAdjustmentDetails)
                .HasForeignKey(d => d.StockAdjustmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StockAdju__Stock__59C55456");
        });

        modelBuilder.Entity<SupplierDebt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Supplier__3214EC07CCCDF0AD");

            entity.ToTable("SupplierDebt");

            entity.HasIndex(e => e.SupplierId, "UQ_SupplierDebt_Supplier").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.LastTransactionDate).HasColumnType("datetime");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaidAmount).HasColumnType("money");
            entity.Property(e => e.RemainingDebt)
                .HasComputedColumnSql("([TotalDebt]-[PaidAmount])", true)
                .HasColumnType("money");
            entity.Property(e => e.TotalDebt).HasColumnType("money");

            entity.HasOne(d => d.Supplier).WithOne(p => p.SupplierDebt)
                .HasForeignKey<SupplierDebt>(d => d.SupplierId)
                .HasConstraintName("FK__SupplierD__Suppl__0F2D40CE");
        });

        modelBuilder.Entity<SupplierDebtHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Supplier__3214EC076874ECB2");

            entity.ToTable("SupplierDebtHistory");

            entity.HasIndex(e => new { e.SupplierDebtId, e.TransactionDate }, "IX_SupplierDebtHistory_Supplier").IsDescending(false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.BalanceAfter).HasColumnType("money");
            entity.Property(e => e.BalanceBefore).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TransactionType).HasMaxLength(50);

            entity.HasOne(d => d.SupplierDebt).WithMany(p => p.SupplierDebtHistories)
                .HasForeignKey(d => d.SupplierDebtId)
                .HasConstraintName("FK__SupplierD__Suppl__15DA3E5D");
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SystemLo__3214EC07FD4DAEFA");

            entity.ToTable("SystemLog");

            entity.HasIndex(e => new { e.AccountId, e.CreatedAt }, "IX_SystemLog_AccountId").IsDescending(false, true);

            entity.HasIndex(e => e.CreatedAt, "IX_SystemLog_CreatedAt").IsDescending();

            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.CreatedAt }, "IX_SystemLog_EntityType").IsDescending(false, false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AccountName).HasMaxLength(250);
            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.LogLevel)
                .HasMaxLength(20)
                .HasDefaultValue("INFO");
            entity.Property(e => e.Module).HasMaxLength(100);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne(d => d.Account).WithMany(p => p.SystemLogs)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK__SystemLog__Accou__6ABAD62E");
        });

        modelBuilder.Entity<TransferOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transfer__3214EC0790A4D613");

            entity.ToTable("TransferOrder");

            entity.HasIndex(e => e.OrderDate, "IX_TransferOrder_OrderDate").IsDescending();

            entity.HasIndex(e => e.Status, "IX_TransferOrder_Status");

            entity.HasIndex(e => e.InvoiceCode, "UQ__Transfer__0D9D7FF3F23FCEAA").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount).HasColumnType("money");
            entity.Property(e => e.InvoiceCode).HasMaxLength(100);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Paid).HasColumnType("money");
            entity.Property(e => e.PayMethod)
                .HasMaxLength(40)
                .HasDefaultValue("Tiền Mặt");
            entity.Property(e => e.ReceivedDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Ch? X? Lý");
            entity.Property(e => e.SubTotal).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.TransferOrders)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__TransferO__Emplo__31B762FC");

            entity.HasOne(d => d.FromWarehouse).WithMany(p => p.TransferOrderFromWarehouses)
                .HasForeignKey(d => d.FromWarehouseId)
                .HasConstraintName("FK__TransferO__FromW__32AB8735");

            entity.HasOne(d => d.ToWarehouse).WithMany(p => p.TransferOrderToWarehouses)
                .HasForeignKey(d => d.ToWarehouseId)
                .HasConstraintName("FK__TransferO__ToWar__339FAB6E");
        });

        modelBuilder.Entity<TransferOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transfer__3214EC07D995BC97");

            entity.ToTable("TransferOrderDetail");

            entity.HasIndex(e => e.TransferOrderId, "IX_TransferOrderDetail_OrderId");

            entity.HasIndex(e => e.ProductId, "IX_TransferOrderDetail_ProductId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CommissionFee).HasColumnType("money");
            entity.Property(e => e.Discount).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ExtraFee).HasColumnType("money");
            entity.Property(e => e.UnitPrice).HasColumnType("money");

            entity.HasOne(d => d.Product).WithMany(p => p.TransferOrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__TransferO__Produ__395884C4");

            entity.HasOne(d => d.TransferOrder).WithMany(p => p.TransferOrderDetails)
                .HasForeignKey(d => d.TransferOrderId)
                .HasConstraintName("FK__TransferO__Trans__3864608B");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Voucher__3214EC07F8F02969");

            entity.ToTable("Voucher");

            entity.HasIndex(e => e.Code, "UQ__Voucher__A25C5AA7A5AAE4FF").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AvailableDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount)
                .HasDefaultValue(0m)
                .HasColumnType("money");
            entity.Property(e => e.DiscountPercent)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ExpiredDate).HasColumnType("datetime");
            entity.Property(e => e.MaxDiscountAmount).HasColumnType("money");
            entity.Property(e => e.MinimumTransaction).HasColumnType("money");
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Khả Dụng");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UsagePerCustomer).HasDefaultValue(1);
            entity.Property(e => e.VoucherType)
                .HasMaxLength(50)
                .HasDefaultValue("Giảm Giá");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Warehous__3214EC0794CB11D3");

            entity.ToTable("Warehouse");

            entity.HasIndex(e => e.BranchId, "IX_Warehouse_BranchId");

            entity.HasIndex(e => e.Code, "UQ__Warehous__A25C5AA7D72462C6").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Hoạt Động");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Branch).WithMany(p => p.Warehouses)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Warehouse__Branc__45F365D3");
        });

        modelBuilder.Entity<WorkShift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WorkShif__3214EC07BCA5B984");

            entity.ToTable("WorkShift");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ShiftCoefficient)
                .HasDefaultValue(1.0m)
                .HasColumnType("decimal(5, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
