using Chibest.Repository.Interface;

namespace Chibest.Repository;

public interface IUnitOfWork
{
    Task SaveChangesAsync();

    IAccountRepository AccountRepository { get; }
    IAccountRoleRepository AccountRoleRepository { get; }
    IRoleRepository RoleRepository { get; }
    IPermissionRepository PermissionRepository { get; }
    IBranchRepository BranchRepository { get; }
    IBranchStockRepository BranchStockRepository { get; }
    IProductRepository ProductRepository { get; }
    IProductDetailRepository ProductDetailRepository { get; }
    IProductPriceHistoryRepository ProductPriceHistoryRepository { get; }
    ICategoryRepository CategoryRepository { get; }
    IColorRepository ColorRepository { get; }
    ISizeRepository SizeRepository { get; }
    IPurchaseOrderRepository PurchaseOrderRepository { get; }
    ITransferOrderRepository TransferOrderRepository { get; }
    IPurchaseReturnRepository PurchaseReturnRepository { get; }
    IStockAdjusmentRepository StockAdjusmentRepository { get; }
    IProductPlanRepository ProductPlanRepository { get; }
    ISupplierDebtRepository SupplierDebtRepository { get; }
    IBranchDebtRepository BranchDebtRepository { get; }
    IBranchDebtHistoryRepository BranchDebtHistoryRepository { get; }
    IPurchaseOrderDetailRepository PurchaseOrderDetailRepository { get; }
    IPurchaseReturnDetailRepository PurchaseReturnDetailRepository { get; }
    IStockAdjustmentDetailRepository StockAdjustmentDetailRepository { get; }
    ISupplierDebtHistoryRepository SupplierDebtHistoryRepository { get; }
    ITransferOrderDetailRepository TransferOrderDetailRepository { get; }
}