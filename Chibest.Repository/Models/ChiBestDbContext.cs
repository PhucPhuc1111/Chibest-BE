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

    public virtual DbSet<BranchStock> BranchStocks { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Commission> Commissions { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerVoucher> CustomerVouchers { get; set; }

    public virtual DbSet<Deduction> Deductions { get; set; }

    public virtual DbSet<Payroll> Payrolls { get; set; }

    public virtual DbSet<PayrollDetail> PayrollDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductDetail> ProductDetails { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SalaryConfig> SalaryConfigs { get; set; }

    public virtual DbSet<SalesOrder> SalesOrders { get; set; }

    public virtual DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }

    public virtual DbSet<SystemLog> SystemLogs { get; set; }

    public virtual DbSet<TransactionOrder> TransactionOrders { get; set; }

    public virtual DbSet<TransactionOrderDetail> TransactionOrderDetails { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    public virtual DbSet<WorkShift> WorkShifts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC07A7AF9627");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Code, "UQ__Account__A25C5AA7D01D7899").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Account__A9D10534C8348AE5").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AvartarUrl).HasColumnName("AvartarURL");
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
            entity.Property(e => e.TaxCode).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<AccountRole>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.RoleId }).HasName("PK__AccountR__8C320947A1610E13");

            entity.ToTable("AccountRole");

            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountRo__Accou__4AB81AF0");

            entity.HasOne(d => d.Branch).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__AccountRo__Branc__49C3F6B7");

            entity.HasOne(d => d.Role).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountRo__RoleI__4BAC3F29");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC078472ADA1");

            entity.ToTable("Attendance");

            entity.HasIndex(e => e.BranchId, "IX_Attendance_BranchId");

            entity.HasIndex(e => new { e.EmployeeId, e.WorkDate }, "IX_Attendance_EmployeeId_WorkDate").IsDescending(false, true);

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
                .HasConstraintName("FK__Attendanc__Branc__4A8310C6");

            entity.HasOne(d => d.Employee).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Emplo__498EEC8D");

            entity.HasOne(d => d.WorkShift).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.WorkShiftId)
                .HasConstraintName("FK__Attendanc__WorkS__4B7734FF");
        });

        modelBuilder.Entity<BranchStock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BranchSt__3214EC071312C8BB");

            entity.ToTable("BranchStock");

            entity.HasIndex(e => new { e.ProductId, e.BranchId }, "UQ__BranchSt__FE1A443000DED79A").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SellingPrice).HasColumnType("money");

            entity.HasOne(d => d.Branch).WithMany(p => p.BranchStocks)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BranchSto__Branc__778AC167");

            entity.HasOne(d => d.Product).WithMany(p => p.BranchStocks)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BranchSto__Produ__76969D2E");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC07A59C1109");

            entity.ToTable("Category");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<Commission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Commissi__3214EC0710D49F2A");

            entity.ToTable("Commission");

            entity.HasIndex(e => e.EmployeeId, "IX_Commission_EmployeeId");

            entity.HasIndex(e => new { e.PeriodYear, e.PeriodMonth }, "IX_Commission_Period");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.CalculationBase).HasColumnType("money");
            entity.Property(e => e.CommissionRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CommissionType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.Commissions)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Commissio__Emplo__55009F39");

            entity.HasOne(d => d.SalesOrder).WithMany(p => p.Commissions)
                .HasForeignKey(d => d.SalesOrderId)
                .HasConstraintName("FK__Commissio__Sales__55F4C372");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07063204DE");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.Code, "UQ__Customer__A25C5AA7E16EEF2E").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AvartarUrl).HasColumnName("AvartarURL");
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DateOfBirth).HasColumnType("datetime");
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.LastActive)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Đã Tạo");
            entity.Property(e => e.Type)
                .HasMaxLength(40)
                .HasDefaultValue("Cá Nhân");

            entity.HasOne(d => d.Group).WithMany(p => p.InverseGroup)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__Customer__GroupI__5441852A");
        });

        modelBuilder.Entity<CustomerVoucher>(entity =>
        {
            entity.HasKey(e => new { e.VoucherId, e.AccountId }).HasName("PK__Customer__49A7A37BDB7DE69D");

            entity.ToTable("CustomerVoucher");

            entity.Property(e => e.CollectedDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Đã Nhận");
            entity.Property(e => e.UsedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerV__Accou__619B8048");

            entity.HasOne(d => d.Voucher).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerV__Vouch__60A75C0F");
        });

        modelBuilder.Entity<Deduction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Deductio__3214EC0780C6A0C6");

            entity.ToTable("Deduction");

            entity.HasIndex(e => e.EmployeeId, "IX_Deduction_EmployeeId");

            entity.HasIndex(e => new { e.PeriodYear, e.PeriodMonth }, "IX_Deduction_Period");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeductionType).HasMaxLength(50);

            entity.HasOne(d => d.Employee).WithMany(p => p.Deductions)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Deduction__Emplo__5AB9788F");
        });

        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payroll__3214EC0722EED29C");

            entity.ToTable("Payroll");

            entity.HasIndex(e => e.EmployeeId, "IX_Payroll_EmployeeId");

            entity.HasIndex(e => new { e.PeriodYear, e.PeriodMonth }, "IX_Payroll_Period");

            entity.HasIndex(e => e.PaymentStatus, "IX_Payroll_Status");

            entity.HasIndex(e => new { e.EmployeeId, e.PeriodYear, e.PeriodMonth }, "UQ__Payroll__32843B6DB5CB9B28").IsUnique();

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
                .HasComputedColumnSql("((((((((([ActualBaseSalary]+[TotalAllowance])+[OvertimeSalary])+[TotalCommission])+[TotalBonus])-[TotalDeduction])-[SocialInsurance])-[HealthInsurance])-[UnemploymentInsurance])-[PersonalTax])", true)
                .HasColumnType("money");
            entity.Property(e => e.OvertimeSalary).HasColumnType("money");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasDefaultValue("Chuyển Khoản");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Chờ Thanh Toán");
            entity.Property(e => e.PersonalTax).HasColumnType("money");
            entity.Property(e => e.SocialInsurance).HasColumnType("money");
            entity.Property(e => e.StandardWorkDays).HasDefaultValue(26);
            entity.Property(e => e.TaxableIncome).HasColumnType("money");
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
                .HasConstraintName("FK__Payroll__BranchI__6166761E");

            entity.HasOne(d => d.Employee).WithMany(p => p.Payrolls)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payroll__Employe__607251E5");
        });

        modelBuilder.Entity<PayrollDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PayrollD__3214EC070724E55C");

            entity.ToTable("PayrollDetail");

            entity.HasIndex(e => e.PayrollId, "IX_PayrollDetail_PayrollId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.IsAddition).HasDefaultValue(true);
            entity.Property(e => e.ItemAmount).HasColumnType("money");
            entity.Property(e => e.ItemName).HasMaxLength(250);
            entity.Property(e => e.ItemType).HasMaxLength(50);

            entity.HasOne(d => d.Payroll).WithMany(p => p.PayrollDetails)
                .HasForeignKey(d => d.PayrollId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PayrollDe__Payro__7849DB76");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3214EC07865827FF");

            entity.ToTable("Product");

            entity.HasIndex(e => e.Sku, "UQ__Product__CA1ECF0D8AB0ECFD").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AvartarUrl).HasColumnName("AvartarURL");
            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(100);
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

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Product__Categor__6C190EBB");

            entity.HasOne(d => d.ParentSkuNavigation).WithMany(p => p.InverseParentSkuNavigation)
                .HasPrincipalKey(p => p.Sku)
                .HasForeignKey(d => d.ParentSku)
                .HasConstraintName("FK__Product__ParentS__6D0D32F4");
        });

        modelBuilder.Entity<ProductDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductD__3214EC07BAC1094D");

            entity.ToTable("ProductDetail");

            entity.HasIndex(e => e.ChipCode, "IX_ProductDetail_ChipCode");

            entity.HasIndex(e => e.Status, "IX_ProductDetail_Status");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ChipCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ContainerCode).HasMaxLength(100);
            entity.Property(e => e.ImportDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LastTransactionDate).HasColumnType("datetime");
            entity.Property(e => e.PurchasePrice).HasColumnType("money");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Kh? D?ng");

            entity.HasOne(d => d.ContainerCodeNavigation).WithMany(p => p.ProductDetails)
                .HasPrincipalKey(p => p.ContainerCode)
                .HasForeignKey(d => d.ContainerCode)
                .HasConstraintName("FK__ProductDe__Conta__17036CC0");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductDe__Produ__151B244E");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.ProductDetails)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__ProductDe__Wareh__160F4887");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Role__3214EC073B1E2623");

            entity.ToTable("Role");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<SalaryConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalaryCo__3214EC075775DE9A");

            entity.ToTable("SalaryConfig");

            entity.HasIndex(e => e.EmployeeId, "IX_SalaryConfig_EmployeeId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.BaseSalary).HasColumnType("money");
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
            entity.Property(e => e.WeekendCoefficient)
                .HasDefaultValue(1.3m)
                .HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Branch).WithMany(p => p.SalaryConfigs)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalaryCon__Branc__3C34F16F");

            entity.HasOne(d => d.Employee).WithMany(p => p.SalaryConfigs)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalaryCon__Emplo__3B40CD36");
        });

        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalesOrd__3214EC07517FB7D5");

            entity.ToTable("SalesOrder");

            entity.HasIndex(e => e.BranchId, "IX_SalesOrder_BranchId");

            entity.HasIndex(e => e.CustomerId, "IX_SalesOrder_CustomerId");

            entity.HasIndex(e => e.EmployeeId, "IX_SalesOrder_EmployeeId");

            entity.HasIndex(e => e.OrderDate, "IX_SalesOrder_OrderDate").IsDescending();

            entity.HasIndex(e => e.PaymentStatus, "IX_SalesOrder_PaymentStatus");

            entity.HasIndex(e => e.Status, "IX_SalesOrder_Status");

            entity.HasIndex(e => e.InvoiceCode, "UQ__SalesOrd__0D9D7FF303CAB2F8").IsUnique();

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
                .HasComputedColumnSql("(((([SubTotal]-[DiscountAmount])-[VoucherAmount])+[ShippingFee])+[TaxAmount])", true)
                .HasColumnType("money");
            entity.Property(e => e.InvoiceCode).HasMaxLength(100);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
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
            entity.Property(e => e.TaxAmount).HasColumnType("money");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.VoucherAmount).HasColumnType("money");

            entity.HasOne(d => d.Branch).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Branc__2739D489");

            entity.HasOne(d => d.Customer).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Custo__29221CFB");

            entity.HasOne(d => d.Employee).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Emplo__282DF8C2");

            entity.HasOne(d => d.Voucher).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("FK__SalesOrde__Vouch__2A164134");
        });

        modelBuilder.Entity<SalesOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalesOrd__3214EC07535ECC1C");

            entity.ToTable("SalesOrderDetail");

            entity.HasIndex(e => e.ProductId, "IX_SalesOrderDetail_ProductId");

            entity.HasIndex(e => e.SalesOrderId, "IX_SalesOrderDetail_SalesOrderId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DiscountAmount).HasColumnType("money");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ItemName).HasMaxLength(250);
            entity.Property(e => e.ProductSku)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ProductSKU");
            entity.Property(e => e.TotalPrice)
                .HasComputedColumnSql("([Quantity]*[UnitPrice]-[DiscountAmount])", true)
                .HasColumnType("money");
            entity.Property(e => e.UnitPrice).HasColumnType("money");

            entity.HasOne(d => d.ProductDetail).WithMany(p => p.SalesOrderDetails)
                .HasForeignKey(d => d.ProductDetailId)
                .HasConstraintName("FK__SalesOrde__Produ__32AB8735");

            entity.HasOne(d => d.Product).WithMany(p => p.SalesOrderDetailProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Produ__30C33EC3");

            entity.HasOne(d => d.ProductSkuNavigation).WithMany(p => p.SalesOrderDetailProductSkuNavigations)
                .HasPrincipalKey(p => p.Sku)
                .HasForeignKey(d => d.ProductSku)
                .HasConstraintName("FK__SalesOrde__Produ__31B762FC");

            entity.HasOne(d => d.SalesOrder).WithMany(p => p.SalesOrderDetails)
                .HasForeignKey(d => d.SalesOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Sales__2FCF1A8A");
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SystemLo__3214EC07B9ABEE76");

            entity.ToTable("SystemLog");

            entity.HasIndex(e => e.AccountId, "IX_SystemLog_AccountId");

            entity.HasIndex(e => e.Action, "IX_SystemLog_Action");

            entity.HasIndex(e => e.CreatedAt, "IX_SystemLog_CreatedAt").IsDescending();

            entity.HasIndex(e => new { e.EntityType, e.EntityId }, "IX_SystemLog_EntityType_EntityId");

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
                .HasConstraintName("FK__SystemLog__Accou__7D0E9093");
        });

        modelBuilder.Entity<TransactionOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC071004B594");

            entity.ToTable("TransactionOrder");

            entity.HasIndex(e => e.OrderDate, "IX_TransactionOrder_OrderDate").IsDescending();

            entity.HasIndex(e => e.Status, "IX_TransactionOrder_Status");

            entity.HasIndex(e => e.Type, "IX_TransactionOrder_Type");

            entity.HasIndex(e => e.InvoiceCode, "UQ__Transact__0D9D7FF327017CD7").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ActualDeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount).HasColumnType("money");
            entity.Property(e => e.ExpectedDeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.FinalCost)
                .HasComputedColumnSql("((([SubTotal]-[DiscountAmount])+[TaxAmount])+[ShippingFee])", true)
                .HasColumnType("money");
            entity.Property(e => e.InvoiceCode).HasMaxLength(100);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PayMethod)
                .HasMaxLength(40)
                .HasDefaultValue("Tiền Mặt");
            entity.Property(e => e.ReceiverName).HasMaxLength(250);
            entity.Property(e => e.ReceiverPhone).HasMaxLength(15);
            entity.Property(e => e.ShippingFee).HasColumnType("money");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Ch? X? Lý");
            entity.Property(e => e.SubTotal).HasColumnType("money");
            entity.Property(e => e.TaxAmount).HasColumnType("money");
            entity.Property(e => e.Type).HasMaxLength(100);

            entity.HasOne(d => d.Employee).WithMany(p => p.TransactionOrderEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Transacti__Emplo__04E4BC85");

            entity.HasOne(d => d.FromWarehouse).WithMany(p => p.TransactionOrderFromWarehouses)
                .HasForeignKey(d => d.FromWarehouseId)
                .HasConstraintName("FK__Transacti__FromW__02FC7413");

            entity.HasOne(d => d.Supplier).WithMany(p => p.TransactionOrderSuppliers)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__Transacti__Suppl__05D8E0BE");

            entity.HasOne(d => d.ToWarehouse).WithMany(p => p.TransactionOrderToWarehouses)
                .HasForeignKey(d => d.ToWarehouseId)
                .HasConstraintName("FK__Transacti__ToWar__03F0984C");
        });

        modelBuilder.Entity<TransactionOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC077F1F2170");

            entity.ToTable("TransactionOrderDetail");

            entity.HasIndex(e => e.ContainerCode, "IX_TransactionOrderDetail_ContainerCode");

            entity.HasIndex(e => e.TransactionOrderId, "IX_TransactionOrderDetail_OrderId");

            entity.HasIndex(e => e.ProductId, "IX_TransactionOrderDetail_ProductId");

            entity.HasIndex(e => e.ContainerCode, "UQ__Transact__874FE470E4F83D7F").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ContainerCode).HasMaxLength(100);
            entity.Property(e => e.DiscountAmount).HasColumnType("money");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PurchaseDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalPrice)
                .HasComputedColumnSql("([Quantity]*[UnitPrice]-[DiscountAmount])", true)
                .HasColumnType("money");
            entity.Property(e => e.UnitPrice).HasColumnType("money");

            entity.HasOne(d => d.Product).WithMany(p => p.TransactionOrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__Produ__0E6E26BF");

            entity.HasOne(d => d.TransactionOrder).WithMany(p => p.TransactionOrderDetails)
                .HasForeignKey(d => d.TransactionOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__Trans__0D7A0286");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Voucher__3214EC0722CB1D68");

            entity.ToTable("Voucher");

            entity.HasIndex(e => e.Code, "UQ__Voucher__A25C5AA7C387F380").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AvailableDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.ExpiredDate).HasColumnType("datetime");
            entity.Property(e => e.MaxDiscountAmount).HasColumnType("money");
            entity.Property(e => e.MinimumTransaction).HasColumnType("money");
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Khả Dụng");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Warehous__3214EC07FC07D679");

            entity.ToTable("Warehouse");

            entity.HasIndex(e => e.ParentWarehouseId, "IX_Warehouse_ParentWarehouseID");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FaxNumber).HasMaxLength(15);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.ParentWarehouseId).HasColumnName("ParentWarehouseID");
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.ParentWarehouse).WithMany(p => p.InverseParentWarehouse)
                .HasForeignKey(d => d.ParentWarehouseId)
                .HasConstraintName("FK__Warehouse__Paren__3B75D760");
        });

        modelBuilder.Entity<WorkShift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WorkShif__3214EC075C3BFE66");

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
