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
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC07E8E3D859");

            entity.ToTable("Account");

            entity.HasIndex(e => e.PhoneNumber, "IX_Account_PhoneNumber");

            entity.HasIndex(e => e.Code, "UQ__Account__A25C5AA7E6B790AC").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Account__A9D1053475112797").IsUnique();

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
            entity.HasKey(e => new { e.AccountId, e.RoleId }).HasName("PK__AccountR__8C320947054C654C");

            entity.ToTable("AccountRole");

            entity.HasIndex(e => new { e.AccountId, e.StartDate }, "IX_AccountRole_AccountId_StartDate");

            entity.HasIndex(e => new { e.BranchId, e.RoleId }, "IX_AccountRole_BranchId_RoleId");

            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountRo__Accou__4F7CD00D");

            entity.HasOne(d => d.Branch).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__AccountRo__Branc__4E88ABD4");

            entity.HasOne(d => d.Role).WithMany(p => p.AccountRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountRo__RoleI__5070F446");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC070A62F638");

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
                .HasConstraintName("FK__Attendanc__Branc__57DD0BE4");

            entity.HasOne(d => d.Employee).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Emplo__56E8E7AB");

            entity.HasOne(d => d.WorkShift).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.WorkShiftId)
                .HasConstraintName("FK__Attendanc__WorkS__58D1301D");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Branch__3214EC07A7899139");

            entity.ToTable("Branch");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Hoạt Động");
        });

        modelBuilder.Entity<BranchStock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BranchSt__3214EC078E735F6C");

            entity.ToTable("BranchStock");

            entity.HasIndex(e => new { e.BranchId, e.AvailableQty }, "IX_BranchStock_BranchId_AvailableQty");

            entity.HasIndex(e => e.ProductId, "IX_BranchStock_ProductId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SellingPrice).HasColumnType("money");

            entity.HasOne(d => d.Branch).WithMany(p => p.BranchStocks)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BranchSto__Branc__7B5B524B");

            entity.HasOne(d => d.OnlineWarehouse).WithMany(p => p.BranchStockOnlineWarehouses)
                .HasForeignKey(d => d.OnlineWarehouseId)
                .HasConstraintName("FK__BranchSto__Onlin__7D439ABD");

            entity.HasOne(d => d.Product).WithMany(p => p.BranchStocks)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BranchSto__Produ__7A672E12");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.BranchStockWarehouses)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__BranchSto__Wareh__7C4F7684");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC07731F0F30");

            entity.ToTable("Category");

            entity.HasIndex(e => new { e.Type, e.Name }, "IX_Category_Type_Name");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<Commission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Commissi__3214EC0722F954F8");

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
                .HasConstraintName("FK__Commissio__Emplo__5CA1C101");

            entity.HasOne(d => d.SalesOrder).WithMany(p => p.Commissions)
                .HasForeignKey(d => d.SalesOrderId)
                .HasConstraintName("FK__Commissio__Sales__5D95E53A");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07004E4D14");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.GroupId, "IX_Customer_GroupId");

            entity.HasIndex(e => e.PhoneNumber, "IX_Customer_PhoneNumber");

            entity.HasIndex(e => e.Code, "UQ__Customer__A25C5AA7582801EB").IsUnique();

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
                .HasConstraintName("FK__Customer__GroupI__59063A47");
        });

        modelBuilder.Entity<CustomerVoucher>(entity =>
        {
            entity.HasKey(e => new { e.VoucherId, e.CustomerId }).HasName("PK__Customer__A0A49F6CD42575A5");

            entity.ToTable("CustomerVoucher");

            entity.Property(e => e.CollectedDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(40)
                .HasDefaultValue("Đã Nhận");
            entity.Property(e => e.UsedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerV__Custo__66603565");

            entity.HasOne(d => d.Voucher).WithMany(p => p.CustomerVouchers)
                .HasForeignKey(d => d.VoucherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerV__Vouch__656C112C");
        });

        modelBuilder.Entity<Deduction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Deductio__3214EC07843567E7");

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
                .HasConstraintName("FK__Deduction__Emplo__625A9A57");
        });

        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payroll__3214EC070B853D53");

            entity.ToTable("Payroll");

            entity.HasIndex(e => e.EmployeeId, "IX_Payroll_EmployeeId");

            entity.HasIndex(e => new { e.PeriodYear, e.PeriodMonth }, "IX_Payroll_Period");

            entity.HasIndex(e => e.PaymentStatus, "IX_Payroll_Status");

            entity.HasIndex(e => new { e.EmployeeId, e.PeriodYear, e.PeriodMonth }, "UQ__Payroll__32843B6D23B1AC8B").IsUnique();

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
                .HasConstraintName("FK__Payroll__BranchI__690797E6");

            entity.HasOne(d => d.Employee).WithMany(p => p.Payrolls)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payroll__Employe__681373AD");
        });

        modelBuilder.Entity<PayrollDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PayrollD__3214EC0714B77085");

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
                .HasConstraintName("FK__PayrollDe__Payro__7FEAFD3E");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3214EC07DDE8AFA4");

            entity.ToTable("Product");

            entity.HasIndex(e => new { e.CategoryId, e.IsMaster }, "IX_Product_Category_IsMaster");

            entity.HasIndex(e => e.Name, "IX_Product_Name");

            entity.HasIndex(e => e.ParentSku, "IX_Product_ParentSKU");

            entity.HasIndex(e => e.Sku, "UQ__Product__CA1ECF0DF4A802D5").IsUnique();

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
                .HasConstraintName("FK__Product__Categor__70DDC3D8");

            entity.HasOne(d => d.ParentSkuNavigation).WithMany(p => p.InverseParentSkuNavigation)
                .HasPrincipalKey(p => p.Sku)
                .HasForeignKey(d => d.ParentSku)
                .HasConstraintName("FK__Product__ParentS__71D1E811");
        });

        modelBuilder.Entity<ProductDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductD__3214EC0781302DCA");

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

            entity.HasOne(d => d.Branch).WithMany(p => p.ProductDetails)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductDe__Branc__1BC821DD");

            entity.HasOne(d => d.ContainerCodeNavigation).WithMany(p => p.ProductDetails)
                .HasPrincipalKey(p => p.ContainerCode)
                .HasForeignKey(d => d.ContainerCode)
                .HasConstraintName("FK__ProductDe__Conta__1EA48E88");

            entity.HasOne(d => d.OnlineWarehouse).WithMany(p => p.ProductDetailOnlineWarehouses)
                .HasForeignKey(d => d.OnlineWarehouseId)
                .HasConstraintName("FK__ProductDe__Onlin__1DB06A4F");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductDe__Produ__1AD3FDA4");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.ProductDetailWarehouses)
                .HasForeignKey(d => d.WarehouseId)
                .HasConstraintName("FK__ProductDe__Wareh__1CBC4616");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Role__3214EC071A12B194");

            entity.ToTable("Role");

            entity.HasIndex(e => e.Name, "IX_Role_Name");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<SalaryConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalaryCo__3214EC078D92D52D");

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
                .HasConstraintName("FK__SalaryCon__Branc__43D61337");

            entity.HasOne(d => d.Employee).WithMany(p => p.SalaryConfigs)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalaryCon__Emplo__42E1EEFE");
        });

        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalesOrd__3214EC075FCCE121");

            entity.ToTable("SalesOrder");

            entity.HasIndex(e => e.BranchId, "IX_SalesOrder_BranchId");

            entity.HasIndex(e => e.CustomerId, "IX_SalesOrder_CustomerId");

            entity.HasIndex(e => e.EmployeeId, "IX_SalesOrder_EmployeeId");

            entity.HasIndex(e => e.OrderDate, "IX_SalesOrder_OrderDate").IsDescending();

            entity.HasIndex(e => e.PaymentStatus, "IX_SalesOrder_PaymentStatus");

            entity.HasIndex(e => e.Status, "IX_SalesOrder_Status");

            entity.HasIndex(e => e.InvoiceCode, "UQ__SalesOrd__0D9D7FF386C80DA2").IsUnique();

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
                .HasConstraintName("FK__SalesOrde__Branc__2EDAF651");

            entity.HasOne(d => d.Customer).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Custo__30C33EC3");

            entity.HasOne(d => d.Employee).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Emplo__2FCF1A8A");

            entity.HasOne(d => d.Voucher).WithMany(p => p.SalesOrders)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("FK__SalesOrde__Vouch__31B762FC");
        });

        modelBuilder.Entity<SalesOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalesOrd__3214EC075ECA48DF");

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
                .HasConstraintName("FK__SalesOrde__Produ__3A4CA8FD");

            entity.HasOne(d => d.Product).WithMany(p => p.SalesOrderDetailProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Produ__3864608B");

            entity.HasOne(d => d.ProductSkuNavigation).WithMany(p => p.SalesOrderDetailProductSkuNavigations)
                .HasPrincipalKey(p => p.Sku)
                .HasForeignKey(d => d.ProductSku)
                .HasConstraintName("FK__SalesOrde__Produ__395884C4");

            entity.HasOne(d => d.SalesOrder).WithMany(p => p.SalesOrderDetails)
                .HasForeignKey(d => d.SalesOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesOrde__Sales__37703C52");
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SystemLo__3214EC079FD2D8EE");

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
                .HasConstraintName("FK__SystemLog__Accou__04AFB25B");
        });

        modelBuilder.Entity<TransactionOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC0768D9AEEE");

            entity.ToTable("TransactionOrder");

            entity.HasIndex(e => e.OrderDate, "IX_TransactionOrder_OrderDate").IsDescending();

            entity.HasIndex(e => e.Status, "IX_TransactionOrder_Status");

            entity.HasIndex(e => e.Type, "IX_TransactionOrder_Type");

            entity.HasIndex(e => e.InvoiceCode, "UQ__Transact__0D9D7FF32AD13014").IsUnique();

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
                .HasDefaultValue("Ch? X? L?");
            entity.Property(e => e.SubTotal).HasColumnType("money");
            entity.Property(e => e.TaxAmount).HasColumnType("money");
            entity.Property(e => e.Type).HasMaxLength(100);

            entity.HasOne(d => d.Employee).WithMany(p => p.TransactionOrderEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Transacti__Emplo__0A9D95DB");

            entity.HasOne(d => d.FromWarehouse).WithMany(p => p.TransactionOrderFromWarehouses)
                .HasForeignKey(d => d.FromWarehouseId)
                .HasConstraintName("FK__Transacti__FromW__08B54D69");

            entity.HasOne(d => d.Supplier).WithMany(p => p.TransactionOrderSuppliers)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__Transacti__Suppl__0B91BA14");

            entity.HasOne(d => d.ToWarehouse).WithMany(p => p.TransactionOrderToWarehouses)
                .HasForeignKey(d => d.ToWarehouseId)
                .HasConstraintName("FK__Transacti__ToWar__09A971A2");
        });

        modelBuilder.Entity<TransactionOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC07C500547A");

            entity.ToTable("TransactionOrderDetail");

            entity.HasIndex(e => e.ContainerCode, "IX_TransactionOrderDetail_ContainerCode");

            entity.HasIndex(e => e.TransactionOrderId, "IX_TransactionOrderDetail_OrderId");

            entity.HasIndex(e => e.ProductId, "IX_TransactionOrderDetail_ProductId");

            entity.HasIndex(e => e.ContainerCode, "UQ__Transact__874FE470BF8E88C0").IsUnique();

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
                .HasConstraintName("FK__Transacti__Produ__14270015");

            entity.HasOne(d => d.TransactionOrder).WithMany(p => p.TransactionOrderDetails)
                .HasForeignKey(d => d.TransactionOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Transacti__Trans__1332DBDC");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Voucher__3214EC07484D72D1");

            entity.ToTable("Voucher");

            entity.HasIndex(e => e.Code, "UQ__Voucher__A25C5AA783460D05").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__Warehous__3214EC076240E393");

            entity.ToTable("Warehouse");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.BranchId).HasColumnName("BranchID");
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
                .HasConstraintName("FK__Warehouse__Branc__403A8C7D");
        });

        modelBuilder.Entity<WorkShift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WorkShif__3214EC07541D4A4A");

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
