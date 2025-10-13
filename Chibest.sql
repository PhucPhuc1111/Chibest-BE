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

CREATE TABLE [Product] (
    ProductID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(100) NOT NULL UNIQUE,   -- Mã nội bộ / SKU
    Name NVARCHAR(255) NOT NULL,
    Barcode NVARCHAR(100) NULL,           
    Description NVARCHAR(MAX) NULL,
    UnitID BIGINT NOT NULL,           -- đơn vị lưu kho/đơn vị cơ sở
    CategoryID BIGINT NOT NULL,
    CostPrice DECIMAL(18,4) NOT NULL DEFAULT(0),  -- giá vốn
    SalePrice DECIMAL(18,4) NOT NULL DEFAULT(0),  -- giá bán mặc định
    CurrentStock DECIMAL(18,4) NOT NULL DEFAULT(0), -- tồn kho theo BaseUnit
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Product_Cate FOREIGN KEY (CategoryID) REFERENCES [Category](CategoryID),
    CONSTRAINT FK_Product_Unit FOREIGN KEY (UnitID) REFERENCES [Unit](UnitID)
);

CREATE TABLE [Tag] (
    TagID BIGINT IDENTITY(1,1) PRIMARY KEY,
    ProductID BIGINT NOT NULL,
    Code NVARCHAR(100) NOT NULL UNIQUE,   
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ProductID) REFERENCES [Product](ProductID)
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
    CONSTRAINT UQ_Inventory UNIQUE (WarehouseID, ProductID) -- 1 sản phẩm chỉ có 1 dòng tồn trong mỗi kho
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

CREATE TABLE [Stockin] (
    PurchaseOrderID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(50) UNIQUE NOT NULL,            -- Số chứng từ (VD: PO2025-0001)
    SupplierID BIGINT NOT NULL,
    WarehouseID BIGINT NOT NULL,
    CreatedBy BIGINT NOT NULL,                    -- Người tạo phiếu
    Status NVARCHAR(50) CHECK (Status IN ('Pending', 'Received', 'Completed', 'Cancelled')) DEFAULT 'Pending',
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0, -- Tổng tiền đơn hàng
    PaidAmount DECIMAL(18,2) NOT NULL DEFAULT 0,  -- Đã thanh toán
    Note NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (SupplierID) REFERENCES [Supplier](SupplierID),
    FOREIGN KEY (WarehouseID) REFERENCES [Warehouse](WarehouseID),
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID)
);
CREATE TABLE [StockinDetail] (
    PODetailID BIGINT IDENTITY(1,1) PRIMARY KEY,
    PurchaseOrderID BIGINT NOT NULL,
    ProductID BIGINT NOT NULL,
    OrderedQuantity DECIMAL(18,2) NOT NULL,        -- Số lượng trên phiếu
    ReceivedQuantity DECIMAL(18,2) NULL,           -- Số lượng thực tế
    UnitCost DECIMAL(18,2) NOT NULL,
    TotalCost AS (ISNULL(ReceivedQuantity, OrderedQuantity) * UnitCost) PERSISTED,
    FOREIGN KEY (PurchaseOrderID) REFERENCES [PurchaseOrder](PurchaseOrderID),
    FOREIGN KEY (ProductID) REFERENCES [Product](ProductID)
);


CREATE TABLE [PurchaseAdjustmentRequest] (
    AdjustmentID BIGINT IDENTITY(1,1) PRIMARY KEY,
    PurchaseOrderID BIGINT NOT NULL,
    RequestedBy BIGINT NOT NULL,
    Status NVARCHAR(50) CHECK (Status IN ('Pending', 'Approved', 'Rejected')) DEFAULT 'Pending',
    Reason NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PurchaseOrderID) REFERENCES [PurchaseOrder](PurchaseOrderID),
    FOREIGN KEY (RequestedBy) REFERENCES [User](UserID)
);

CREATE TABLE [PurchaseAdjustmentDetail] (
    AdjustmentDetailID BIGINT IDENTITY(1,1) PRIMARY KEY,
    AdjustmentID BIGINT NOT NULL,
    ProductID BIGINT NOT NULL,
    OldQuantity DECIMAL(18,2) NOT NULL,     -- Số lượng gốc
    NewQuantity DECIMAL(18,2) NOT NULL,     -- Số lượng sau điều chỉnh
    Note NVARCHAR(255),
    FOREIGN KEY (AdjustmentID) REFERENCES [PurchaseAdjustmentRequest](AdjustmentID),
    FOREIGN KEY (ProductID) REFERENCES [Product](ProductID)
);


CREATE TABLE [SupplierTransaction] (
    TransactionID BIGINT IDENTITY(1,1) PRIMARY KEY,
    SupplierID BIGINT NOT NULL,
    PurchaseOrderID BIGINT NULL,
    TransactionType NVARCHAR(50) CHECK (TransactionType IN ('Purchase', 'Payment', 'Adjustment', 'Return')) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Note NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CreatedBy BIGINT NULL,
    FOREIGN KEY (SupplierID) REFERENCES [Supplier](SupplierID),
    FOREIGN KEY (PurchaseOrderID) REFERENCES [PurchaseOrder](PurchaseOrderID),
    FOREIGN KEY (CreatedBy) REFERENCES [User](UserID)
);

CREATE TABLE [SupplierBalance] (
    SupplierID BIGINT PRIMARY KEY,
    TotalPurchase DECIMAL(18,2) DEFAULT 0,
    TotalPayment DECIMAL(18,2) DEFAULT 0,
    Balance AS (TotalPurchase - TotalPayment) PERSISTED,
    FOREIGN KEY (SupplierID) REFERENCES [Supplier](SupplierID)
);

CREATE TABLE [StockTransfer]
