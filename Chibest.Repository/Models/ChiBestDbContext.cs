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

    public virtual DbSet<Color> Colors { get; set; }

    public virtual DbSet<Commission> Commissions { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerVoucher> CustomerVouchers { get; set; }

    public virtual DbSet<Deduction> Deductions { get; set; }

    public virtual DbSet<Payroll> Payrolls { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductDetail> ProductDetails { get; set; }

    public virtual DbSet<ProductPlan> ProductPlans { get; set; }

    public virtual DbSet<ProductPriceHistory> ProductPriceHistories { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }

    public virtual DbSet<PurchaseReturn> PurchaseReturns { get; set; }

    public virtual DbSet<PurchaseReturnDetail> PurchaseReturnDetails { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SalaryConfig> SalaryConfigs { get; set; }

    public virtual DbSet<SalesOrder> SalesOrders { get; set; }

    public virtual DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public virtual DbSet<StockAdjustment> StockAdjustments { get; set; }

    public virtual DbSet<StockAdjustmentDetail> StockAdjustmentDetails { get; set; }

    public virtual DbSet<SupplierDebt> SupplierDebts { get; set; }

    public virtual DbSet<SupplierDebtHistory> SupplierDebtHistories { get; set; }

    public virtual DbSet<TransferOrder> TransferOrders { get; set; }

    public virtual DbSet<TransferOrderDetail> TransferOrderDetails { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<WorkShift> WorkShifts { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Account_pkey");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Code, "Account_Code_key").IsUnique();

            entity.HasIndex(e => e.Email, "Account_Email_key").IsUnique();

            entity.HasIndex(e => e.Email, "ix_account_email");

            entity.HasIndex(e => e.PhoneNumber, "ix_account_phonenumber");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AvatarUrl).HasColumnName("AvatarURL");
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FcmToken).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.RefreshTokenExpiryTime).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'Active'::character varying");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<AccountRole>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.RoleId }).HasName("pk_accountrole");

            entity.ToTable("AccountRole");

            entity.HasIndex(e => e.AccountId, "ix_accountrole_accountid");

            entity.HasIndex(e => new { e.BranchId, e.RoleId }, "ix_accountrole_branchid_roleid");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("fk_accountrole_account");

            entity.HasOne(d => d.Branch).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_accountrole_branch");

            entity.HasOne(d => d.Role).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("fk_accountrole_role");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Attendance_pkey");

            entity.ToTable("Attendance");

            entity.HasIndex(e => new { e.BranchId, e.WorkDate }, "ix_attendance_branchid").IsDescending(false, true);

            entity.HasIndex(e => new { e.EmployeeId, e.WorkDate }, "ix_attendance_employeeid").IsDescending(false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AttendanceStatus)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Có Mặt'::character varying");
            entity.Property(e => e.CheckInTime).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CheckOutTime).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DayType)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Ngày Thường'::character varying");
            entity.Property(e => e.OvertimeHours).HasPrecision(5, 2);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.WorkHours).HasPrecision(5, 2);

            entity.HasOne(d => d.Branch).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Attendance_BranchId_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Attendance_EmployeeId_fkey");

            entity.HasOne(d => d.WorkShift).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.WorkShiftId)
                .HasConstraintName("Attendance_WorkShiftId_fkey");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Branch_pkey");

            entity.ToTable("Branch");

            entity.HasIndex(e => e.Code, "Branch_Code_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.IsFranchise).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'Active'::character varying");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<BranchDebt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BranchDebt_pkey");

            entity.ToTable("BranchDebt");

            entity.HasIndex(e => e.BranchId, "uq_branchdebt_branch").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.LastTransactionDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.PaidAmount).HasColumnType("money");
            entity.Property(e => e.RemainingDebt)
                .HasComputedColumnSql("((\"TotalDebt\" - \"PaidAmount\") - \"ReturnAmount\")", true)
                .HasColumnType("money");
            entity.Property(e => e.ReturnAmount).HasColumnType("money");
            entity.Property(e => e.TotalDebt).HasColumnType("money");

            entity.HasOne(d => d.Branch).WithOne(p => p.BranchDebt)
                .HasForeignKey<BranchDebt>(d => d.BranchId)
                .HasConstraintName("BranchDebt_BranchId_fkey");
        });

        modelBuilder.Entity<BranchDebtHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BranchDebtHistory_pkey");

            entity.ToTable("BranchDebtHistory");

            entity.HasIndex(e => new { e.BranchDebtId, e.TransactionDate }, "IX_BranchDebtHistory_Branch").IsDescending(false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.TransactionType).HasMaxLength(50);

            entity.HasOne(d => d.BranchDebt).WithMany(p => p.BranchDebtHistories)
                .HasForeignKey(d => d.BranchDebtId)
                .HasConstraintName("BranchDebtHistory_BranchDebtId_fkey");
        });

        modelBuilder.Entity<BranchStock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BranchStock_pkey");

            entity.ToTable("BranchStock");

            entity.HasIndex(e => new { e.BranchId, e.AvailableQty }, "ix_branchstock_branchid");

            entity.HasIndex(e => e.ProductId, "ix_branchstock_productid");

            entity.HasIndex(e => new { e.ProductId, e.BranchId }, "uq_branchstock_product_branch").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AvailableQty).HasDefaultValue(0);
            entity.Property(e => e.MaximumStock).HasDefaultValue(0);
            entity.Property(e => e.MinimumStock).HasDefaultValue(0);

            entity.HasOne(d => d.Branch).WithMany(p => p.BranchStocks)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("BranchStock_BranchId_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.BranchStocks)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("BranchStock_ProductId_fkey");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Category_pkey");

            entity.ToTable("Category");

            entity.HasIndex(e => e.Name, "ix_category_type_name");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Color_pkey");

            entity.ToTable("Color");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Code).HasMaxLength(10);
        });

        modelBuilder.Entity<Commission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Commission_pkey");

            entity.ToTable("Commission");

            entity.HasIndex(e => new { e.EmployeeId, e.PeriodYear, e.PeriodMonth }, "ix_commission_employeeid").IsDescending(false, true, true);

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.CalculationBase).HasColumnType("money");
            entity.Property(e => e.CommissionRate).HasPrecision(5, 2);
            entity.Property(e => e.CommissionType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ReferenceType).HasMaxLength(50);

            entity.HasOne(d => d.Employee).WithMany(p => p.Commissions)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Commission_EmployeeId_fkey");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Customer_pkey");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.Code, "Customer_Code_key").IsUnique();

            entity.HasIndex(e => e.Email, "ix_customer_email");

            entity.HasIndex(e => e.GroupId, "ix_customer_groupid");

            entity.HasIndex(e => e.PhoneNumber, "ix_customer_phonenumber");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AvatarUrl).HasColumnName("AvatarURL");
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DateOfBirth).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.LastActive)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Working'::character varying");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");

            entity.HasOne(d => d.Group).WithMany(p => p.InverseGroup)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("Customer_GroupId_fkey");
        });

        modelBuilder.Entity<CustomerVoucher>(entity =>
        {
            entity.HasKey(e => new { e.VoucherId, e.CustomerId }).HasName("CustomerVoucher_pkey");

            entity.ToTable("CustomerVoucher");

            entity.Property(e => e.CollectedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'Đã Nhận'::character varying");
            entity.Property(e => e.UsedDate).HasColumnType("timestamp(3) without time zone");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CustomerVoucher_CustomerId_fkey");

            entity.HasOne(d => d.Voucher).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CustomerVoucher_VoucherId_fkey");
        });

        modelBuilder.Entity<Deduction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Deduction_pkey");

            entity.ToTable("Deduction");

            entity.HasIndex(e => new { e.EmployeeId, e.PeriodYear, e.PeriodMonth }, "ix_deduction_employeeid").IsDescending(false, true, true);

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DeductionType).HasMaxLength(50);

            entity.HasOne(d => d.Employee).WithMany(p => p.Deductions)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Deduction_EmployeeId_fkey");
        });

        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Payroll_pkey");

            entity.ToTable("Payroll");

            entity.HasIndex(e => new { e.EmployeeId, e.PeriodYear, e.PeriodMonth }, "ix_payroll_employeeid").IsDescending(false, true, true);

            entity.HasIndex(e => new { e.PaymentStatus, e.PeriodYear, e.PeriodMonth }, "ix_payroll_status").IsDescending(false, true, true);

            entity.HasIndex(e => new { e.EmployeeId, e.PeriodYear, e.PeriodMonth }, "uq_payroll_employee_period").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ActualBaseSalary).HasColumnType("money");
            entity.Property(e => e.BaseSalary).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.GrossSalary)
                .HasComputedColumnSql("((((\"ActualBaseSalary\" + \"TotalAllowance\") + \"OvertimeSalary\") + \"TotalCommission\") + \"TotalBonus\")", true)
                .HasColumnType("money");
            entity.Property(e => e.HealthInsurance).HasColumnType("money");
            entity.Property(e => e.NetSalary)
                .HasComputedColumnSql("((((((((\"ActualBaseSalary\" + \"TotalAllowance\") + \"OvertimeSalary\") + \"TotalCommission\") + \"TotalBonus\") - \"TotalDeduction\") - \"SocialInsurance\") - \"HealthInsurance\") - \"UnemploymentInsurance\")", true)
                .HasColumnType("money");
            entity.Property(e => e.OvertimeSalary).HasColumnType("money");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Chuyển Khoản'::character varying");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Chờ Thanh Toán'::character varying");
            entity.Property(e => e.SocialInsurance).HasColumnType("money");
            entity.Property(e => e.StandardWorkDays).HasDefaultValue(26);
            entity.Property(e => e.TotalAllowance).HasColumnType("money");
            entity.Property(e => e.TotalBonus).HasColumnType("money");
            entity.Property(e => e.TotalCommission).HasColumnType("money");
            entity.Property(e => e.TotalDeduction).HasColumnType("money");
            entity.Property(e => e.TotalOvertimeHours).HasPrecision(10, 2);
            entity.Property(e => e.TotalWorkDays).HasDefaultValue(0);
            entity.Property(e => e.TotalWorkHours).HasPrecision(10, 2);
            entity.Property(e => e.UnemploymentInsurance).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");

            entity.HasOne(d => d.Branch).WithMany(p => p.Payrolls)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Payroll_BranchId_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.Payrolls)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Payroll_EmployeeId_fkey");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Permission_pkey");

            entity.ToTable("Permission");

            entity.HasIndex(e => e.Code, "Permission_Code_key").IsUnique();

            entity.HasIndex(e => e.Code, "ix_permission_code");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Code).HasMaxLength(100);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Product_pkey");

            entity.ToTable("Product");

            entity.HasIndex(e => e.BarCode, "Product_BarCode_key").IsUnique();

            entity.HasIndex(e => e.Sku, "Product_SKU_key").IsUnique();

            entity.HasIndex(e => e.CategoryId, "ix_product_categoryid");

            entity.HasIndex(e => e.ColorId, "ix_product_colorid");

            entity.HasIndex(e => e.Name, "ix_product_name");

            entity.HasIndex(e => e.ParentSku, "ix_product_parentsku");

            entity.HasIndex(e => e.SizeId, "ix_product_sizeid");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AvatarUrl).HasColumnName("AvatarURL");
            entity.Property(e => e.BarCode).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.IsMaster).HasDefaultValue(true);
            entity.Property(e => e.Material).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.Note).HasMaxLength(200);
            entity.Property(e => e.ParentSku)
                .HasMaxLength(50)
                .HasColumnName("ParentSKU");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'UnAvailable'::character varying");
            entity.Property(e => e.Style).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.VideoUrl).HasColumnName("VideoURL");
            entity.Property(e => e.Weight).HasDefaultValue(0);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("Product_CategoryId_fkey");

            entity.HasOne(d => d.Color).WithMany(p => p.Products)
                .HasForeignKey(d => d.ColorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Product_ColorId_fkey");

            entity.HasOne(d => d.ParentSkuNavigation).WithMany(p => p.InverseParentSkuNavigation)
                .HasPrincipalKey(p => p.Sku)
                .HasForeignKey(d => d.ParentSku)
                .HasConstraintName("Product_ParentSKU_fkey");

            entity.HasOne(d => d.Size).WithMany(p => p.Products)
                .HasForeignKey(d => d.SizeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Product_SizeId_fkey");
        });

        modelBuilder.Entity<ProductDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ProductDetail_pkey");

            entity.ToTable("ProductDetail");

            entity.HasIndex(e => e.ChipCode, "ProductDetail_ChipCode_key").IsUnique();

            entity.HasIndex(e => e.TagId, "ProductDetail_TagId_key").IsUnique();

            entity.HasIndex(e => e.ChipCode, "ix_productdetail_chipcode");

            entity.HasIndex(e => e.ProductId, "ix_productdetail_productid");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ChipCode).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.TagId).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("ProductDetail_ProductId_fkey");
        });

        modelBuilder.Entity<ProductPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ProductPlan_pkey");

            entity.ToTable("ProductPlan");

            entity.HasIndex(e => new { e.SupplierId, e.ProductId, e.SendDate }, "ix_productplan_product_supplier").IsDescending(false, false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DetailAmount).HasMaxLength(100);
            entity.Property(e => e.Note).HasMaxLength(100);
            entity.Property(e => e.SendDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'Queue'::character varying");
            entity.Property(e => e.Type).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductPlans)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("ProductPlan_ProductId_fkey");

            entity.HasOne(d => d.Supplier).WithMany(p => p.ProductPlans)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("ProductPlan_SupplierId_fkey");
        });

        modelBuilder.Entity<ProductPriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ProductPriceHistory_pkey");

            entity.ToTable("ProductPriceHistory");

            entity.HasIndex(e => new { e.ProductId, e.BranchId, e.EffectiveDate }, "ix_productpricehistory_product_branch").IsDescending(false, false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CostPrice).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.EffectiveDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ExpiryDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.SellingPrice).HasColumnType("money");

            entity.HasOne(d => d.Branch).WithMany(p => p.ProductPriceHistories)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("ProductPriceHistory_BranchId_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductPriceHistories)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("ProductPriceHistory_ProductId_fkey");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PurchaseOrder_pkey");

            entity.ToTable("PurchaseOrder");

            entity.HasIndex(e => e.InvoiceCode, "PurchaseOrder_InvoiceCode_key").IsUnique();

            entity.HasIndex(e => e.OrderDate, "ix_transactionorder_orderdate").IsDescending();

            entity.HasIndex(e => e.Status, "ix_transactionorder_status");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.InvoiceCode).HasMaxLength(100);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'Draft'::character varying");
            entity.Property(e => e.SubTotal).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");

            entity.HasOne(d => d.Branch).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("PurchaseOrder_BranchId_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.PurchaseOrderEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("PurchaseOrder_EmployeeId_fkey");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseOrderSuppliers)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("PurchaseOrder_SupplierId_fkey");
        });

        modelBuilder.Entity<PurchaseOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PurchaseOrderDetail_pkey");

            entity.ToTable("PurchaseOrderDetail");

            entity.HasIndex(e => e.PurchaseOrderId, "ix_transactionorderdetail_orderid");

            entity.HasIndex(e => e.ProductId, "ix_transactionorderdetail_productid");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ReFee).HasColumnType("money");
            entity.Property(e => e.UnitPrice).HasColumnType("money");

            entity.HasOne(d => d.Product).WithMany(p => p.PurchaseOrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("PurchaseOrderDetail_ProductId_fkey");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.PurchaseOrderDetails)
                .HasForeignKey(d => d.PurchaseOrderId)
                .HasConstraintName("PurchaseOrderDetail_PurchaseOrderId_fkey");
        });

        modelBuilder.Entity<PurchaseReturn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PurchaseReturn_pkey");

            entity.ToTable("PurchaseReturn");

            entity.HasIndex(e => e.InvoiceCode, "PurchaseReturn_InvoiceCode_key").IsUnique();

            entity.HasIndex(e => e.BranchId, "ix_purchasereturn_branch");

            entity.HasIndex(e => e.OrderDate, "ix_purchasereturn_orderdate").IsDescending();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.InvoiceCode).HasMaxLength(100);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'Draft'::character varying");
            entity.Property(e => e.SubTotal).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");

            entity.HasOne(d => d.Branch).WithMany(p => p.PurchaseReturns)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("fk_purchasereturn_branch");

            entity.HasOne(d => d.Employee).WithMany(p => p.PurchaseReturnEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("fk_purchasereturn_employee");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseReturnSuppliers)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("fk_purchasereturn_supplier");
        });

        modelBuilder.Entity<PurchaseReturnDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PurchaseReturnDetail_pkey");

            entity.ToTable("PurchaseReturnDetail");

            entity.HasIndex(e => e.PurchaseReturnId, "ix_purchasereturndetail_orderid");

            entity.HasIndex(e => e.ProductId, "ix_purchasereturndetail_productid");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.UnitPrice).HasColumnType("money");

            entity.HasOne(d => d.Product).WithMany(p => p.PurchaseReturnDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("PurchaseReturnDetail_ProductId_fkey");

            entity.HasOne(d => d.PurchaseReturn).WithMany(p => p.PurchaseReturnDetails)
                .HasForeignKey(d => d.PurchaseReturnId)
                .HasConstraintName("PurchaseReturnDetail_PurchaseReturnId_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Role_pkey");

            entity.ToTable("Role");

            entity.HasIndex(e => e.Name, "ix_role_name");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("fk_rolepermission_permission"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("fk_rolepermission_role"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId").HasName("pk_rolepermission");
                        j.ToTable("RolePermission");
                    });
        });

        modelBuilder.Entity<SalaryConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SalaryConfig_pkey");

            entity.ToTable("SalaryConfig");

            entity.HasIndex(e => new { e.EmployeeId, e.EffectiveDate }, "ix_salaryconfig_employeeid").IsDescending(false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.BaseSalary).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.HolidayCoefficient)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("2.0");
            entity.Property(e => e.HourlyRate).HasColumnType("money");
            entity.Property(e => e.HousingAllowance).HasColumnType("money");
            entity.Property(e => e.MealAllowance).HasColumnType("money");
            entity.Property(e => e.OvertimeCoefficient)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("1.5");
            entity.Property(e => e.PhoneAllowance).HasColumnType("money");
            entity.Property(e => e.PositionAllowance).HasColumnType("money");
            entity.Property(e => e.SalaryType)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Theo Tháng'::character varying");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'Đang Áp Dụng'::character varying");
            entity.Property(e => e.TransportAllowance).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.WeekendCoefficient)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("1.3");

            entity.HasOne(d => d.Branch).WithMany(p => p.SalaryConfigs)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalaryConfig_BranchId_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.SalaryConfigs)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalaryConfig_EmployeeId_fkey");
        });

        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SalesOrder_pkey");

            entity.ToTable("SalesOrder");

            entity.HasIndex(e => e.OrderCode, "SalesOrder_OrderCode_key").IsUnique();

            entity.HasIndex(e => new { e.BranchId, e.OrderDate }, "ix_salesorder_branchid").IsDescending(false, true);

            entity.HasIndex(e => new { e.CustomerId, e.OrderDate }, "ix_salesorder_customerid").IsDescending(false, true);

            entity.HasIndex(e => e.PaymentStatus, "ix_salesorder_paymentstatus");

            entity.HasIndex(e => new { e.Status, e.OrderDate }, "ix_salesorder_status").IsDescending(false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ActualDeliveryDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CustomerEmail).HasMaxLength(100);
            entity.Property(e => e.CustomerName).HasMaxLength(250);
            entity.Property(e => e.CustomerPhone).HasMaxLength(15);
            entity.Property(e => e.DeliveryMethod)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Tại Cửa Hàng'::character varying");
            entity.Property(e => e.DiscountAmount).HasColumnType("money");
            entity.Property(e => e.ExpectedDeliveryDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.FinalAmount)
                .HasComputedColumnSql("(((\"SubTotal\" - \"DiscountAmount\") - \"VoucherAmount\") + \"ShippingFee\")", true)
                .HasColumnType("money");
            entity.Property(e => e.OrderCode).HasMaxLength(100);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.PaidAmount).HasColumnType("money");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Tiền Mặt'::character varying");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Chờ Thanh Toán'::character varying");
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);
            entity.Property(e => e.ShippingFee).HasColumnType("money");
            entity.Property(e => e.ShippingPhone).HasMaxLength(15);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Đặt Trước'::character varying");
            entity.Property(e => e.SubTotal).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.VoucherAmount).HasColumnType("money");

            entity.HasOne(d => d.Branch).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalesOrder_BranchId_fkey");

            entity.HasOne(d => d.Customer).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalesOrder_CustomerId_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalesOrder_EmployeeId_fkey");

            entity.HasOne(d => d.Voucher).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("SalesOrder_VoucherId_fkey");
        });

        modelBuilder.Entity<SalesOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SalesOrderDetail_pkey");

            entity.ToTable("SalesOrderDetail");

            entity.HasIndex(e => e.SalesOrderId, "ix_salesorderdetail_orderid");

            entity.HasIndex(e => e.ProductId, "ix_salesorderdetail_productid");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DiscountAmount).HasColumnType("money");
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.ItemName).HasMaxLength(250);
            entity.Property(e => e.ProductSku)
                .HasMaxLength(50)
                .HasColumnName("ProductSKU");
            entity.Property(e => e.TotalPrice)
                .HasComputedColumnSql("(((\"Quantity\" * \"UnitPrice\") - \"DiscountAmount\") + ((\"Quantity\" * \"UnitPrice\") / 100))", true)
                .HasColumnType("money");
            entity.Property(e => e.UnitPrice).HasColumnType("money");

            entity.HasOne(d => d.ProductDetail).WithMany(p => p.SalesOrderDetails)
                .HasForeignKey(d => d.ProductDetailId)
                .HasConstraintName("SalesOrderDetail_ProductDetailId_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.SalesOrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalesOrderDetail_ProductId_fkey");

            entity.HasOne(d => d.SalesOrder).WithMany(p => p.SalesOrderDetails)
                .HasForeignKey(d => d.SalesOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SalesOrderDetail_SalesOrderId_fkey");
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Size_pkey");

            entity.ToTable("Size");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Code).HasMaxLength(20);
        });

        modelBuilder.Entity<StockAdjustment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("StockAdjustment_pkey");

            entity.ToTable("StockAdjustment");

            entity.HasIndex(e => e.AdjustmentCode, "StockAdjustment_AdjustmentCode_key").IsUnique();

            entity.HasIndex(e => new { e.BranchId, e.AdjustmentDate }, "ix_stockadjustment_branchid").IsDescending(false, true);

            entity.HasIndex(e => new { e.AdjustmentType, e.AdjustmentDate }, "ix_stockadjustment_type_date").IsDescending(false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AdjustmentCode).HasMaxLength(100);
            entity.Property(e => e.AdjustmentDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.AdjustmentType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'Draft'::character varying");
            entity.Property(e => e.TotalValueChange).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.StockAdjustmentApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("StockAdjustment_ApprovedBy_fkey");

            entity.HasOne(d => d.Branch).WithMany(p => p.StockAdjustments)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("StockAdjustment_BranchId_fkey");

            entity.HasOne(d => d.Employee).WithMany(p => p.StockAdjustmentEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("StockAdjustment_EmployeeId_fkey");
        });

        modelBuilder.Entity<StockAdjustmentDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("StockAdjustmentDetail_pkey");

            entity.ToTable("StockAdjustmentDetail");

            entity.HasIndex(e => e.StockAdjustmentId, "ix_stockadjustmentdetail_adjustmentid");

            entity.HasIndex(e => e.ProductId, "ix_stockadjustmentdetail_productid");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DifferenceQty).HasComputedColumnSql("(\"ActualQty\" - \"SystemQty\")", true);
            entity.Property(e => e.TotalValueChange)
                .HasComputedColumnSql("((\"ActualQty\" - \"SystemQty\") * \"UnitCost\")", true)
                .HasColumnType("money");
            entity.Property(e => e.UnitCost).HasColumnType("money");

            entity.HasOne(d => d.Product).WithMany(p => p.StockAdjustmentDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("StockAdjustmentDetail_ProductId_fkey");

            entity.HasOne(d => d.StockAdjustment).WithMany(p => p.StockAdjustmentDetails)
                .HasForeignKey(d => d.StockAdjustmentId)
                .HasConstraintName("StockAdjustmentDetail_StockAdjustmentId_fkey");
        });

        modelBuilder.Entity<SupplierDebt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SupplierDebt_pkey");

            entity.ToTable("SupplierDebt");

            entity.HasIndex(e => e.SupplierId, "uq_supplierdebt_supplier").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.PaidAmount).HasColumnType("money");
            entity.Property(e => e.RemainingDebt)
                .HasComputedColumnSql("((\"TotalDebt\" - \"PaidAmount\") - \"ReturnAmount\")", true)
                .HasColumnType("money");
            entity.Property(e => e.ReturnAmount).HasColumnType("money");
            entity.Property(e => e.TotalDebt).HasColumnType("money");

            entity.HasOne(d => d.Supplier).WithOne(p => p.SupplierDebt)
                .HasForeignKey<SupplierDebt>(d => d.SupplierId)
                .HasConstraintName("SupplierDebt_SupplierId_fkey");
        });

        modelBuilder.Entity<SupplierDebtHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SupplierDebtHistory_pkey");

            entity.ToTable("SupplierDebtHistory");

            entity.HasIndex(e => new { e.SupplierDebtId, e.TransactionDate }, "ix_supplierdebthistory_supplier").IsDescending(false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.TransactionType).HasMaxLength(50);

            entity.HasOne(d => d.SupplierDebt).WithMany(p => p.SupplierDebtHistories)
                .HasForeignKey(d => d.SupplierDebtId)
                .HasConstraintName("SupplierDebtHistory_SupplierDebtId_fkey");
        });

        modelBuilder.Entity<TransferOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TransferOrder_pkey");

            entity.ToTable("TransferOrder");

            entity.HasIndex(e => e.InvoiceCode, "TransferOrder_InvoiceCode_key").IsUnique();

            entity.HasIndex(e => new { e.FromBranch, e.ToBranch, e.OrderDate }, "ix_transferorder_branch").IsDescending(false, false, true);

            entity.HasIndex(e => e.OrderDate, "ix_transferorder_orderdate").IsDescending();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.InvoiceCode).HasMaxLength(100);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'Draft'::character varying");
            entity.Property(e => e.SubTotal).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");

            entity.HasOne(d => d.Employee).WithMany(p => p.TransferOrders)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("TransferOrder_EmployeeId_fkey");

            entity.HasOne(d => d.FromBranchNavigation).WithMany(p => p.TransferOrderFromBranchNavigations)
                .HasForeignKey(d => d.FromBranch)
                .HasConstraintName("TransferOrder_FromBranch_fkey");

            entity.HasOne(d => d.ToBranchNavigation).WithMany(p => p.TransferOrderToBranchNavigations)
                .HasForeignKey(d => d.ToBranch)
                .HasConstraintName("TransferOrder_ToBranch_fkey");
        });

        modelBuilder.Entity<TransferOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TransferOrderDetail_pkey");

            entity.ToTable("TransferOrderDetail");

            entity.HasIndex(e => e.TransferOrderId, "ix_transferorderdetail_orderid");

            entity.HasIndex(e => e.ProductId, "ix_transferorderdetail_productid");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CommissionFee).HasColumnType("money");
            entity.Property(e => e.UnitPrice).HasColumnType("money");

            entity.HasOne(d => d.Product).WithMany(p => p.TransferOrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("TransferOrderDetail_ProductId_fkey");

            entity.HasOne(d => d.TransferOrder).WithMany(p => p.TransferOrderDetails)
                .HasForeignKey(d => d.TransferOrderId)
                .HasConstraintName("TransferOrderDetail_TransferOrderId_fkey");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Voucher_pkey");

            entity.ToTable("Voucher");

            entity.HasIndex(e => e.Code, "Voucher_Code_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AvailableDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DiscountAmount)
                .HasDefaultValueSql("0")
                .HasColumnType("money");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0");
            entity.Property(e => e.ExpiredDate).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.MaxDiscountAmount).HasColumnType("money");
            entity.Property(e => e.MinimumTransaction).HasColumnType("money");
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValueSql("'Available'::character varying");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.UsagePerCustomer).HasDefaultValue(1);
            entity.Property(e => e.UsedCount).HasDefaultValue(0);
            entity.Property(e => e.VoucherType)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Giảm Giá'::character varying");
        });

        modelBuilder.Entity<WorkShift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("WorkShift_pkey");

            entity.ToTable("WorkShift");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.IsOvernight).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ShiftCoefficient)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("1.0");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
