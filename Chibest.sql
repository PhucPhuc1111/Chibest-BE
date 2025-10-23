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
-- =============================================
-- MODULE 1: ORGANIZATION & LOCATION
-- Quản lý chi nhánh và kho
-- =============================================

CREATE TABLE Branch (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    Code NVARCHAR(50) UNIQUE NOT NULL,
    [Name] NVARCHAR(255) NOT NULL,
    [Address] NVARCHAR(500) NOT NULL,
    PhoneNumber NVARCHAR(15),
    [Status] NVARCHAR(40) NOT NULL DEFAULT N'Hoạt Động',
    IsFranchise BIT NOT NULL DEFAULT 0,
    OwnerName NVARCHAR(255) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE TABLE Warehouse (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    Code NVARCHAR(50) UNIQUE NOT NULL,
    [Name] NVARCHAR(255) NOT NULL,
    [Address] NVARCHAR(500) NOT NULL,
    PhoneNumber NVARCHAR(15),
    IsMainWarehouse BIT NOT NULL DEFAULT 0,
    IsOnlineWarehouse BIT NOT NULL DEFAULT 0,
    [Status] NVARCHAR(40) NOT NULL DEFAULT N'Hoạt Động',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    BranchId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Branch](Id)
);
GO

CREATE NONCLUSTERED INDEX IX_Warehouse_BranchId ON Warehouse(BranchId);
GO

-- =============================================
-- MODULE 2: ACCOUNT & ROLE MANAGEMENT
-- Quản lý tài khoản, phân quyền
-- =============================================

CREATE TABLE Account (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    Code NVARCHAR(100) UNIQUE NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    [Password] NVARCHAR(MAX) NOT NULL,
    [Name] NVARCHAR(250) NOT NULL,
    PhoneNumber NVARCHAR(15),
    [Address] NVARCHAR(MAX),
    CCCD NVARCHAR(20),
    TaxCode NVARCHAR(50),
    FaxNumber NVARCHAR(15),
    AvatarURL NVARCHAR(MAX),
    fcmToken NVARCHAR(255),
    RefreshToken NVARCHAR(MAX),
    RefreshTokenExpiryTime DATETIME,
    [Status] NVARCHAR(40) NOT NULL DEFAULT N'Hoạt Động',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE NONCLUSTERED INDEX IX_Account_PhoneNumber ON Account(PhoneNumber);
CREATE NONCLUSTERED INDEX IX_Account_Email ON Account(Email);
GO

CREATE TABLE [Role] (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX)
);
GO

CREATE NONCLUSTERED INDEX IX_Role_Name ON [Role]([Name]);
GO

CREATE TABLE AccountRole (
    AccountId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    RoleId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Role](Id),
    BranchId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Branch](Id),
    PRIMARY KEY (AccountId, RoleId, StartDate),

    StartDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    EndDate DATETIME
);
GO

CREATE NONCLUSTERED INDEX IX_AccountRole_AccountId ON AccountRole(AccountId, StartDate DESC);
CREATE NONCLUSTERED INDEX IX_AccountRole_BranchId_RoleId ON AccountRole(BranchId, RoleId) INCLUDE (AccountId);
GO

-- =============================================
-- MODULE 3: CUSTOMER MANAGEMENT
-- Quản lý khách hàng
-- =============================================

CREATE TABLE Customer (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    Code NVARCHAR(20) UNIQUE NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Address] NVARCHAR(MAX),
    PhoneNumber NVARCHAR(15),
    Email NVARCHAR(100),
    DateOfBirth DATETIME,
    AvatarURL NVARCHAR(MAX),
    [Status] NVARCHAR(30) NOT NULL DEFAULT N'Hoạt Động',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastActive DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    GroupId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Customer](Id)
);
GO

CREATE NONCLUSTERED INDEX IX_Customer_PhoneNumber ON Customer(PhoneNumber);
CREATE NONCLUSTERED INDEX IX_Customer_Email ON Customer(Email);
CREATE NONCLUSTERED INDEX IX_Customer_GroupId ON Customer(GroupId);
GO

-- =============================================
-- MODULE 4: PRODUCT CATALOG
-- Quản lý danh mục và sản phẩm
-- =============================================

CREATE TABLE Category (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    [Type] NVARCHAR(50) NOT NULL,
    [Name] NVARCHAR(150) NOT NULL,
    [Description] NVARCHAR(MAX),
    ParentId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Category](Id)
);
GO

CREATE NONCLUSTERED INDEX IX_Category_Type_Name ON Category([Type], [Name]);
CREATE NONCLUSTERED INDEX IX_Category_ParentId ON Category(ParentId);
GO

CREATE TABLE [Product] (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    SKU VARCHAR(50) UNIQUE NOT NULL,
    [Name] NVARCHAR(250) NOT NULL,
    [Description] NVARCHAR(MAX),
    AvatarURL NVARCHAR(MAX),
    Color NVARCHAR(100),
    Size NVARCHAR(100),
    Style NVARCHAR(100),
    Brand NVARCHAR(100),
    Material NVARCHAR(100),
    [Weight] INT NOT NULL DEFAULT 0,
    IsMaster BIT NOT NULL DEFAULT 1,
    [Status] NVARCHAR(40) NOT NULL DEFAULT N'Khả Dụng',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CategoryId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Category](Id),
    ParentSKU VARCHAR(50) NULL FOREIGN KEY REFERENCES [Product](SKU),
);
GO

CREATE NONCLUSTERED INDEX IX_Product_Name ON [Product]([Name]);
CREATE NONCLUSTERED INDEX IX_Product_CategoryId ON [Product](CategoryId);
CREATE NONCLUSTERED INDEX IX_Product_ParentSKU ON [Product](ParentSKU);
GO

-- =============================================
-- MODULE 5: PRICING MANAGEMENT
-- Quản lý giá theo thời điểm và địa điểm
-- =============================================

-- Lịch sử giá bán theo chi nhánh
CREATE TABLE ProductPriceHistory (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    SellingPrice MONEY NOT NULL,
    EffectiveDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ExpiryDate DATETIME,
    Note NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CreatedBy UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Account](Id),
    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id),
    BranchId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Branch](Id), -- NULL = giá chung cho tất cả chi nhánh
);
GO

CREATE NONCLUSTERED INDEX IX_ProductPriceHistory_Product_Branch ON ProductPriceHistory(ProductId, BranchId, EffectiveDate DESC);
GO

-- Lịch sử giá nhập từ nhà cung cấp
CREATE TABLE PurchasePriceHistory (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    PurchasePrice MONEY NOT NULL,
    EffectiveDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ExpiryDate DATETIME NULL,
    MinOrderQty INT NULL,
    Note NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id),
    SupplierId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Account](Id)
);
GO

CREATE NONCLUSTERED INDEX IX_PurchasePriceHistory_Product_Supplier ON PurchasePriceHistory(ProductId, SupplierId, EffectiveDate DESC);
GO

-- =============================================
-- MODULE 6: INVENTORY MANAGEMENT
-- Quản lý tồn kho theo chi nhánh/kho
-- =============================================

CREATE TABLE BranchStock (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id),
    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Branch](Id),
    WarehouseId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Warehouse](Id),
    
    -- Số lượng
    AvailableQty INT NOT NULL DEFAULT 0,
    ReservedQty INT NOT NULL DEFAULT 0,
    InTransitQty INT NOT NULL DEFAULT 0,
    DefectiveQty INT NOT NULL DEFAULT 0,
    TotalQty AS (AvailableQty + ReservedQty + InTransitQty + DefectiveQty) PERSISTED,
    
    -- Ngưỡng tồn kho
    MinimumStock INT NOT NULL DEFAULT 0,
    MaximumStock INT NOT NULL DEFAULT 0,
    ReorderPoint INT NOT NULL DEFAULT 0,
    ReorderQty INT NOT NULL DEFAULT 0,
    
    -- Giá bán hiện tại (tham chiếu, có thể lấy từ PriceHistory)
    CurrentSellingPrice MONEY NOT NULL DEFAULT 0,
    
    LastUpdated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT UQ_BranchStock_Product_Branch UNIQUE (ProductId, BranchId, WarehouseId)
);
GO

CREATE NONCLUSTERED INDEX IX_BranchStock_BranchId ON BranchStock(BranchId, AvailableQty);
CREATE NONCLUSTERED INDEX IX_BranchStock_ProductId ON BranchStock(ProductId);
GO

-- Tracking chi tiết sản phẩm vật lý (RFID/Barcode)
CREATE TABLE ProductDetail (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    ChipCode VARCHAR(100) UNIQUE,
    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id),
    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Branch](Id),
    WarehouseId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Warehouse](Id),
    
    -- Thông tin nhập hàng
    PurchasePrice MONEY NOT NULL DEFAULT 0,
    ImportDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    SupplierId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Account](Id),
    
    -- Trạng thái và vị trí hiện tại
    [Status] NVARCHAR(50) NOT NULL DEFAULT N'Khả Dụng',
    LastTransactionDate DATETIME NULL,
    LastTransactionType NVARCHAR(50) NULL,
    
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE NONCLUSTERED INDEX IX_ProductDetail_ChipCode ON ProductDetail(ChipCode);
CREATE NONCLUSTERED INDEX IX_ProductDetail_ProductId_Status ON ProductDetail(ProductId, [Status]);
CREATE NONCLUSTERED INDEX IX_ProductDetail_BranchId_WarehouseId ON ProductDetail(BranchId, WarehouseId);
GO

-- =============================================
-- MODULE 7: STOCK MOVEMENT TRACKING
-- Tracking mọi biến động kho
-- =============================================

CREATE TABLE StockMovement (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    MovementCode NVARCHAR(100) UNIQUE NOT NULL,
    MovementType NVARCHAR(50) NOT NULL, -- Nhập Kho, Xuất Kho, Chuyển Kho, Điều Chỉnh, Kiểm Kê, Bán Hàng, Trả Hàng
    MovementDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id),
    
    -- Vị trí nguồn và đích
    FromBranchId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Branch](Id),
    FromWarehouseId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Warehouse](Id),
    ToBranchId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Branch](Id),
    ToWarehouseId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Warehouse](Id),
    
    -- Số lượng
    Quantity INT NOT NULL,
    UnitPrice MONEY NULL,
    TotalValue MONEY NULL,
    
    -- Tham chiếu
    ReferenceType NVARCHAR(50) NULL, -- PurchaseOrder, SalesOrder, TransferOrder, etc.
    ReferenceId UNIQUEIDENTIFIER NULL,
    ReferenceCode NVARCHAR(100) NULL,
    
    -- Chi tiết item (nếu track RFID/Barcode)
    ProductDetailIds NVARCHAR(MAX) NULL, -- JSON array of IDs
    
    Note NVARCHAR(MAX),
    CreatedBy UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Account](Id),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE NONCLUSTERED INDEX IX_StockMovement_Type_Date ON StockMovement(MovementType, MovementDate DESC);
CREATE NONCLUSTERED INDEX IX_StockMovement_ProductId ON StockMovement(ProductId, MovementDate DESC);
CREATE NONCLUSTERED INDEX IX_StockMovement_FromBranch ON StockMovement(FromBranchId, MovementDate DESC);
CREATE NONCLUSTERED INDEX IX_StockMovement_ToBranch ON StockMovement(ToBranchId, MovementDate DESC);
CREATE NONCLUSTERED INDEX IX_StockMovement_Reference ON StockMovement(ReferenceType, ReferenceId);
GO

-- =============================================
-- MODULE 8: PURCHASE ORDERS
-- Quản lý đơn nhập hàng từ NCC
-- =============================================

CREATE TABLE PurchaseOrder (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    InvoiceCode NVARCHAR(100) NOT NULL UNIQUE,              -- NYC-CUST105-INV78 (location + client + sequence)                                
    OrderDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- Thời gian giao hàng
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ActualDeliveryDate DATETIME,
    -- Thông tin thanh toán
    PayMethod NVARCHAR(40) DEFAULT N'Tiền Mặt',
    SubTotal MONEY NOT NULL DEFAULT 0,
    DiscountAmount MONEY NOT NULL DEFAULT 0,
    TaxAmount MONEY NOT NULL DEFAULT 0,
    Paid MONEY NOT NULL DEFAULT 0,
    Note NVARCHAR(MAX),
    [Status] NVARCHAR(40) NOT NULL DEFAULT 'Chờ Xử Lý',
    WarehouseId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Warehouse](Id),                        -- Kho nhận
    EmployeeId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Account](Id),                           -- Nhân viên xác nhận giao dịch
    SupplierId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Account](Id),                           
);
GO

CREATE NONCLUSTERED INDEX IX_PurchaseOrder_InvoiceCode ON [PurchaseOrder]([InvoiceCode]);
CREATE NONCLUSTERED INDEX IX_TransactionOrder_Status ON [PurchaseOrder]([Status]);
CREATE NONCLUSTERED INDEX IX_TransactionOrder_OrderDate ON [PurchaseOrder](OrderDate DESC);
GO

-- =====================================================================
-- Chi Tiết Đơn Nhập
CREATE TABLE PurchaseOrderDetail (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    ContainerCode NVARCHAR(100) NOT NULL UNIQUE,                -- Mã lô hàng
    Quantity INT NOT NULL,
    ActualQuantity INT,
    ReFee money not null,
    UnitPrice MONEY NOT NULL,
    Discount DECIMAL(5,2) NOT NULL DEFAULT 0,
    Note NVARCHAR(MAX),
    PurchaseOrderId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [PurchaseOrder](Id),
    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id)
);
GO

CREATE NONCLUSTERED INDEX IX_TransactionOrderDetail_OrderId ON [PurchaseOrderDetail](PurchaseOrderId);
CREATE NONCLUSTERED INDEX IX_TransactionOrderDetail_ProductId ON [PurchaseOrderDetail](ProductId);
CREATE NONCLUSTERED INDEX IX_TransactionOrderDetail_ContainerCode ON [PurchaseOrderDetail](ContainerCode);
GO

-- =============================================
-- MODULE 9: TRANSFER ORDERS
-- Quản lý chuyển kho và Trả hàng lỗi
-- =============================================
CREATE TABLE TransferOrder (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    InvoiceCode NVARCHAR(100) NOT NULL UNIQUE,              -- NYC-CUST105-INV78 (location + client + sequence)                                
    OrderDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- Thời gian giao hàng
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ActualDeliveryDate DATETIME,
    -- Thông tin thanh toán
    PayMethod NVARCHAR(40) DEFAULT N'Tiền Mặt',
    SubTotal MONEY NOT NULL DEFAULT 0,
    DiscountAmount MONEY NOT NULL DEFAULT 0,
    TaxAmount MONEY NOT NULL DEFAULT 0,
    Paid MONEY NOT NULL DEFAULT 0,
    Note NVARCHAR(MAX),
    [Status] NVARCHAR(40) NOT NULL DEFAULT 'Chờ Xử Lý',
    EmployeeId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Account](Id),                           -- Nhân viên xác nhận giao dịch
    FromWarehouseId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Warehouse](Id),                    -- Kho nguồn 
    ToWarehouseId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES [Warehouse](Id),                      -- Kho đích
);
GO

CREATE NONCLUSTERED INDEX IX_TransferOrder_Status ON [TransferOrder]([Status]);
CREATE NONCLUSTERED INDEX IX_TransferOrder_OrderDate ON [TransferOrder](OrderDate DESC);
GO

-- =====================================================================
-- Chi Tiết Đơn  chuyển
CREATE TABLE TransferOrderDetail (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    ContainerCode NVARCHAR(100) NOT NULL UNIQUE,                -- Mã lô hàng
    Quantity INT NOT NULL,
    ActualQuantity INT,
    ExtraFee money not null,
    CommissionFee money not null,
    UnitPrice MONEY NOT NULL,
    Discount DECIMAL(5,2) NOT NULL DEFAULT 0,
    Note NVARCHAR(MAX),
    TransferOrderId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [TransferOrder](Id),
    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id)
);
GO

CREATE NONCLUSTERED INDEX IX_TransferOrderDetail_OrderId ON [TransferOrderDetail](TransferOrderId);
CREATE NONCLUSTERED INDEX IX_TransferOrderDetail_ProductId ON [TransferOrderDetail](ProductId);
CREATE NONCLUSTERED INDEX IX_TransferOrderDetail_ContainerCode ON [TransferOrderDetail](ContainerCode);
GO

-- Phiếu đơn lỗi
CREATE TABLE PurchaseReturn (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    InvoiceCode NVARCHAR(100) NOT NULL UNIQUE,             
    OrderDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PayMethod NVARCHAR(40) DEFAULT N'Tiền Mặt',
    SubTotal MONEY NOT NULL DEFAULT 0,
    DiscountAmount MONEY NOT NULL DEFAULT 0,
    Paid MONEY NOT NULL DEFAULT 0,
    Note NVARCHAR(MAX),
    [Status] NVARCHAR(40) NOT NULL DEFAULT N'Chờ Xử Lý',
    EmployeeId UNIQUEIDENTIFIER NULL,
    WarehouseId UNIQUEIDENTIFIER NULL,
    SupplierId UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_PurchaseReturn_Employee FOREIGN KEY (EmployeeId) REFERENCES [Account](Id),
    CONSTRAINT FK_PurchaseReturn_Warehouse FOREIGN KEY (WarehouseId) REFERENCES [Warehouse](Id),
    CONSTRAINT FK_PurchaseReturn_Supplier FOREIGN KEY (SupplierId) REFERENCES [Account](Id)
);

GO
CREATE NONCLUSTERED INDEX IX_PurchaseReturn_Status ON [PurchaseReturn]([Status]);
CREATE NONCLUSTERED INDEX IX_PurchaseReturn_OrderDate ON [PurchaseReturn](OrderDate DESC);
GO
-- Chi Tiết Đơn lỗi
CREATE TABLE PurchaseReturnDetail (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    ContainerCode NVARCHAR(100) NOT NULL UNIQUE,                -- Mã lô hàng
    Quantity INT NOT NULL,
    UnitPrice MONEY NOT NULL,
    ReturnPrice MONEY NOT NULL,
    Note NVARCHAR(MAX),
    PurchaseReturnId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [PurchaseReturn](Id),
    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id)
);
GO
CREATE NONCLUSTERED INDEX IX_PurchaseReturnDetail_OrderId ON [PurchaseReturnDetail](PurchaseReturnId);
CREATE NONCLUSTERED INDEX IX_PurchaseReturnDetail_ProductId ON [PurchaseReturnDetail](ProductId);
CREATE NONCLUSTERED INDEX IX_PurchaseReturnDetail_ContainerCode ON [PurchaseReturnDetail](ContainerCode);
GO

-- =============================================
-- MODULE 10: STOCK ADJUSTMENT
-- Điều chỉnh và kiểm kê kho và Cân bằng kho
-- =============================================

CREATE TABLE StockAdjustment (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    AdjustmentCode NVARCHAR(100) UNIQUE NOT NULL,
    AdjustmentDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AdjustmentType NVARCHAR(50) NOT NULL, -- Kiểm Kê, Điều Chỉnh, Hư Hỏng, Mất Mát
    
    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Branch](Id),
    WarehouseId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Warehouse](Id),
    EmployeeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    
    TotalValueChange MONEY NOT NULL DEFAULT 0,
    [Status] NVARCHAR(40) NOT NULL DEFAULT N'Chờ Duyệt',
    
    Reason NVARCHAR(MAX),
    Note NVARCHAR(MAX),
    
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ApprovedBy UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Account](Id),
    ApprovedAt DATETIME NULL
);
GO

CREATE NONCLUSTERED INDEX IX_StockAdjustment_Type_Date ON StockAdjustment(AdjustmentType, AdjustmentDate DESC);
CREATE NONCLUSTERED INDEX IX_StockAdjustment_BranchId ON StockAdjustment(BranchId, AdjustmentDate DESC);
GO



CREATE TABLE StockAdjustmentDetail (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    StockAdjustmentId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [StockAdjustment](Id),
    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id),
    
    SystemQty INT NOT NULL, -- Số lượng trong hệ thống
    ActualQty INT NOT NULL, -- Số lượng thực tế
    DifferenceQty AS (ActualQty - SystemQty) PERSISTED,
    
    UnitCost MONEY NOT NULL DEFAULT 0,
    TotalValueChange AS ((ActualQty - SystemQty) * UnitCost) PERSISTED,
    
    Reason NVARCHAR(MAX),
    Note NVARCHAR(MAX)
);
GO

CREATE NONCLUSTERED INDEX IX_StockAdjustmentDetail_AdjustmentId ON StockAdjustmentDetail(StockAdjustmentId);
CREATE NONCLUSTERED INDEX IX_StockAdjustmentDetail_ProductId ON StockAdjustmentDetail(ProductId);
GO


-- =============================================
-- MODULE 11: SALES ORDERS
-- Quản lý đơn bán hàng
-- =============================================

CREATE TABLE [Voucher] (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    Code NVARCHAR(100) UNIQUE NOT NULL,
    [Name] NVARCHAR(250) NOT NULL,
    [Description] NVARCHAR(MAX),
    VoucherType NVARCHAR(50) NOT NULL DEFAULT N'Giảm Giá', -- Giảm Giá, Miễn Phí Ship, Quà Tặng
    
    AvailableDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ExpiredDate DATETIME NOT NULL,
    
    MinimumTransaction MONEY NOT NULL DEFAULT 0,
    MaxDiscountAmount MONEY NULL,
    DiscountPercent DECIMAL(5,2) DEFAULT 0,
    DiscountAmount MONEY DEFAULT 0,
    
    UsageLimit INT NULL,
    UsagePerCustomer INT DEFAULT 1,
    UsedCount INT NOT NULL DEFAULT 0,
    
    ApplicableProducts NVARCHAR(MAX) NULL, -- JSON array of product IDs
    ApplicableCategories NVARCHAR(MAX) NULL, -- JSON array of category IDs
    
    [Status] NVARCHAR(40) NOT NULL DEFAULT N'Khả Dụng',
    
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE TABLE CustomerVoucher (
    VoucherId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Voucher](Id),
    CustomerId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Customer](Id),
    
    CollectedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UsedDate DATETIME NULL,
    [Status] NVARCHAR(40) NOT NULL DEFAULT N'Đã Nhận',
    
    PRIMARY KEY (VoucherId, CustomerId)
);
GO

CREATE TABLE SalesOrder (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    OrderCode NVARCHAR(100) UNIQUE NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Thông tin khách hàng
    CustomerId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Customer](Id),
    CustomerName NVARCHAR(250) NOT NULL,
    CustomerPhone NVARCHAR(15),
    CustomerEmail NVARCHAR(100),
    
    -- Thông tin giao dịch
    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Branch](Id),
    WarehouseId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Warehouse](Id),
    EmployeeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    
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
    VoucherId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Voucher](Id),
    VoucherAmount MONEY NOT NULL DEFAULT 0,
    ShippingFee MONEY NOT NULL DEFAULT 0,
    TaxAmount MONEY NOT NULL DEFAULT 0,
    FinalAmount AS (SubTotal - DiscountAmount - VoucherAmount + ShippingFee + TaxAmount) PERSISTED,
    PaidAmount MONEY NOT NULL DEFAULT 0,
    
    [Status] NVARCHAR(50) NOT NULL DEFAULT N'Đặt Trước',
    Note NVARCHAR(MAX),
    
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE NONCLUSTERED INDEX IX_SalesOrder_CustomerId ON SalesOrder(CustomerId, OrderDate DESC);
CREATE NONCLUSTERED INDEX IX_SalesOrder_BranchId ON SalesOrder(BranchId, OrderDate DESC);
CREATE NONCLUSTERED INDEX IX_SalesOrder_Status ON SalesOrder([Status], OrderDate DESC);
CREATE NONCLUSTERED INDEX IX_SalesOrder_PaymentStatus ON SalesOrder(PaymentStatus);
GO

CREATE TABLE SalesOrderDetail (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    SalesOrderId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [SalesOrder](Id),
    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id),
    ProductSKU VARCHAR(50) NOT NULL,
    ProductDetailId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [ProductDetail](Id),
    
    ItemName NVARCHAR(250) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice MONEY NOT NULL,
    DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    DiscountAmount MONEY NOT NULL DEFAULT 0,
    TaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    TotalPrice AS (Quantity * UnitPrice - DiscountAmount + (Quantity * UnitPrice * TaxPercent / 100)) PERSISTED,
    
    Note NVARCHAR(MAX)
);
GO

CREATE NONCLUSTERED INDEX IX_SalesOrderDetail_OrderId ON SalesOrderDetail(SalesOrderId);
CREATE NONCLUSTERED INDEX IX_SalesOrderDetail_ProductId ON SalesOrderDetail(ProductId);
GO

-- Đơn trả hàng từ khách
CREATE TABLE SalesReturn (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    ReturnCode NVARCHAR(100) UNIQUE NOT NULL,
    ReturnDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    SalesOrderId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [SalesOrder](Id),
    CustomerId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Customer](Id),
    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Branch](Id),
    EmployeeId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Account](Id),
    
    RefundAmount MONEY NOT NULL DEFAULT 0,
    RefundMethod NVARCHAR(50) NOT NULL DEFAULT N'Tiền Mặt',
    
    [Status] NVARCHAR(40) NOT NULL DEFAULT N'Chờ Xử Lý',
    Reason NVARCHAR(MAX),
    Note NVARCHAR(MAX),
    
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE TABLE SalesReturnDetail (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    SalesReturnId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [SalesReturn](Id),
    SalesOrderDetailId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [SalesOrderDetail](Id),
    ProductId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Product](Id),
    ProductDetailId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [ProductDetail](Id),
    
    ReturnQty INT NOT NULL,
    UnitPrice MONEY NOT NULL,
    RefundAmount MONEY NOT NULL,
    
    Condition NVARCHAR(50) NOT NULL, -- Nguyên Vẹn, Hư Hỏng, Lỗi
    Note NVARCHAR(MAX)
);
GO

-- =============================================
-- MODULE 12: DEBT MANAGEMENT
-- Quản lý công nợ
-- =============================================

-- Công nợ nhà cung cấp
CREATE TABLE SupplierDebt (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    SupplierId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    
    TotalDebt MONEY NOT NULL DEFAULT 0,
    PaidAmount MONEY NOT NULL DEFAULT 0,
    RemainingDebt AS (TotalDebt - PaidAmount) PERSISTED,
    
    LastTransactionDate DATETIME NULL,
    LastUpdated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT UQ_SupplierDebt_Supplier UNIQUE (SupplierId)
);
GO

CREATE TABLE SupplierDebtHistory (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    SupplierId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    
    TransactionType NVARCHAR(50) NOT NULL, -- Phát Sinh Nợ, Thanh Toán, Điều Chỉnh
    TransactionDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Amount MONEY NOT NULL,
    
    ReferenceType NVARCHAR(50) NULL,
    ReferenceId UNIQUEIDENTIFIER NULL,
    ReferenceCode NVARCHAR(100) NULL,
    
    BalanceBefore MONEY NOT NULL,
    BalanceAfter MONEY NOT NULL,
    
    Note NVARCHAR(MAX),
    CreatedBy UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Account](Id),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE NONCLUSTERED INDEX IX_SupplierDebtHistory_Supplier ON SupplierDebtHistory(SupplierId, TransactionDate DESC);
GO

-- Công nợ chi nhánh (với trụ sở chính)
CREATE TABLE BranchDebt (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Branch](Id),
    
    TotalDebt MONEY NOT NULL DEFAULT 0,
    PaidAmount MONEY NOT NULL DEFAULT 0,
    RemainingDebt AS (TotalDebt - PaidAmount) PERSISTED,
    
    LastTransactionDate DATETIME NULL,
    LastUpdated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT UQ_BranchDebt_Branch UNIQUE (BranchId)
);
GO

CREATE TABLE BranchDebtHistory (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Branch](Id),
    
    TransactionType NVARCHAR(50) NOT NULL,
    TransactionDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Amount MONEY NOT NULL,
    
    ReferenceType NVARCHAR(50) NULL,
    ReferenceId UNIQUEIDENTIFIER NULL,
    ReferenceCode NVARCHAR(100) NULL,
    
    BalanceBefore MONEY NOT NULL,
    BalanceAfter MONEY NOT NULL,
    
    Note NVARCHAR(MAX),
    CreatedBy UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Account](Id),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE NONCLUSTERED INDEX IX_BranchDebtHistory_Branch ON BranchDebtHistory(BranchId, TransactionDate DESC);
GO

-- =============================================
-- MODULE 13: EMPLOYEE & PAYROLL
-- Quản lý nhân viên và lương
-- =============================================

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

CREATE TABLE SalaryConfig (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    EmployeeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Branch](Id),
    
    SalaryType NVARCHAR(50) NOT NULL DEFAULT N'Theo Tháng',
    BaseSalary MONEY NOT NULL,
    HourlyRate MONEY NULL,
    
    -- Phụ cấp
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
    ExpiryDate DATE NULL,
    [Status] NVARCHAR(40) NOT NULL DEFAULT N'Đang Áp Dụng',
    
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE NONCLUSTERED INDEX IX_SalaryConfig_EmployeeId ON SalaryConfig(EmployeeId, EffectiveDate DESC);
GO

CREATE TABLE Attendance (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    EmployeeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Branch](Id),
    WorkShiftId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [WorkShift](Id),
    
    WorkDate DATE NOT NULL,
    CheckInTime DATETIME NULL,
    CheckOutTime DATETIME NULL,
    
    WorkHours DECIMAL(5,2) NOT NULL DEFAULT 0,
    OvertimeHours DECIMAL(5,2) NOT NULL DEFAULT 0,
    
    DayType NVARCHAR(50) NOT NULL DEFAULT N'Ngày Thường',
    AttendanceStatus NVARCHAR(50) NOT NULL DEFAULT N'Có Mặt',
    
    Note NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT UQ_Attendance_Employee_Date UNIQUE (EmployeeId, WorkDate)
);
GO

CREATE NONCLUSTERED INDEX IX_Attendance_EmployeeId ON Attendance(EmployeeId, WorkDate DESC);
CREATE NONCLUSTERED INDEX IX_Attendance_BranchId ON Attendance(BranchId, WorkDate DESC);
GO

CREATE TABLE Commission (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    EmployeeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    
    CommissionType NVARCHAR(50) NOT NULL, -- Doanh Số, KPI, Thưởng Đặc Biệt
    Amount MONEY NOT NULL,
    CalculationBase MONEY NULL,
    CommissionRate DECIMAL(5,2) NULL,
    
    ReferenceType NVARCHAR(50) NULL,
    ReferenceId UNIQUEIDENTIFIER NULL,
    
    PeriodMonth INT NOT NULL,
    PeriodYear INT NOT NULL,
    
    Note NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE NONCLUSTERED INDEX IX_Commission_EmployeeId ON Commission(EmployeeId, PeriodYear DESC, PeriodMonth DESC);
GO

CREATE TABLE Deduction (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    EmployeeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    
    DeductionType NVARCHAR(50) NOT NULL, -- Đi Trễ, Nghỉ Không Phép, Vi Phạm, Khác
    Amount MONEY NOT NULL,
    
    PeriodMonth INT NOT NULL,
    PeriodYear INT NOT NULL,
    
    [Description] NVARCHAR(MAX),
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE NONCLUSTERED INDEX IX_Deduction_EmployeeId ON Deduction(EmployeeId, PeriodYear DESC, PeriodMonth DESC);
GO

CREATE TABLE Payroll (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    EmployeeId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Account](Id),
    BranchId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Branch](Id),
    
    PeriodMonth INT NOT NULL,
    PeriodYear INT NOT NULL,
    
    -- Công và giờ làm
    TotalWorkDays INT NOT NULL DEFAULT 0,
    TotalWorkHours DECIMAL(10,2) NOT NULL DEFAULT 0,
    TotalOvertimeHours DECIMAL(10,2) NOT NULL DEFAULT 0,
    StandardWorkDays INT NOT NULL DEFAULT 26,
    
    -- Lương cơ bản
    BaseSalary MONEY NOT NULL DEFAULT 0,
    ActualBaseSalary MONEY NOT NULL DEFAULT 0,
    
    -- Phụ cấp và tăng ca
    TotalAllowance MONEY NOT NULL DEFAULT 0,
    OvertimeSalary MONEY NOT NULL DEFAULT 0,
    
    -- Thưởng và hoa hồng
    TotalCommission MONEY NOT NULL DEFAULT 0,
    TotalBonus MONEY NOT NULL DEFAULT 0,
    
    -- Khấu trừ
    TotalDeduction MONEY NOT NULL DEFAULT 0,
    
    -- Bảo hiểm
    SocialInsurance MONEY NOT NULL DEFAULT 0,
    HealthInsurance MONEY NOT NULL DEFAULT 0,
    UnemploymentInsurance MONEY NOT NULL DEFAULT 0,
    
    -- Thuế
    TaxableIncome MONEY NOT NULL DEFAULT 0,
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
    
    CONSTRAINT UQ_Payroll_Employee_Period UNIQUE(EmployeeId, PeriodYear, PeriodMonth)
);
GO

CREATE NONCLUSTERED INDEX IX_Payroll_EmployeeId ON Payroll(EmployeeId, PeriodYear DESC, PeriodMonth DESC);
CREATE NONCLUSTERED INDEX IX_Payroll_Status ON Payroll(PaymentStatus, PeriodYear DESC, PeriodMonth DESC);
GO

-- =============================================
-- MODULE 14: SYSTEM AUDIT
-- Quản lý log hệ thống
-- =============================================

CREATE TABLE SystemLog (
    Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    
    [Action] NVARCHAR(50) NOT NULL,
    EntityType NVARCHAR(100) NOT NULL,
    EntityId UNIQUEIDENTIFIER NULL,
    
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    [Description] NVARCHAR(MAX),
    
    AccountId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES [Account](Id),
    AccountName NVARCHAR(250),
    
    IPAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    
    LogLevel NVARCHAR(20) NOT NULL DEFAULT 'INFO',
    Module NVARCHAR(100),
    
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
GO

CREATE NONCLUSTERED INDEX IX_SystemLog_CreatedAt ON SystemLog(CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_SystemLog_EntityType ON SystemLog(EntityType, EntityId, CreatedAt DESC);
CREATE NONCLUSTERED INDEX IX_SystemLog_AccountId ON SystemLog(AccountId, CreatedAt DESC);


