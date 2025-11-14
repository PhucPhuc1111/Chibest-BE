using Chibest.Repository.Interface;

namespace Chibest.Repository;

public interface IUnitOfWork
{
    Task SaveChangesAsync();

    IWarehouseRepository WarehouseRepository { get; }
    IAccountRepository AccountRepository { get; }
    IAccountRoleRepository AccountRoleRepository { get; }
    IRoleRepository RoleRepository { get; }
    IBranchRepository BranchRepository { get; }
    IBranchStockRepository BranchStockRepository { get; }
    IProductRepository ProductRepository { get; }
    IProductDetailRepository ProductDetailRepository { get; }
    IProductPriceHistoryRepository ProductPriceHistoryRepository { get; }
    ICategoryRepository CategoryRepository { get; }
    IPurchaseOrderRepository PurchaseOrderRepository { get; }
    ITransferOrderRepository TransferOrderRepository { get; }
    IPurchaseReturnRepository PurchaseReturnRepository { get; }
    IStockAdjusmentRepository StockAdjusmentRepository { get; }
    ISupplierDebtRepository SupplierDebtRepository { get; }
    IBranchDebtRepository BranchDebtRepository { get; }
    IBranchDebtHistoryRepository BranchDebtHistoryRepository { get; }
    IPurchaseOrderDetailRepository PurchaseOrderDetailRepository { get; }
    IPurchaseReturnDetailRepository PurchaseReturnDetailRepository { get; }
    IStockAdjustmentDetailRepository StockAdjustmentDetailRepository { get; }
    ISupplierDebtHistoryRepository SupplierDebtHistoryRepository { get; }
    ITransferOrderDetailRepository TransferOrderDetailRepository { get; }
}