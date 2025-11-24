using Chibest.Repository.Interface;
using Chibest.Repository.Models;
using Chibest.Repository.Repositories;
using EFCore.BulkExtensions;

namespace Chibest.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        protected readonly ChiBestDbContext _context;
        private IAccountRepository _accountRepository;
        private IAccountRoleRepository _accountRoleRepository;
        private IRoleRepository _roleRepository;
        private IPermissionRepository _permissionRepository;
        private IBranchRepository _branchRepository;
        private IBranchStockRepository _branchStockRepository;
        private IProductRepository _productRepository;
        private IProductDetailRepository _productDetailRepository;
        private IProductPriceHistoryRepository _productPriceHistoryRepository;
        private ICategoryRepository _categoryRepository;
        private IColorRepository _colorRepository;
        private ISizeRepository _sizeRepository;
        private IPurchaseOrderRepository _purchaseOrderRepository;
        private ITransferOrderRepository _transferOrderRepository;
        private IPurchaseReturnRepository _purchaseReturnRepository;
        private IStockAdjusmentRepository _stockAdjusmentRepository;
        private IProductPlanRepository _productPlanRepository;
        private ISupplierDebtRepository _supplierDebtRepository;    
        private IBranchDebtRepository _branchDebtRepository;
        private IBranchDebtHistoryRepository _branchDebtHistoryRepository;
        private IPurchaseOrderDetailRepository _purchaseOrderDetailRepository;
        private IPurchaseReturnDetailRepository _purchaseReturnDetailRepository;
        private IStockAdjustmentDetailRepository _stockAdjustmentDetailRepository;
        private ISupplierDebtHistoryRepository _supplierDebtHistoryRepository;
        private ITransferOrderDetailRepository _transferOrderDetailRepository;
        public UnitOfWork(ChiBestDbContext context)
        {
            _context = context;
        }
        public IAccountRepository AccountRepository => _accountRepository ??= new AccountRepository(_context);
        public IAccountRoleRepository AccountRoleRepository => _accountRoleRepository ??= new AccountRoleRepository(_context);
        public IRoleRepository RoleRepository => _roleRepository ??= new RoleRepository(_context);
        public IPermissionRepository PermissionRepository => _permissionRepository ??= new PermissionRepository(_context);
        public IBranchRepository BranchRepository => _branchRepository ??= new BranchRepository(_context);

        public IBranchStockRepository BranchStockRepository => _branchStockRepository ??= new BranchStockRepository(_context);
        public ICategoryRepository CategoryRepository => _categoryRepository ??= new CategoryRepository(_context);
        public IColorRepository ColorRepository => _colorRepository ??= new ColorRepository(_context);
        public ISizeRepository SizeRepository => _sizeRepository ??= new SizeRepository(_context);
        public IProductRepository ProductRepository => _productRepository ??= new ProductRepository(_context);
        public IProductDetailRepository ProductDetailRepository 
            => _productDetailRepository ??= new ProductDetailRepository(_context);
        public IProductPriceHistoryRepository ProductPriceHistoryRepository 
            => _productPriceHistoryRepository ??= new ProductPriceHistoryRepository(_context);

        public IPurchaseOrderRepository PurchaseOrderRepository => _purchaseOrderRepository ??= new PurchaseOrderRepository(_context);

        public ITransferOrderRepository TransferOrderRepository => _transferOrderRepository ??= new TransferOrderRepository(_context);

        public IPurchaseReturnRepository PurchaseReturnRepository => _purchaseReturnRepository ??= new PurchaseReturnRepository(_context);
        public IProductPlanRepository ProductPlanRepository => _productPlanRepository ??= new ProductPlanRepository(_context);

        public IStockAdjusmentRepository StockAdjusmentRepository => _stockAdjusmentRepository ??= new StockAdjusmentRepository(_context);
        public ISupplierDebtRepository SupplierDebtRepository => _supplierDebtRepository ??= new SupplierDebtRepository(_context);
        public IBranchDebtRepository BranchDebtRepository => _branchDebtRepository ??=new BranchDebtRepository(_context);
        public IBranchDebtHistoryRepository BranchDebtHistoryRepository => _branchDebtHistoryRepository ??= new BranchDebtHistoryRepository(_context);
        public IPurchaseOrderDetailRepository PurchaseOrderDetailRepository => _purchaseOrderDetailRepository ??= new PurchaseOrderDetailRepository(_context);
        public IPurchaseReturnDetailRepository PurchaseReturnDetailRepository => _purchaseReturnDetailRepository ??= new PurchaseReturnDetailRepository(_context);
        public IStockAdjustmentDetailRepository StockAdjustmentDetailRepository => _stockAdjustmentDetailRepository ??= new StockAdjustmentDetailRepository(_context);
        public ISupplierDebtHistoryRepository SupplierDebtHistoryRepository => _supplierDebtHistoryRepository ??= new SupplierDebtHistoryRepository(_context);
        public ITransferOrderDetailRepository TransferOrderDetailRepository => _transferOrderDetailRepository ??= new TransferOrderDetailRepository(_context);
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        
    }
}
