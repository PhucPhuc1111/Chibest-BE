using Chibest.Repository.Interface;
using Chibest.Repository.Models;
using Chibest.Repository.Repositories;
using EFCore.BulkExtensions;

namespace Chibest.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        protected readonly ChiBestDbContext _context;
        private IWarehouseRepository _warehouseRepository;
        private IAccountRepository _accountRepository;
        private IAccountRoleRepository _accountRoleRepository;
        private IRoleRepository _roleRepository;
        private IBranchRepository _branchRepository;
        private IBranchStockRepository _branchStockRepository;
        private IProductRepository _productRepository;
        private IProductDetailRepository _productDetailRepository;
        private ISystemLogRepository _systemLogRepository;
        private ICategoryRepository _categoryRepository;
        private IPurchaseOrderRepository _purchaseOrderRepository;
        private ITransferOrderRepository _transferOrderRepository;
        private IPurchaseReturnRepository _purchaseReturnRepository;

        public UnitOfWork(ChiBestDbContext context)
        {
            _context = context;
        }
        public IWarehouseRepository WarehouseRepository => _warehouseRepository ??= new WarehouseRepository(_context);
        public IAccountRepository AccountRepository => _accountRepository ??= new AccountRepository(_context);
        public IAccountRoleRepository AccountRoleRepository => _accountRoleRepository ??= new AccountRoleRepository(_context);
        public IRoleRepository RoleRepository => _roleRepository ??= new RoleRepository(_context);
        public IBranchRepository BranchRepository => _branchRepository ??= new BranchRepository(_context);

        public IBranchStockRepository BranchStockRepository => _branchStockRepository ??= new BranchStockRepository(_context);
        public ICategoryRepository CategoryRepository => _categoryRepository ??= new CategoryRepository(_context);
        public IProductRepository ProductRepository => _productRepository ??= new ProductRepository(_context);
        public IProductDetailRepository ProductDetailRepository => _productDetailRepository ??= new ProductDetailRepository(_context);
        public ISystemLogRepository SystemLogRepository => _systemLogRepository ??= new SystemLogRepository(_context);

        public IPurchaseOrderRepository PurchaseOrderRepository => _purchaseOrderRepository ??= new PurchaseOrderRepository(_context);

        public ITransferOrderRepository TransferOrderRepository => _transferOrderRepository ??= new TransferOrderRepository(_context);

        public IPurchaseReturnRepository PurchaseReturnRepository => _purchaseReturnRepository ??= new PurchaseReturnRepository(_context);

        public async Task BeginTransaction()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransaction()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransaction()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task BulkInsertAsync<T>(IList<T> entities) where T : class
        {
            if (entities == null || entities.Count == 0) return;
            await _context.BulkInsertAsync(entities);
        }

        public async Task BulkUpdateAsync<T>(IList<T> entities) where T : class
        {
            if (entities == null || entities.Count == 0) return;
            await _context.BulkUpdateAsync(entities);
        }

        public async Task BulkDeleteAsync<T>(IList<T> entities) where T : class
        {
            if (entities == null || entities.Count == 0) return;
            await _context.BulkDeleteAsync(entities);
        }
    }
}
