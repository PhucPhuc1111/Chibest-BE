USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Chibest')
BEGIN
    CREATE DATABASE Chibest;
END;
GO

USE Chibest;
GO

-- Bỏ ràng buộc FOREIGN KEY trước khi xóa bảng
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += 'ALTER TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + '] DROP CONSTRAINT [' + CONSTRAINT_NAME + '];' + CHAR(13)
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE CONSTRAINT_TYPE = 'FOREIGN KEY';

EXEC sp_executesql @sql;

-- Xóa tất cả bảng
SET @sql = '';
SELECT @sql += 'DROP TABLE IF EXISTS [' + TABLE_SCHEMA + '].[' + TABLE_NAME + '];' + CHAR(13)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';

EXEC sp_executesql @sql;
GO

-- Bảng Role
CREATE TABLE Role (
    RoleID BIGINT PRIMARY KEY,
    RoleName NVARCHAR(50) UNIQUE NOT NULL
);


CREATE TABLE [Branch] (
    BranchID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);


CREATE TABLE [User] (
    UserID BIGINT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    [Password] NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20),
    fcmToken NVARCHAR(255) NULL,
    Status NVARCHAR(50) CHECK (Status IN ('Active', 'Inactive')) DEFAULT 'Active',
    RoleID BIGINT NOT NULL,
    BranchID BIGINT NOT NULL,
    RefreshToken NVARCHAR(MAX) NULL,
    RefreshTokenExpiryTime DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (RoleID) REFERENCES Role(RoleID),
    FOREIGN KEY (BranchID) REFERENCES Branch(BranchID)
);


CREATE TABLE [Warehouse] (
    WarehouseID BIGINT IDENTITY(1,1) PRIMARY KEY,
    BranchID BIGINT UNIQUE  NULL, -- UNIQUE đảm bảo 1-1
    Code NVARCHAR(50) UNIQUE NOT NULL, 
    Name NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (BranchID) REFERENCES Branch(BranchID)
);

CREATE TABLE [Category] (
    CategoryID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE [Unit] (
    UnitID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);




-- ==============================
-- PRODUCT (SẢN PHẨM GỐC)
-- ==============================
CREATE TABLE [Product] (
    ProductID BIGINT IDENTITY(1,1) PRIMARY KEY,
    ProductCode NVARCHAR(100) NOT NULL UNIQUE,   -- Mã sản phẩm gốc
    ProductName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    CategoryID BIGINT NOT NULL,
    UnitID BIGINT NOT NULL,                      -- Đơn vị cơ sở (VD: cái, hộp)
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Product_Category FOREIGN KEY (CategoryID) REFERENCES [Category](CategoryID),
    CONSTRAINT FK_Product_Unit FOREIGN KEY (UnitID) REFERENCES [Unit](UnitID)
);

-- ==============================
-- PRODUCT VARIANT (BIẾN THỂ SẢN PHẨM)
-- ==============================
CREATE TABLE [ProductVariant] (
    VariantID BIGINT IDENTITY(1,1) PRIMARY KEY,
    ProductID BIGINT NOT NULL,
    VariantCode NVARCHAR(100) NOT NULL UNIQUE,  -- Mã SKU cụ thể của biến thể
    Color NVARCHAR(100) NULL,
    Size NVARCHAR(100) NULL,
    CostPrice DECIMAL(18,4) NOT NULL DEFAULT(0),
    SalePrice DECIMAL(18,4) NOT NULL DEFAULT(0),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Variant_Product FOREIGN KEY (ProductID) REFERENCES [Product](ProductID)
);

CREATE TABLE [Chip] (
    ChipID BIGINT IDENTITY(1,1) PRIMARY KEY,
    ChipCode NVARCHAR(100) NOT NULL UNIQUE,
);

-- ==============================
-- PRODUCT VARIANT TAG (QUAN HỆ N-N GIỮA VARIANT VÀ TAG)
-- ==============================
CREATE TABLE [VariantChip] (
    VariantID BIGINT NOT NULL,
    ChipID BIGINT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY (VariantID, ChipID),
    CONSTRAINT FK_PVT_Variant FOREIGN KEY (VariantID) REFERENCES [ProductVariant](VariantID),
    CONSTRAINT FK_PVT_Tag FOREIGN KEY (ChipID) REFERENCES [Chip](ChipID)
);

CREATE TABLE [Inventory] (
    InventoryID BIGINT IDENTITY(1,1) PRIMARY KEY,
    WarehouseID BIGINT NOT NULL,          -- Kho hàng
    ProductID BIGINT NOT NULL,            -- Sản phẩm
    Quantity DECIMAL(18,2) NOT NULL DEFAULT 0,  -- Số lượng tồn kho
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (WarehouseID) REFERENCES [Warehouse](WarehouseID),
    FOREIGN KEY (ProductID) REFERENCES [Product](ProductID),
    CONSTRAINT UQ_Inventory UNIQUE (WarehouseID, ProductID) -- 1 biến thể sản phẩm chỉ có 1 dòng tồn trong mỗi kho
);

CREATE TABLE [InventoryHistory] (
    TransactionID BIGINT IDENTITY(1,1) PRIMARY KEY,
    InventoryID BIGINT,
    TransactionType NVARCHAR(50) NOT NULL CHECK (TransactionType IN ('Import', 'Sale', 'Transfer')),
    Quantity DECIMAL(18,2) NOT NULL,          -- Số lượng thay đổi (+/-)
    UnitCost DECIMAL(18,2) NULL,              -- Giá đơn vị (cho nhập hàng)
    ReferenceCode NVARCHAR(100) NULL,         -- Mã chứng từ (VD: PO123, SO456, ADJ001)
    Note NVARCHAR(255) NULL,
    CreatedBy BIGINT NULL,                    -- Người thực hiện (UserID)
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (InventoryID) REFERENCES [Inventory](InventoryID),
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID)
);

CREATE TABLE [Supplier] (
    SupplierID BIGINT IDENTITY(1,1) PRIMARY KEY,
    SupplierCode NVARCHAR(100) NOT NULL UNIQUE,
    SupplierName NVARCHAR(255) NOT NULL,
    ContactName NVARCHAR(100),
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(255),
    TaxCode NVARCHAR(50),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE [StockIn] (
    StockInID BIGINT IDENTITY(1,1) PRIMARY KEY,
    SupplierID BIGINT NULL,                       
    StockInCode NVARCHAR(100) NOT NULL UNIQUE,    
    StockInDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,4) NOT NULL DEFAULT 0,     
    Note NVARCHAR(255) NULL,
    Status NVARCHAR(50) 
        CHECK (Status IN ('Draft', 'PartialReceived', 'Received', 'Cancelled')) DEFAULT 'Draft',
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (SupplierID) REFERENCES [Supplier](SupplierID)
);

CREATE TABLE [StockInDetail] (
    StockInDetailID BIGINT IDENTITY(1,1) PRIMARY KEY,
    StockInID BIGINT NOT NULL,
    ProductID BIGINT NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    ReceivedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
    UnitCost DECIMAL(18,4) NOT NULL,
    ExtraFee DECIMAL(18,4) NOT NULL DEFAULT 0,        
    DiscountAmount DECIMAL(18,4) NOT NULL DEFAULT 0,  
    Subtotal AS ((UnitCost - DiscountAmount) * ReceivedQuantity) PERSISTED,  -- Công nợ NCC
    FOREIGN KEY (StockInID) REFERENCES [StockIn](StockInID),
    FOREIGN KEY (ProductID) REFERENCES [Product](ProductID)
);

CREATE TABLE [StockInDistribution] (
    DistributionID BIGINT IDENTITY(1,1) PRIMARY KEY,
    StockInDetailID BIGINT NOT NULL,
    WarehouseID BIGINT NOT NULL,
    ProductID BIGINT NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,               
    ReceivedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0, 
    UnitCost DECIMAL(18,4) NOT NULL,               -- Giá nhập gốc
    ExtraFee DECIMAL(18,4) NOT NULL DEFAULT 0,     -- Phụ phí phân bổ riêng từng kho
    BranchDebt AS ((UnitCost + ExtraFee) * ReceivedQuantity) PERSISTED, -- Tiền chi nhánh nợ
    Status NVARCHAR(50) 
        CHECK (Status IN ('Draft', 'PartialReceived', 'Received', 'Cancelled')) DEFAULT 'Draft',
    FOREIGN KEY (StockInDetailID) REFERENCES [StockInDetail](StockInDetailID),
    FOREIGN KEY (WarehouseID) REFERENCES [Warehouse](WarehouseID),
    FOREIGN KEY (ProductID) REFERENCES [Product](ProductID),
    CONSTRAINT UQ_StockInDistribution UNIQUE (StockInDetailID, WarehouseID, ProductID)
);


CREATE TABLE [SupplierDebt] (
    SupplierDebtID BIGINT PRIMARY KEY,
    SupplierID BIGINT,
    Debt DECIMAL(18,4) NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (SupplierID) REFERENCES [Supplier](SupplierID)
);

CREATE TABLE [WarehouseDebt] (
    WarehouseDebtID BIGINT PRIMARY KEY,
    WarehouseID BIGINT,
    Debt DECIMAL(18,4) NOT NULL DEFAULT 0,  
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (WarehouseID) REFERENCES [Warehouse](WarehouseID)
);

-- ==============================
-- PHIẾU TRẢ HÀNG CHO NCC
-- ==============================
CREATE TABLE [NGProduct] (
    NGProductID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(100) NOT NULL UNIQUE,
    SupplierID BIGINT NOT NULL,
    ReturnDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,4) NOT NULL DEFAULT 0,
    Note NVARCHAR(255),
    Status NVARCHAR(50) 
        CHECK (Status IN ('Draft','Completed','Cancelled')) DEFAULT 'Draft',
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (SupplierID) REFERENCES [Supplier](SupplierID)
);

-- ==============================
-- CHI TIẾT TRẢ HÀNG CHO NCC
-- ==============================
CREATE TABLE [NGProductDetail] (
    NGProductDetailID BIGINT IDENTITY(1,1) PRIMARY KEY,
    ReturnToSupplierID BIGINT NOT NULL,
    ProductID BIGINT NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    UnitCost DECIMAL(18,4) NOT NULL,
    Subtotal AS (UnitCost * Quantity) PERSISTED,
    FOREIGN KEY (ReturnToSupplierID) REFERENCES [NGProduct](NGProductID),
    FOREIGN KEY (ProductID) REFERENCES [Product](ProductID)
);


-- ==============================
-- Chuyển kho
-- ==============================
CREATE TABLE [TransferProduct] (
    TransferProductID BIGINT IDENTITY(1,1) PRIMARY KEY,
    ReturnCode NVARCHAR(100) NOT NULL UNIQUE,
    Type NVARCHAR(50) NOT NULL CHECK (Type IN ('NGProduct', 'Transfer')),
    FromWarehouseID BIGINT NOT NULL,    -- Shop/kho trả
    ToWarehouseID BIGINT NULL,      -- Kho nhận
    ReturnDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,4) NOT NULL DEFAULT 0,
    Note NVARCHAR(255),
    Status NVARCHAR(50)
        CHECK (Status IN ('Draft','Completed','Cancelled')) DEFAULT 'Draft',
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (FromWarehouseID) REFERENCES [Warehouse](WarehouseID),
    FOREIGN KEY (ToWarehouseID) REFERENCES [Warehouse](WarehouseID)
);

CREATE TABLE [TransferProductDetail] (
    TransferProductDetailID BIGINT IDENTITY(1,1) PRIMARY KEY,
    ProductID BIGINT NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    UnitCost DECIMAL(18,4) NOT NULL,
    Subtotal AS (UnitCost * Quantity) PERSISTED,
    FOREIGN KEY (ProductID) REFERENCES [Product](ProductID)
);
