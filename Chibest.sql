USE MASTER;
GO

IF EXISTS (SELECT NAME FROM SYS.DATABASES WHERE NAME = 'ChiBestDB')
BEGIN
    ALTER DATABASE ChiBestDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
	DROP DATABASE ChiBestDB;
END;
GO

CREATE DATABASE ChiBestDB;
GO

USE ChiBestDB;
GO

-- =====================================================================
-- Kho/Chi nhánh
CREATE TABLE Branch (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    [Name] NVARCHAR(255) NOT NULL,
    [Address] NVARCHAR(500) NOT NULL,
    PhoneNumber NVARCHAR(15),
    [Status] NVARCHAR(40) NOT NULL DEFAULT N'Hoạt Động',
);

CREATE TABLE Warehouse (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    [Name] NVARCHAR(255) NOT NULL,
    [Address] NVARCHAR(500) NOT NULL,
    PhoneNumber NVARCHAR(15),
    IsMainWarehouse BIT NOT NULL DEFAULT 0,          
    [Status] NVARCHAR(40) NOT NULL DEFAULT N'Hoạt Động',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    BranchID UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Branch](Id)
);
GO


-- =====================================================================
CREATE TABLE Account (
	Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    fcmToken NVARCHAR(255) NULL,
    RefreshToken NVARCHAR(MAX) NULL,
    RefreshTokenExpiryTime DATETIME NULL,
    AvartarURL NVARCHAR(MAX),
    Code NVARCHAR(100) UNIQUE NOT NULL,
	Email NVARCHAR(100) UNIQUE NOT NULL,
	[Password] NVARCHAR(MAX) NOT NULL,
	[Name] NVARCHAR(250) NOT NULL, 
	PhoneNumber NVARCHAR(15),
    [Address] NVARCHAR(MAX),
	CCCD NVARCHAR(20),
    TaxCode NVARCHAR(50),
    FaxNumber NVARCHAR(15),
	CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	    
	[Status] NVARCHAR(40) NOT NULL DEFAULT N'Hoạt Động',                -- Ngưng Hoạt Động   |   Bị Cấm	 	|   Đã Xóa
);
GO

CREATE NONCLUSTERED INDEX IX_Account_PhoneNumber ON dbo.Account(PhoneNumber);
GO

-- =====================================================================
-- Admin    |    Nhà Cung Cấp   |   Nhân Viên Chi Nhánh(Thu ngân, sale)
CREATE TABLE [Role] (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX)
);
GO

-- Role: lookups by Name
CREATE NONCLUSTERED INDEX IX_Role_Name ON dbo.[Role]([Name]);
GO

-- =====================================================================
CREATE TABLE AccountRole (
    PRIMARY KEY (AccountId, RoleId),
    StartDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	EndDate DATETIME,
    BranchId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Branch](Id), -- Thuộc chi nhánh nào
    AccountId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    RoleId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Role](Id),
);
GO

-- AccountRole: PK covers AccountId+RoleId; add indexes for Account-centric and Branch-centric queries
CREATE NONCLUSTERED INDEX IX_AccountRole_AccountId_StartDate ON dbo.AccountRole(AccountId, StartDate) INCLUDE (RoleId, BranchId, EndDate);
GO
CREATE NONCLUSTERED INDEX IX_AccountRole_BranchId_RoleId ON dbo.AccountRole(BranchId, RoleId) INCLUDE (AccountId, StartDate, EndDate);
GO

-- =====================================================================
-- Customer/Group Orgranizer
CREATE TABLE Customer (
	Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    AvartarURL NVARCHAR(MAX),
    Code NVARCHAR(20) UNIQUE NOT NULL,
    [Name] NVARCHAR(200) NOT NULL, 
	[Address] NVARCHAR(MAX),
    PhoneNumber NVARCHAR(15),
	DateOfBirth DATETIME,
    [Type] NVARCHAR(40) NOT NULL DEFAULt N'Cá Nhân',		-- Tổ Chức
	CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	LastActive DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	[Status] NVARCHAR(30) NOT NULL DEFAULT N'Đã Tạo',		-- Bị Cấm	|   Đã Xóa

    GroupId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Customer](Id)
);
GO
-- Customer: PhoneNumber lookups
CREATE NONCLUSTERED INDEX IX_Customer_PhoneNumber ON dbo.Customer(PhoneNumber);
GO

-- Customer: GroupId lookups
CREATE NONCLUSTERED INDEX IX_Customer_GroupId ON dbo.Customer(GroupId) INCLUDE (Id, [Name], PhoneNumber);
GO

-- =====================================================================
create table [Voucher](
	Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
	Code NVARCHAR(100) UNIQUE NOT NULL,
	[Name] NVARCHAR(250) NOT NULL,
	[Description] NVARCHAR(MAX),
	AvailableDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	ExpiredDate DATETIME NOT NULL,
	MinimumTransaction MONEY NOT NULL DEFAULT 0,
	MaxDiscountAmount MONEY NULL,
	DiscountPercent TINYINT DEFAULT 0 NOT NULL,
	UsageLimit INT NULL,
	UsedCount INT NOT NULL DEFAULT 0,
	[Status] NVARCHAR(40) NOT NULL DEFAULT N'Khả Dụng'		--Hết Hạn   |   Đã Xóa
);
go

-- =====================================================================
create table [CustomerVoucher](
	CollectedDate DATETIME NOT NULL,
    UsedDate DATETIME,
	[Status] NVARCHAR(40) NOT NULL DEFAULT N'Đã Nhận'		--Đã Dùng	|	Hết Hạn	|	Đã Xóa
	
    PRIMARY KEY (VoucherId, CustomerId),
	VoucherId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Voucher](Id),
	CustomerId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Customer](Id),
);
go

-- =====================================================================
CREATE TABLE Category (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    [Type] NVARCHAR(50) NOT NULL,                       -- vd: Áo / Quần / Váy
    [Name] NVARCHAR(150) NOT NULL,
    [Description] NVARCHAR(MAX)
);
GO

-- Category: type/name lookups
CREATE NONCLUSTERED INDEX IX_Category_Type_Name ON dbo.Category([Type], [Name]);
GO

-- =====================================================================
-- Sản phẩm tượng chưng
CREATE TABLE [Product] (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    AvartarURL NVARCHAR(MAX),
    SKU VARCHAR(50) UNIQUE NOT NULL,                    -- Mã sản phẩm (có thể là mã biến thể)
    [Name] NVARCHAR(250) NOT NULL,
    [Description] NVARCHAR(MAX),
    Color NVARCHAR(100),
    Size NVARCHAR(100),
    Style NVARCHAR(100),                                -- (slim, regular, classic, oversized)
    Brand NVARCHAR(100),
    Material NVARCHAR(100),
    [Weight] INT NOT NULL DEFAULT 0,                    -- gram
    IsMaster BIT NOT NULL DEFAULT 1,                    -- 1 = sản phẩm gốc, 0 = biến thể
    [Status] NVARCHAR(40) NOT NULL DEFAULT N'Khả Dụng', -- Tạm Ngưng | Ngừng Nhập

    CategoryId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Category](Id),
    -- Tham chiếu đến mã SKU gốc
    ParentSKU VARCHAR(50) NULL FOREIGN KEY REFERENCES [Product](SKU)
);
GO

-- Product: SKU unique already indexed; add indexes for name, category, IsMaster and ParentSKU joins
CREATE NONCLUSTERED INDEX IX_Product_Name ON dbo.[Product]([Name]) INCLUDE (SKU, Brand, CategoryId);
GO
CREATE NONCLUSTERED INDEX IX_Product_Category_IsMaster ON dbo.[Product](CategoryId, IsMaster) INCLUDE (SKU, [Name]);
GO
CREATE NONCLUSTERED INDEX IX_Product_ParentSKU ON dbo.[Product](ParentSKU) INCLUDE (SKU, Id);
GO
-- =====================================================================
-- Quản Lý tồn kho (kho/chi nhánh quản lý tồn & giá bán)
CREATE TABLE BranchStock (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    SellingPrice MONEY NOT NULL,                    -- Giá bán áp dụng tại chi nhánh
    ReOrderPriority INT NOT NULL DEFAULT 0,         -- số càng cao : ưu tiên càng cao
    AvailableQty INT NOT NULL DEFAULT 0,            -- số hàng còn lại thực tế
    ReservedQty INT NOT NULL DEFAULT 0,             -- số hàng đã đặt trước
    MinimumStock INT NOT NULL DEFAULT 0,
    LastUpdated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id),
    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Warehouse](Id),
    UNIQUE (ProductId, BranchId)                    -- 1 branch : 1 product
);
GO

-- BranchStock: unique constraint (ProductId, BranchId) exists; add branch-centric and quantity indexes
CREATE NONCLUSTERED INDEX IX_BranchStock_BranchId_AvailableQty ON dbo.BranchStock(BranchId, AvailableQty) INCLUDE (ProductId, SellingPrice, ReservedQty);
Go
CREATE NONCLUSTERED INDEX IX_BranchStock_ProductId ON dbo.BranchStock(ProductId) INCLUDE (BranchId, SellingPrice, AvailableQty, LastUpdated);
GO

-- =====================================================================
-- Quản Lí đơn/ yêu cầu các loại
CREATE TABLE TransactionOrder (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    InvoiceCode NVARCHAR(100) NOT NULL UNIQUE,              -- NYC-CUST105-INV78 (location + client + sequence)
    [Type] NVARCHAR(100),                                   -- Nhập Hàng | Xuất Hàng | Vận Chuyển  | Trả Hàng Nhập | Trả Hàng Xuất | Other (Utilities, Rent, Transport)
    OrderDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Thông tin giao hàng
    ReceiverName NVARCHAR(250) NOT NULL,
    ReceiverPhone NVARCHAR(15),
    ReceiverAddress NVARCHAR(MAX) NOT NULL,

    -- Thông tin giao hàng
    ExpectedDeliveryDate DATETIME,
    ActualDeliveryDate DATETIME,

    -- Thông tin thanh toán
    PayMethod NVARCHAR(40) NOT NULL DEFAULT N'Tiền Mặt',
    SubTotal MONEY NOT NULL DEFAULT 0,
    DiscountAmount MONEY NOT NULL DEFAULT 0,
    TaxAmount MONEY NOT NULL DEFAULT 0,
    ShippingFee MONEY NOT NULL DEFAULT 0,
    FinalCost AS (SubTotal - DiscountAmount + TaxAmount + ShippingFee) PERSISTED,
    Note NVARCHAR(MAX),
    [Status] NVARCHAR(40) NOT NULL DEFAULT 'Chờ Xử Lý',

    FromWarehouseId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Warehouse](Id),                    -- Kho nguồn (NULL nếu nhập từ nhà cung cấp)
    ToWarehouseId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Warehouse](Id),                      -- Kho đích
    EmployeeId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Account](Id),                           -- Nhân viên thực hiện giao dịch
    SupplierId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Account](Id),                           -- Nhà cung cấp (chỉ khi TransactionType = 'Import')
);
GO

CREATE NONCLUSTERED INDEX IX_TransactionOrder_Type ON [TransactionOrder]([Type]);
CREATE NONCLUSTERED INDEX IX_TransactionOrder_Status ON [TransactionOrder]([Status]);
CREATE NONCLUSTERED INDEX IX_TransactionOrder_OrderDate ON [TransactionOrder](OrderDate DESC);
GO

-- =====================================================================
-- Chi Tiết Đơn Xuất Nhập
CREATE TABLE TransactionOrderDetail (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    ContainerCode NVARCHAR(100) NOT NULL UNIQUE,                -- Mã lô hàng
    Quantity INT NOT NULL,
    UnitPrice MONEY NOT NULL,
    DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    DiscountAmount MONEY NOT NULL DEFAULT 0,
    TotalPrice AS (Quantity * UnitPrice - DiscountAmount) PERSISTED,
    PurchaseDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Note NVARCHAR(MAX),

    TransactionOrderId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [TransactionOrder](Id),
    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id)
);
GO

CREATE NONCLUSTERED INDEX IX_TransactionOrderDetail_OrderId ON [TransactionOrderDetail](TransactionOrderId);
CREATE NONCLUSTERED INDEX IX_TransactionOrderDetail_ProductId ON [TransactionOrderDetail](ProductId);
CREATE NONCLUSTERED INDEX IX_TransactionOrderDetail_ContainerCode ON [TransactionOrderDetail](ContainerCode);
GO

-- =====================================================================
-- Chi tiết sản phẩm thực tế ở mỗi chi nhánh(mỗi item vật lý: barcode/RFID).
CREATE TABLE ProductDetail (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    ChipCode VARCHAR(100),                                  -- RFID | barcode | null
    PurchasePrice MONEY NOT NULL DEFAULT 0,
    ImportDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- Ngày nhập vào hệ thống
    LastTransactionDate DATETIME,
    [Status] NVARCHAR(50) DEFAULT 'Khả Dụng',               -- Đã Đặt , Đã Bán, Bị Hư, Đang Giao
    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id),
    WarehouseId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Warehouse](Id),                            -- hiện thuộc chi nhánh nào
    ContainerCode NVARCHAR(100) FOREIGN KEY REFERENCES [TransactionOrderDetail](ContainerCode)      -- thuộc lô hàng nào
);
GO

CREATE NONCLUSTERED INDEX IX_ProductDetail_ChipCode ON [ProductDetail](ChipCode);
CREATE NONCLUSTERED INDEX IX_ProductDetail_Status ON [ProductDetail]([Status]);
GO

-- =====================================================================
-- Đơn bán hàng của mỗi chi nhánh
CREATE TABLE SalesOrder (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    InvoiceCode NVARCHAR(100) NOT NULL UNIQUE,
    OrderDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Thông tin khách hàng
    CustomerName NVARCHAR(250) NOT NULL,
    CustomerPhone NVARCHAR(15),
    CustomerEmail NVARCHAR(100),
    
    
    -- Thông tin giao hàng
    DeliveryMethod NVARCHAR(50) NOT NULL DEFAULT N'Tại Cửa Hàng',
    ShippingAddress NVARCHAR(500),
    ShippingPhone NVARCHAR(15),
    ExpectedDeliveryDate DATETIME NULL,
    ActualDeliveryDate DATETIME NULL,
    
    -- Thông tin thanh toán
    PaymentMethod NVARCHAR(50) NOT NULL DEFAULT N'Tiền Mặt',
    PaymentStatus NVARCHAR(50) NOT NULL DEFAULT N'Chờ Thanh Toán',
    SubTotal MONEY NOT NULL DEFAULT 0,
    DiscountAmount MONEY NOT NULL DEFAULT 0,
    VoucherAmount MONEY NOT NULL DEFAULT 0,
    ShippingFee MONEY NOT NULL DEFAULT 0,
    TaxAmount MONEY NOT NULL DEFAULT 0,
    FinalAmount AS (SubTotal - DiscountAmount - VoucherAmount + ShippingFee + TaxAmount) PERSISTED,
    Note NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    [Status] NVARCHAR(50) NOT NULL DEFAULT N'Đặt Trước',

    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Warehouse](Id),
    EmployeeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    CustomerId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Customer](Id),
    VoucherId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Voucher](Id),
);
GO

CREATE NONCLUSTERED INDEX IX_SalesOrder_CustomerId ON [SalesOrder](CustomerId);
CREATE NONCLUSTERED INDEX IX_SalesOrder_BranchId ON [SalesOrder](BranchId);
CREATE NONCLUSTERED INDEX IX_SalesOrder_EmployeeId ON [SalesOrder](EmployeeId);
CREATE NONCLUSTERED INDEX IX_SalesOrder_Status ON [SalesOrder]([Status]);
CREATE NONCLUSTERED INDEX IX_SalesOrder_PaymentStatus ON [SalesOrder](PaymentStatus);
CREATE NONCLUSTERED INDEX IX_SalesOrder_OrderDate ON [SalesOrder](OrderDate DESC);
GO

-- =====================================================================
-- chi tiết đơn bán hàng
CREATE TABLE SalesOrderDetail (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    ItemName NVARCHAR(250) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice MONEY NOT NULL,
    DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    DiscountAmount MONEY NOT NULL DEFAULT 0,
    TotalPrice AS (Quantity * UnitPrice - DiscountAmount) PERSISTED,
    Note NVARCHAR(MAX),
    SalesOrderId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [SalesOrder](Id),
    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id),
    ProductSKU VARCHAR(50) FOREIGN KEY REFERENCES [Product](SKU),
    ProductDetailId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [ProductDetail](Id)
);
GO

CREATE NONCLUSTERED INDEX IX_SalesOrderDetail_SalesOrderId ON [SalesOrderDetail](SalesOrderId);
CREATE NONCLUSTERED INDEX IX_SalesOrderDetail_ProductId ON [SalesOrderDetail](ProductId);
GO

-- ==========================================================================================================================================
-- HỆ THỐNG QUẢN LÝ LƯƠNG
-- ==========================================================================================================================================

-- Cấu hình ca làm việc
CREATE TABLE WorkShift (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsOvernight BIT NOT NULL DEFAULT 0,
    ShiftCoefficient DECIMAL(5,2) NOT NULL DEFAULT 1.0,
    [Description] NVARCHAR(MAX)
);
GO

-- Cấu hình lương cơ bản theo nhân viên
CREATE TABLE SalaryConfig (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    EmployeeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Warehouse](Id),
    
    -- Lương cơ bản
    SalaryType NVARCHAR(50) NOT NULL DEFAULT N'Theo Tháng',
    BaseSalary MONEY NOT NULL,
    HourlyRate MONEY,
    
    -- Phụ cấp cố định
    PositionAllowance MONEY NOT NULL DEFAULT 0,
    TransportAllowance MONEY NOT NULL DEFAULT 0,
    MealAllowance MONEY NOT NULL DEFAULT 0,
    PhoneAllowance MONEY NOT NULL DEFAULT 0,
    HousingAllowance MONEY NOT NULL DEFAULT 0,
    
    -- Hệ số
    OvertimeCoefficient DECIMAL(5,2) NOT NULL DEFAULT 1.5,
    HolidayCoefficient DECIMAL(5,2) NOT NULL DEFAULT 2.0,
    WeekendCoefficient DECIMAL(5,2) NOT NULL DEFAULT 1.3,
    
    EffectiveDate DATE NOT NULL,
    ExpiryDate DATE,
    [Status] NVARCHAR(40) NOT NULL DEFAULT N'Đang Áp Dụng'
);
GO

CREATE NONCLUSTERED INDEX IX_SalaryConfig_EmployeeId ON [SalaryConfig](EmployeeId);
GO

-- Chấm công
CREATE TABLE Attendance (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    WorkDate DATE NOT NULL,
    CheckInTime DATETIME,
    CheckOutTime DATETIME,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Note NVARCHAR(MAX),

    WorkHours DECIMAL(5,2) NOT NULL DEFAULT 0,                      -- Số giờ làm việc
    OvertimeHours DECIMAL(5,2) NOT NULL DEFAULT 0,
    DayType NVARCHAR(50) NOT NULL DEFAULT N'Ngày Thường',           -- Loại ngày
    AttendanceStatus NVARCHAR(50) NOT NULL DEFAULT N'Có Mặt',       -- Trạng thái

    EmployeeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Warehouse](Id),
    WorkShiftId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [WorkShift](Id)
);
GO

CREATE NONCLUSTERED INDEX IX_Attendance_EmployeeId_WorkDate ON [Attendance](EmployeeId, WorkDate DESC);
CREATE NONCLUSTERED INDEX IX_Attendance_BranchId ON [Attendance](BranchId);
GO

-- Hoa hồng và thưởng
CREATE TABLE Commission (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    EmployeeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    SalesOrderId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [SalesOrder](Id),
    
    CommissionType NVARCHAR(50) NOT NULL,
    Amount MONEY NOT NULL,
    CalculationBase MONEY NULL,
    CommissionRate DECIMAL(5,2) NULL,
    
    PeriodMonth INT NOT NULL,
    PeriodYear INT NOT NULL,
    
    Note NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE NONCLUSTERED INDEX IX_Commission_EmployeeId ON [Commission](EmployeeId);
CREATE NONCLUSTERED INDEX IX_Commission_Period ON [Commission](PeriodYear, PeriodMonth);
GO

-- Khấu trừ
CREATE TABLE Deduction (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    EmployeeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    
    DeductionType NVARCHAR(50) NOT NULL,
    Amount MONEY NOT NULL,
    
    PeriodMonth INT NOT NULL,
    PeriodYear INT NOT NULL,
    
    [Description] NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE NONCLUSTERED INDEX IX_Deduction_EmployeeId ON [Deduction](EmployeeId);
CREATE NONCLUSTERED INDEX IX_Deduction_Period ON [Deduction](PeriodYear, PeriodMonth);
GO

-- Bảng lương
CREATE TABLE Payroll (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    EmployeeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Warehouse](Id),
    
    PeriodMonth INT NOT NULL,
    PeriodYear INT NOT NULL,
    
    TotalWorkDays INT NOT NULL DEFAULT 0,               -- Công
    TotalWorkHours DECIMAL(10,2) NOT NULL DEFAULT 0,
    TotalOvertimeHours DECIMAL(10,2) NOT NULL DEFAULT 0,
    StandardWorkDays INT NOT NULL DEFAULT 26,
    
    BaseSalary MONEY NOT NULL DEFAULT 0,                -- Lương cơ bản
    ActualBaseSalary MONEY NOT NULL DEFAULT 0,

    TotalAllowance MONEY NOT NULL DEFAULT 0,            -- Phụ cấp
    OvertimeSalary MONEY NOT NULL DEFAULT 0,            -- Lương tăng ca

    TotalCommission MONEY NOT NULL DEFAULT 0,           -- Hoa hồng và thưởng
    TotalBonus MONEY NOT NULL DEFAULT 0,
    
    TotalDeduction MONEY NOT NULL DEFAULT 0,            -- Khấu trừ
    
    SocialInsurance MONEY NOT NULL DEFAULT 0,           -- Bảo hiểm
    HealthInsurance MONEY NOT NULL DEFAULT 0,
    UnemploymentInsurance MONEY NOT NULL DEFAULT 0,
    
    TaxableIncome MONEY NOT NULL DEFAULT 0,             -- Thuế
    PersonalTax MONEY NOT NULL DEFAULT 0,
    
    -- Tổng
    GrossSalary AS (ActualBaseSalary + TotalAllowance + OvertimeSalary + TotalCommission + TotalBonus) PERSISTED,
    NetSalary AS (ActualBaseSalary + TotalAllowance + OvertimeSalary + TotalCommission + TotalBonus 
                  - TotalDeduction - SocialInsurance - HealthInsurance - UnemploymentInsurance - PersonalTax) PERSISTED,
    
    PaymentDate DATE NULL,
    PaymentMethod NVARCHAR(50) DEFAULT N'Chuyển Khoản',
    PaymentStatus NVARCHAR(50) NOT NULL DEFAULT N'Chờ Thanh Toán',
    
    Note NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE(EmployeeId, PeriodYear, PeriodMonth)
);
GO

CREATE NONCLUSTERED INDEX IX_Payroll_EmployeeId ON [Payroll](EmployeeId);
CREATE NONCLUSTERED INDEX IX_Payroll_Period ON [Payroll](PeriodYear, PeriodMonth);
CREATE NONCLUSTERED INDEX IX_Payroll_Status ON [Payroll](PaymentStatus);
GO

-- Chi tiết bảng lương
CREATE TABLE PayrollDetail (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    PayrollId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Payroll](Id),
    
    ItemType NVARCHAR(50) NOT NULL,
    ItemName NVARCHAR(250) NOT NULL,
    ItemAmount MONEY NOT NULL,
    IsAddition BIT NOT NULL DEFAULT 1,
    
    [Description] NVARCHAR(MAX),
    ReferenceId UNIQUEIDENTIFIER NULL
);
GO

CREATE NONCLUSTERED INDEX IX_PayrollDetail_PayrollId ON [PayrollDetail](PayrollId);
GO

-- =====================================================================
CREATE TABLE SystemLog (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    
    -- Thông tin hành động
    [Action] NVARCHAR(50) NOT NULL,
    EntityType NVARCHAR(100) NOT NULL,
    EntityId UNIQUEIDENTIFIER NULL,
    
    -- Dữ liệu thay đổi
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    
    -- Mô tả
    [Description] NVARCHAR(MAX),
    
    -- Thông tin người dùng
    AccountId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Account](Id),
    AccountName NVARCHAR(250),
    
    -- Thông tin kỹ thuật
    IPAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    
    -- Thời gian
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Phân loại
    LogLevel NVARCHAR(20) NOT NULL DEFAULT 'INFO',
    Module NVARCHAR(100)
);
GO

CREATE NONCLUSTERED INDEX IX_SystemLog_EntityType_EntityId ON [SystemLog](EntityType, EntityId);
CREATE NONCLUSTERED INDEX IX_SystemLog_AccountId ON [SystemLog](AccountId);
CREATE NONCLUSTERED INDEX IX_SystemLog_CreatedAt ON [SystemLog](CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_SystemLog_Action ON [SystemLog]([Action]);
GO
