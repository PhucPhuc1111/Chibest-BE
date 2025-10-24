using Chibest.Repository.Interface;

namespace Chibest.Repository;

public interface IUnitOfWork
{
    Task BeginTransaction();
    Task CommitTransaction();
    Task RollbackTransaction();
    Task SaveChangesAsync();
    Task BulkInsertAsync<T>(IList<T> entities) where T : class;
    Task BulkUpdateAsync<T>(IList<T> entities) where T : class;
    Task BulkDeleteAsync<T>(IList<T> entities) where T : class;

    IWarehouseRepository WarehouseRepository { get; }
    IAccountRepository AccountRepository { get; }
    IAccountRoleRepository AccountRoleRepository { get; }
    IRoleRepository RoleRepository { get; }
    IBranchRepository BranchRepository { get; }
    IBranchStockRepository BranchStockRepository { get; }
    IProductRepository ProductRepository { get; }
    IProductDetailRepository ProductDetailRepository { get; }
    IProductPriceHistoryRepository ProductPriceHistoryRepository { get; }
    ISystemLogRepository SystemLogRepository { get; }
    ICategoryRepository CategoryRepository { get; }
    IPurchaseOrderRepository PurchaseOrderRepository { get; }
    ITransferOrderRepository TransferOrderRepository { get; }
    IPurchaseReturnRepository PurchaseReturnRepository { get; }
    IStockAdjusmentRepository StockAdjusmentRepository { get; }
}