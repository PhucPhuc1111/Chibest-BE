-- Chạy riêng lệnh này:
DROP DATABASE IF EXISTS "ChiBestDB";

-- Sau đó chạy riêng:
CREATE DATABASE "ChiBestDB";

-- =============================================
-- MODULE 1: ORGANIZATION & LOCATION
-- Quản lý chi nhánh và kho
-- =============================================

CREATE TABLE "Branch" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "Code" VARCHAR(50) UNIQUE NOT NULL,
    "Name" VARCHAR(255) NOT NULL,
    "Address" VARCHAR(500) NOT NULL,
    "PhoneNumber" VARCHAR(15),
    "Status" VARCHAR(40) NOT NULL DEFAULT 'Active', -- 'InActive', 'Deleted'
    "IsFranchise" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);


-- =============================================
-- MODULE 2: ACCOUNT & ROLE MANAGEMENT
-- Quản lý tài khoản, phân quyền
-- =============================================

CREATE TABLE "Account" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "Code" VARCHAR(100) UNIQUE NOT NULL,
    "Email" VARCHAR(100) UNIQUE NOT NULL,
    "Password" TEXT NOT NULL,
    "Name" VARCHAR(250) NOT NULL,
    "PhoneNumber" VARCHAR(15),
    "AvatarURL" TEXT,
    "FcmToken" VARCHAR(255),
    "RefreshToken" TEXT,
    "RefreshTokenExpiryTime" TIMESTAMP(3),
    "Status" VARCHAR(40) NOT NULL DEFAULT 'Active', -- 'InActive', 'Deleted'
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IX_Account_PhoneNumber ON "Account"("PhoneNumber");
CREATE INDEX IX_Account_Email ON "Account"("Email");

CREATE TABLE "Role" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL
);

CREATE INDEX IX_Role_Name ON "Role"("Name");

CREATE TABLE "AccountRole" (
    "AccountId" UUID NOT NULL,
    "RoleId" UUID NOT NULL,
    "BranchId" UUID NULL,

    CONSTRAINT PK_AccountRole PRIMARY KEY ("AccountId", "RoleId", "StartDate"),

    CONSTRAINT FK_AccountRole_Account FOREIGN KEY ("AccountId")
        REFERENCES "Account"("Id")
        ON DELETE CASCADE,

    CONSTRAINT FK_AccountRole_Role FOREIGN KEY ("RoleId")
        REFERENCES "Role"("Id")
        ON DELETE CASCADE,

    CONSTRAINT FK_AccountRole_Branch FOREIGN KEY ("BranchId")
        REFERENCES "Branch"("Id")
        ON DELETE CASCADE
);

CREATE INDEX IX_AccountRole_AccountId ON "AccountRole"("AccountId");
CREATE INDEX IX_AccountRole_BranchId_RoleId ON "AccountRole"("BranchId", "RoleId") INCLUDE ("AccountId");

-- Tạo bảng Permission
CREATE TABLE "Permission" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "Code" VARCHAR(100) UNIQUE NOT NULL,
);

CREATE INDEX IX_Permission_Code ON "Permission"("Code");
CREATE INDEX IX_Permission_Module ON "Permission"("Module");

-- Tạo bảng RolePermission
CREATE TABLE "RolePermission" (
    "RoleId" UUID NOT NULL,
    "PermissionId" UUID NOT NULL,
    
    CONSTRAINT PK_RolePermission PRIMARY KEY ("RoleId", "PermissionId"),
    
    CONSTRAINT FK_RolePermission_Role FOREIGN KEY ("RoleId")
        REFERENCES "Role"("Id")
        ON DELETE CASCADE,
        
    CONSTRAINT FK_RolePermission_Permission FOREIGN KEY ("PermissionId")
        REFERENCES "Permission"("Id")
        ON DELETE CASCADE
);



-- =============================================
-- MODULE 3: CUSTOMER MANAGEMENT
-- Quản lý khách hàng
-- =============================================

CREATE TABLE "Customer" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "Code" VARCHAR(20) UNIQUE NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Address" TEXT,
    "PhoneNumber" VARCHAR(15),
    "Email" VARCHAR(100),
    "DateOfBirth" TIMESTAMP(3),
    "AvatarURL" TEXT,
    "Status" VARCHAR(30) NOT NULL DEFAULT 'Working',
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastActive" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    "GroupId" UUID REFERENCES "Customer"("Id")
);

CREATE INDEX IX_Customer_PhoneNumber ON "Customer"("PhoneNumber");
CREATE INDEX IX_Customer_Email ON "Customer"("Email");
CREATE INDEX IX_Customer_GroupId ON "Customer"("GroupId");

-- =============================================
-- MODULE 4: PRODUCT CATALOG
-- Quản lý danh mục và sản phẩm
-- =============================================

CREATE TABLE "Category" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "Name" VARCHAR(150) NOT NULL,
);

CREATE INDEX IX_Category_Type_Name ON "Category"("Name");


CREATE TABLE "Color" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "Code" VARCHAR(10) NOT NULL
);

-- Tạo bảng Size
CREATE TABLE "Size" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "Code" VARCHAR(20) NOT NULL
);

-- Sửa đổi bảng Product
CREATE TABLE "Product" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "SKU" VARCHAR(50) UNIQUE NOT NULL,
    "Name" VARCHAR(250) NOT NULL,
    "Description" TEXT,
    "AvatarURL" TEXT,
    "VideoURL" TEXT,
    "ColorId" UUID NULL REFERENCES "Color"("Id") ON DELETE SET NULL,
    "SizeId" UUID NULL REFERENCES "Size"("Id") ON DELETE SET NULL,
    "Style" VARCHAR(100),
    "Material" VARCHAR(100),
    "Weight" INT NOT NULL DEFAULT 0,
    "IsMaster" BOOLEAN NOT NULL DEFAULT TRUE,
    "BarCode" VARCHAR(100) UNIQUE,
    "Status" VARCHAR(40) NOT NULL DEFAULT 'UnAvailable', -- Available
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Note" VARCHAR(200),
    "CategoryId" UUID NOT NULL REFERENCES "Category"("Id") ON DELETE CASCADE,
    "ParentSKU" VARCHAR(50) NULL REFERENCES "Product"("SKU")
);
CREATE INDEX IX_Product_Name ON "Product"("Name");
CREATE INDEX IX_Product_CategoryId ON "Product"("CategoryId");
CREATE INDEX IX_Product_ParentSKU ON "Product"("ParentSKU");
CREATE INDEX IX_Product_ColorId ON "Product"("ColorId");
CREATE INDEX IX_Product_SizeId ON "Product"("SizeId");

CREATE TABLE "ProductPlan" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "ProductId" UUID NOT NULL REFERENCES "Product"("Id") ON DELETE CASCADE,
	"SupplierId" UUID NOT NULL REFERENCES "Account"("Id") ON DELETE CASCADE,
    "Type" VARCHAR(100),
    "SendDate" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
	"DetailAmount" VARCHAR(100),
	"Amount" INT,
    "Status" VARCHAR(40) NOT NULL DEFAULT 'Queue', -- Processing / Delivery / Cancel
	"Note" VARCHAR(100),
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
);
CREATE INDEX IX_ProductPlan_Product_Supplier ON "Product"("SupplierId","ProductId","SendDate" DESC);

-- =============================================
-- MODULE 5: PRICING MANAGEMENT
-- Quản lý giá theo thời điểm và địa điểm
-- =============================================

-- Lịch sử giá bán theo chi nhánh
CREATE TABLE "ProductPriceHistory" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "SellingPrice" MONEY NOT NULL,
    "CostPrice" MONEY NOT NULL,
    "EffectiveDate" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ExpiryDate" TIMESTAMP(3),
    "Note" TEXT,
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ProductId" UUID NOT NULL REFERENCES "Product"("Id") ON DELETE CASCADE,
    "BranchId" UUID REFERENCES "Branch"("Id") ON DELETE CASCADE -- NULL = giá chung cho tất cả chi nhánh
);

CREATE INDEX IX_ProductPriceHistory_Product_Branch ON "ProductPriceHistory"("ProductId", "BranchId", "EffectiveDate" DESC);

-- =============================================
-- MODULE 6: INVENTORY MANAGEMENT
-- Quản lý tồn kho theo chi nhánh/kho
-- =============================================

CREATE TABLE "BranchStock" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "ProductId" UUID NOT NULL REFERENCES "Product"("Id") ON DELETE CASCADE,
    "BranchId" UUID NOT NULL REFERENCES "Branch"("Id") ON DELETE CASCADE,
    
    -- Số lượng
    "AvailableQty" INT NOT NULL DEFAULT 0,
    
    -- Ngưỡng tồn kho
    "MinimumStock" INT NOT NULL DEFAULT 0,
    "MaximumStock" INT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_BranchStock_Product_Branch UNIQUE ("ProductId", "BranchId")
);

CREATE INDEX IX_BranchStock_BranchId ON "BranchStock"("BranchId", "AvailableQty");
CREATE INDEX IX_BranchStock_ProductId ON "BranchStock"("ProductId");

CREATE TABLE "ProductDetail" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "ChipCode" VARCHAR(100) UNIQUE,
    "TagId" VARCHAR(100) UNIQUE,
    "ProductId" UUID NOT NULL REFERENCES "Product"("Id") ON DELETE CASCADE,
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IX_ProductDetail_ChipCode ON "ProductDetail"("ChipCode");
CREATE INDEX IX_ProductDetail_ProductId ON "ProductDetail"("ProductId");


-- =============================================
-- MODULE 8: PURCHASE ORDERS
-- Quản lý đơn nhập hàng từ NCC
-- =============================================

CREATE TABLE "PurchaseOrder" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "InvoiceCode" VARCHAR(100) NOT NULL UNIQUE,              -- NYC-CUST105-INV78 (location + client + sequence)                                
    "OrderDate" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- Thời gian giao hàng
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
	"UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- Thông tin thanh toán
    "SubTotal" MONEY NOT NULL DEFAULT 0,
    "Note" TEXT,
    "Status" VARCHAR(40) NOT NULL DEFAULT 'Draft', -- 'Submit' / 'Cancel' / 'Done'
    "BranchId" UUID REFERENCES "Branch"("Id") ON DELETE CASCADE,                       
    "EmployeeId" UUID REFERENCES "Account"("Id"),                           -- Nhân viên xác nhận giao dịch
    "SupplierId" UUID REFERENCES "Account"("Id") ON DELETE CASCADE                           
);

CREATE INDEX IX_PurchaseOrder_InvoiceCode ON "PurchaseOrder_Branch"("BranchId");
CREATE INDEX IX_TransactionOrder_Status ON "PurchaseOrder"("Status");
CREATE INDEX IX_TransactionOrder_OrderDate ON "PurchaseOrder"("OrderDate" DESC);

-- =====================================================================
-- Chi Tiết Đơn Nhập
CREATE TABLE "PurchaseOrderDetail" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "Quantity" INT NOT NULL,
    "ActualQuantity" INT,
    "ReFee" money not null,
    "UnitPrice" MONEY NOT NULL,
    "Note" TEXT,
    "PurchaseOrderId" UUID NOT NULL REFERENCES "PurchaseOrder"("Id") ON DELETE CASCADE,
    "ProductId" UUID NOT NULL REFERENCES "Product"("Id") ON DELETE CASCADE
);

CREATE INDEX IX_TransactionOrderDetail_OrderId ON "PurchaseOrderDetail"("PurchaseOrderId");
CREATE INDEX IX_TransactionOrderDetail_ProductId ON "PurchaseOrderDetail"("ProductId");

-- =============================================
-- MODULE 9: TRANSFER ORDERS
-- Quản lý chuyển kho và Trả hàng lỗi
-- =============================================
CREATE TABLE "TransferOrder" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "InvoiceCode" VARCHAR(100) NOT NULL UNIQUE,              -- NYC-CUST105-INV78 (location + client + sequence)                                
    "OrderDate" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- Thời gian giao hàng
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
	"UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- Thông tin thanh toán
    "SubTotal" MONEY NOT NULL DEFAULT 0,
    "Note" TEXT,
    "Status" VARCHAR(40) NOT NULL DEFAULT 'Draft', -- 'Submit' / 'Cancel' / 'Done'
    "EmployeeId" UUID REFERENCES "Account"("Id"),                           -- Nhân viên xác nhận giao dịch
    "FromBranch" UUID REFERENCES "Branch"("Id"),                    
    "ToBranch" UUID REFERENCES "Branch"("Id")                      
);

CREATE INDEX IX_TransferOrder_Branch ON "TransferOrder"("FromBranch", "ToBranch", "OrderDate" DESC);
CREATE INDEX IX_TransferOrder_OrderDate ON "TransferOrder"("OrderDate" DESC);

-- =====================================================================
-- Chi Tiết Đơn  chuyển
CREATE TABLE "TransferOrderDetail" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "Quantity" INT NOT NULL,
    "ActualQuantity" INT,
    "CommissionFee" money not null,
    "UnitPrice" MONEY NOT NULL,
    "Note" TEXT,
    "TransferOrderId" UUID NOT NULL REFERENCES "TransferOrder"("Id") ON DELETE CASCADE,
    "ProductId" UUID NOT NULL REFERENCES "Product"("Id") ON DELETE CASCADE
);

CREATE INDEX IX_TransferOrderDetail_OrderId ON "TransferOrderDetail"("TransferOrderId");
CREATE INDEX IX_TransferOrderDetail_ProductId ON "TransferOrderDetail"("ProductId");

-- Phiếu đơn lỗi
CREATE TABLE "PurchaseReturn" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "InvoiceCode" VARCHAR(100) NOT NULL UNIQUE,             
    "OrderDate" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "SubTotal" MONEY NOT NULL DEFAULT 0,
    "Note" TEXT,
    "Status" VARCHAR(40) NOT NULL DEFAULT 'Draft', -- 'Submit' / 'Cancel' / 'Done'
    "EmployeeId" UUID NULL,
    "BranchId" UUID NULL,
    "SupplierId" UUID NULL,
    CONSTRAINT FK_PurchaseReturn_Employee FOREIGN KEY ("EmployeeId") REFERENCES "Account"("Id"),
    CONSTRAINT FK_PurchaseReturn_Branch FOREIGN KEY ("BranchId") REFERENCES "Branch"("Id"),
    CONSTRAINT FK_PurchaseReturn_Supplier FOREIGN KEY ("SupplierId") REFERENCES "Account"("Id")
);
CREATE INDEX IX_PurchaseReturn_Branch ON "PurchaseReturn"("BranchId");
CREATE INDEX IX_PurchaseReturn_OrderDate ON "PurchaseReturn"("OrderDate" DESC);
-- Chi Tiết Đơn lỗi
CREATE TABLE "PurchaseReturnDetail" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "Quantity" INT NOT NULL,
    "UnitPrice" MONEY NOT NULL, -- Thường là giá vốn
    "Note" TEXT,
    "PurchaseReturnId" UUID NOT NULL REFERENCES "PurchaseReturn"("Id") ON DELETE CASCADE,
    "ProductId" UUID NOT NULL REFERENCES "Product"("Id") ON DELETE CASCADE
);
CREATE INDEX IX_PurchaseReturnDetail_OrderId ON "PurchaseReturnDetail"("PurchaseReturnId");
CREATE INDEX IX_PurchaseReturnDetail_ProductId ON "PurchaseReturnDetail"("ProductId");

-- =============================================
-- MODULE 10: STOCK ADJUSTMENT
-- Điều chỉnh và kiểm kê kho 
-- =============================================

CREATE TABLE "StockAdjustment" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "AdjustmentCode" VARCHAR(100) UNIQUE NOT NULL,
    "AdjustmentDate" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "AdjustmentType" VARCHAR(50) NOT NULL, 
    
    "BranchId" UUID NOT NULL REFERENCES "Branch"("Id") ON DELETE CASCADE,
    "EmployeeId" UUID NOT NULL REFERENCES "Account"("Id"),
    
    "TotalValueChange" MONEY NOT NULL DEFAULT 0,
    "Status" VARCHAR(40) NOT NULL DEFAULT 'Draft', -- 'Submit' / 'Cancel' / 'Done'
    "Note" TEXT,
    
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ApprovedBy" UUID NULL REFERENCES "Account"("Id")
);

CREATE INDEX IX_StockAdjustment_Type_Date ON "StockAdjustment"("AdjustmentType", "AdjustmentDate" DESC);
CREATE INDEX IX_StockAdjustment_BranchId ON "StockAdjustment"("BranchId", "AdjustmentDate" DESC);



CREATE TABLE "StockAdjustmentDetail" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "StockAdjustmentId" UUID NOT NULL REFERENCES "StockAdjustment"("Id") ON DELETE CASCADE,
    "ProductId" UUID NOT NULL REFERENCES "Product"("Id") ,
    
    "SystemQty" INT NOT NULL, -- Số lượng trong hệ thống
    "ActualQty" INT NOT NULL, -- Số lượng thực tế
    "DifferenceQty" INT GENERATED ALWAYS AS ("ActualQty" - "SystemQty") STORED,
    
    "UnitCost" MONEY NOT NULL DEFAULT 0,
    "TotalValueChange" MONEY GENERATED ALWAYS AS (("ActualQty" - "SystemQty") * "UnitCost") STORED,
    "Note" TEXT
);

CREATE INDEX IX_StockAdjustmentDetail_AdjustmentId ON "StockAdjustmentDetail"("StockAdjustmentId");
CREATE INDEX IX_StockAdjustmentDetail_ProductId ON "StockAdjustmentDetail"("ProductId");


-- =============================================
-- MODULE 11: SALES ORDERS
-- Quản lý đơn bán hàng
-- =============================================

CREATE TABLE "Voucher" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "Code" VARCHAR(100) UNIQUE NOT NULL,
    "Name" VARCHAR(250) NOT NULL,
    "Description" TEXT,
    "VoucherType" VARCHAR(50) NOT NULL DEFAULT 'Giảm Giá', -- Giảm Giá, Miễn Phí Ship, Quà Tặng
    
    "AvailableDate" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ExpiredDate" TIMESTAMP(3) NOT NULL,
    
    "MinimumTransaction" MONEY NOT NULL DEFAULT 0,
    "MaxDiscountAmount" MONEY NULL,
    "DiscountPercent" DECIMAL(5,2) DEFAULT 0,
    "DiscountAmount" MONEY DEFAULT 0,
    
    "UsageLimit" INT NULL,
    "UsagePerCustomer" INT DEFAULT 1,
    "UsedCount" INT NOT NULL DEFAULT 0,
    
    "ApplicableProducts" TEXT NULL, -- JSON array of product IDs
    "ApplicableCategories" TEXT NULL, -- JSON array of category IDs
    
    "Status" VARCHAR(40) NOT NULL DEFAULT 'Available',
    
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "CustomerVoucher" (
    "VoucherId" UUID NOT NULL REFERENCES "Voucher"("Id"),
    "CustomerId" UUID NOT NULL REFERENCES "Customer"("Id"),
    "CollectedDate" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UsedDate" TIMESTAMP(3) NULL,
    "Status" VARCHAR(40) NOT NULL DEFAULT 'Đã Nhận',
    
    PRIMARY KEY ("VoucherId", "CustomerId")
);

CREATE TABLE "SalesOrder" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "OrderCode" VARCHAR(100) UNIQUE NOT NULL,
    "OrderDate" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Thông tin khách hàng
    "CustomerId" UUID NOT NULL REFERENCES "Customer"("Id"),
    "CustomerName" VARCHAR(250) NOT NULL,
    "CustomerPhone" VARCHAR(15),
    "CustomerEmail" VARCHAR(100),
    
    -- Thông tin giao dịch
    "BranchId" UUID NOT NULL REFERENCES "Branch"("Id"),
    "EmployeeId" UUID NOT NULL REFERENCES "Account"("Id"),
    
    -- Thông tin giao hàng
    "DeliveryMethod" VARCHAR(50) NOT NULL DEFAULT 'Tại Cửa Hàng',
    "ShippingAddress" VARCHAR(500),
    "ShippingPhone" VARCHAR(15),
    "ExpectedDeliveryDate" TIMESTAMP(3) NULL,
    "ActualDeliveryDate" TIMESTAMP(3) NULL,
    
    -- Thông tin thanh toán
    "PaymentMethod" VARCHAR(50) NOT NULL DEFAULT 'Tiền Mặt',
    "PaymentStatus" VARCHAR(50) NOT NULL DEFAULT 'Chờ Thanh Toán',
    
    "SubTotal" MONEY NOT NULL DEFAULT 0,
    "DiscountAmount" MONEY NOT NULL DEFAULT 0,
    "VoucherId" UUID NULL REFERENCES "Voucher"("Id"),
    "VoucherAmount" MONEY NOT NULL DEFAULT 0,
    "ShippingFee" MONEY NOT NULL DEFAULT 0,
    "FinalAmount" MONEY GENERATED ALWAYS AS ("SubTotal" - "DiscountAmount" - "VoucherAmount" + "ShippingFee") STORED,
    "PaidAmount" MONEY NOT NULL DEFAULT 0,
    
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Đặt Trước',
    "Note" TEXT,
    
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IX_SalesOrder_CustomerId ON "SalesOrder"("CustomerId", "OrderDate" DESC);
CREATE INDEX IX_SalesOrder_BranchId ON "SalesOrder"("BranchId", "OrderDate" DESC);
CREATE INDEX IX_SalesOrder_Status ON "SalesOrder"("Status", "OrderDate" DESC);
CREATE INDEX IX_SalesOrder_PaymentStatus ON "SalesOrder"("PaymentStatus");

CREATE TABLE "SalesOrderDetail" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "SalesOrderId" UUID NOT NULL REFERENCES "SalesOrder"("Id"),
    "ProductId" UUID NOT NULL REFERENCES "Product"("Id"),
    "ProductSKU" VARCHAR(50) NOT NULL,
    "ProductDetailId" UUID NULL REFERENCES "ProductDetail"("Id"),
    
    "ItemName" VARCHAR(250) NOT NULL,
    "Quantity" INT NOT NULL,
    "UnitPrice" MONEY NOT NULL,
    "DiscountPercent" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "DiscountAmount" MONEY NOT NULL DEFAULT 0,
    "TotalPrice" MONEY GENERATED ALWAYS AS ("Quantity" * "UnitPrice" - "DiscountAmount" + ("Quantity" * "UnitPrice" / 100)) STORED,
    
    "Note" TEXT
);

CREATE INDEX IX_SalesOrderDetail_OrderId ON "SalesOrderDetail"("SalesOrderId");
CREATE INDEX IX_SalesOrderDetail_ProductId ON "SalesOrderDetail"("ProductId");

-- =============================================
-- MODULE 12: DEBT MANAGEMENT
-- Quản lý công nợ
-- =============================================

-- Công nợ nhà cung cấp
CREATE TABLE "SupplierDebt" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "SupplierId" UUID NOT NULL REFERENCES "Account"("Id") ON DELETE CASCADE,
    "TotalDebt" MONEY NOT NULL DEFAULT 0,
    "PaidAmount" MONEY NOT NULL DEFAULT 0,
    "ReturnAmount" MONEY NOT NULL DEFAULT 0, 
    "RemainingDebt" MONEY GENERATED ALWAYS AS ("TotalDebt" - "PaidAmount" - "ReturnAmount") STORED,
    CONSTRAINT UQ_SupplierDebt_Supplier UNIQUE ("SupplierId")
);

CREATE TABLE "SupplierDebtHistory" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "SupplierDebtId" UUID NOT NULL REFERENCES "SupplierDebt"("Id") ON DELETE CASCADE,
    "TransactionType" VARCHAR(50) NOT NULL, -- Nhập hàng, Thanh Toán, Điều Chỉnh
    "TransactionDate" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Amount" MONEY NOT NULL,    
    "Note" TEXT,
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IX_SupplierDebtHistory_Supplier ON "SupplierDebtHistory"("SupplierDebtId", "TransactionDate" DESC);

-- Công nợ chi nhánh (với trụ sở chính)
-- Bảng tổng nợ của chi nhánh
CREATE TABLE "BranchDebt" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "BranchId" UUID NOT NULL REFERENCES "Branch"("Id") ON DELETE CASCADE,

    "TotalDebt" MONEY NOT NULL DEFAULT 0,
    "PaidAmount" MONEY NOT NULL DEFAULT 0,
    "ReturnAmount" MONEY NOT NULL DEFAULT 0, 
    "RemainingDebt" MONEY GENERATED ALWAYS AS ("TotalDebt" - "PaidAmount" - "ReturnAmount") STORED,
    "LastTransactionDate" TIMESTAMP(3) NULL,
    "LastUpdated" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT UQ_BranchDebt_Branch UNIQUE ("BranchId")
);


-- Bảng lịch sử biến động nợ (nhiều record cho 1 BranchDebt)
CREATE TABLE "BranchDebtHistory" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    
    -- Thay vì tham chiếu trực tiếp BranchId, tham chiếu tới BranchDebt
    "BranchDebtId" UUID NOT NULL REFERENCES "BranchDebt"("Id") ON DELETE CASCADE,

    "TransactionType" VARCHAR(50) NOT NULL,
    "TransactionDate" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Amount" MONEY NOT NULL,
    "Note" TEXT,
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);


CREATE INDEX "IX_BranchDebtHistory_Branch" ON "BranchDebtHistory"("BranchDebtId", "TransactionDate" DESC);

-- =============================================
-- MODULE 14: EMPLOYEE & PAYROLL
-- Quản lý nhân viên và lương
-- =============================================

CREATE TABLE "WorkShift" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "StartTime" TIME NOT NULL,
    "EndTime" TIME NOT NULL,
    "IsOvernight" BOOLEAN NOT NULL DEFAULT FALSE,
    "ShiftCoefficient" DECIMAL(5,2) NOT NULL DEFAULT 1.0,
    "Description" TEXT
);

CREATE TABLE "SalaryConfig" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "EmployeeId" UUID NOT NULL REFERENCES "Account"("Id"),
    "BranchId" UUID NOT NULL REFERENCES "Branch"("Id"),
    
    "SalaryType" VARCHAR(50) NOT NULL DEFAULT 'Theo Tháng',
    "BaseSalary" MONEY NOT NULL,
    "HourlyRate" MONEY NULL,
    
    -- Phụ cấp
    "PositionAllowance" MONEY NOT NULL DEFAULT 0,
    "TransportAllowance" MONEY NOT NULL DEFAULT 0,
    "MealAllowance" MONEY NOT NULL DEFAULT 0,
    "PhoneAllowance" MONEY NOT NULL DEFAULT 0,
    "HousingAllowance" MONEY NOT NULL DEFAULT 0,
    
    -- Hệ số
    "OvertimeCoefficient" DECIMAL(5,2) NOT NULL DEFAULT 1.5,
    "HolidayCoefficient" DECIMAL(5,2) NOT NULL DEFAULT 2.0,
    "WeekendCoefficient" DECIMAL(5,2) NOT NULL DEFAULT 1.3,
    
    "EffectiveDate" DATE NOT NULL,
    "ExpiryDate" DATE NULL,
    "Status" VARCHAR(40) NOT NULL DEFAULT 'Đang Áp Dụng',
    
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IX_SalaryConfig_EmployeeId ON "SalaryConfig"("EmployeeId", "EffectiveDate" DESC);

CREATE TABLE "Attendance" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "EmployeeId" UUID NOT NULL REFERENCES "Account"("Id"),
    "BranchId" UUID NOT NULL REFERENCES "Branch"("Id"),
    "WorkShiftId" UUID NULL REFERENCES "WorkShift"("Id"),
    
    "WorkDate" DATE NOT NULL,
    "CheckInTime" TIMESTAMP(3) NULL,
    "CheckOutTime" TIMESTAMP(3) NULL,
    
    "WorkHours" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "OvertimeHours" DECIMAL(5,2) NOT NULL DEFAULT 0,
    
    "DayType" VARCHAR(50) NOT NULL DEFAULT 'Ngày Thường',
    "AttendanceStatus" VARCHAR(50) NOT NULL DEFAULT 'Có Mặt',
    
    "Note" TEXT,
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IX_Attendance_EmployeeId ON "Attendance"("EmployeeId", "WorkDate" DESC);
CREATE INDEX IX_Attendance_BranchId ON "Attendance"("BranchId", "WorkDate" DESC);

-- Tien hoa hong
CREATE TABLE "Commission" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "EmployeeId" UUID NOT NULL REFERENCES "Account"("Id"),
    
    "CommissionType" VARCHAR(50) NOT NULL, -- Doanh Số, KPI, Thưởng Đặc Biệt
    "Amount" MONEY NOT NULL,
    "CalculationBase" MONEY NULL,
    "CommissionRate" DECIMAL(5,2) NULL,
    
    "ReferenceType" VARCHAR(50) NULL,
    "ReferenceId" UUID NULL,
    
    "PeriodMonth" INT NOT NULL,
    "PeriodYear" INT NOT NULL,
    
    "Note" TEXT,
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IX_Commission_EmployeeId ON "Commission"("EmployeeId", "PeriodYear" DESC, "PeriodMonth" DESC);

CREATE TABLE "Deduction" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "EmployeeId" UUID NOT NULL REFERENCES "Account"("Id"),
    
    "DeductionType" VARCHAR(50) NOT NULL, -- Đi Trễ, Nghỉ Không Phép, Vi Phạm, Khác
    "Amount" MONEY NOT NULL,
    
    "PeriodMonth" INT NOT NULL,
    "PeriodYear" INT NOT NULL,
    
    "Description" TEXT,
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IX_Deduction_EmployeeId ON "Deduction"("EmployeeId", "PeriodYear" DESC, "PeriodMonth" DESC);

CREATE TABLE "Payroll" (
    "Id" UUID DEFAULT GEN_RANDOM_UUID() PRIMARY KEY,
    "EmployeeId" UUID NOT NULL REFERENCES "Account"("Id"),
    "BranchId" UUID NOT NULL REFERENCES "Branch"("Id"),
    
    "PeriodMonth" INT NOT NULL,
    "PeriodYear" INT NOT NULL,
    
    -- Công và giờ làm
    "TotalWorkDays" INT NOT NULL DEFAULT 0,
    "TotalWorkHours" DECIMAL(10,2) NOT NULL DEFAULT 0,
    "TotalOvertimeHours" DECIMAL(10,2) NOT NULL DEFAULT 0,
    "StandardWorkDays" INT NOT NULL DEFAULT 26,
    
    -- Lương cơ bản
    "BaseSalary" MONEY NOT NULL DEFAULT 0,
    "ActualBaseSalary" MONEY NOT NULL DEFAULT 0,
    
    -- Phụ cấp và tăng ca
    "TotalAllowance" MONEY NOT NULL DEFAULT 0,
    "OvertimeSalary" MONEY NOT NULL DEFAULT 0,
    
    -- Thưởng và hoa hồng
    "TotalCommission" MONEY NOT NULL DEFAULT 0,
    "TotalBonus" MONEY NOT NULL DEFAULT 0,
    
    -- Khấu trừ
    "TotalDeduction" MONEY NOT NULL DEFAULT 0,
    
    -- Bảo hiểm
    "SocialInsurance" MONEY NOT NULL DEFAULT 0,
    "HealthInsurance" MONEY NOT NULL DEFAULT 0,
    "UnemploymentInsurance" MONEY NOT NULL DEFAULT 0,
    
    
    -- Tổng
    "GrossSalary" MONEY GENERATED ALWAYS AS ("ActualBaseSalary" + "TotalAllowance" + "OvertimeSalary" + "TotalCommission" + "TotalBonus") STORED,
    "NetSalary" MONEY GENERATED ALWAYS AS ("ActualBaseSalary" + "TotalAllowance" + "OvertimeSalary" + "TotalCommission" + "TotalBonus" 
                  - "TotalDeduction" - "SocialInsurance" - "HealthInsurance" - "UnemploymentInsurance") STORED,
    
    "PaymentDate" DATE NULL,
    "PaymentMethod" VARCHAR(50) DEFAULT 'Chuyển Khoản',
    "PaymentStatus" VARCHAR(50) NOT NULL DEFAULT 'Chờ Thanh Toán',
    
    "Note" TEXT,
    "CreatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT UQ_Payroll_Employee_Period UNIQUE("EmployeeId", "PeriodYear", "PeriodMonth")
);

CREATE INDEX IX_Payroll_EmployeeId ON "Payroll"("EmployeeId", "PeriodYear" DESC, "PeriodMonth" DESC);
CREATE INDEX IX_Payroll_Status ON "Payroll"("PaymentStatus", "PeriodYear" DESC, "PeriodMonth" DESC);


