using Chibest.Repository.Interface;

namespace Chibest.Repository;

public interface IUnitOfWork
{
    Task BeginTransaction();
    Task CommitTransaction();
    Task RollbackTransaction();
    Task SaveChangesAsync();

    IWarehouseRepository WarehouseRepository { get; }
    IAccountRepository AccountRepository { get; }
    IAccountRoleRepository AccountRoleRepository { get; }
    IRoleRepository RoleRepository { get; }
    IBranchRepository BranchRepository { get; }
    IBranchStockRepository BranchStockRepository { get; }
    IProductRepository ProductRepository { get; }
    IProductDetailRepository ProductDetailRepository { get; }
    ISystemLogRepository SystemLogRepository { get; }
    ICategoryRepository CategoryRepository { get; }
}