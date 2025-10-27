using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;
using Chibest.Service.ModelDTOs.Response;
using Chibest.Service.Utilities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;

namespace Chibest.Service.Services;

public class BranchStockService : IBranchStockService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly ISystemLogService _systemLogService;

    public BranchStockService(IUnitOfWork unitOfWork, ISystemLogService systemLogService)
    {
        _unitOfWork = unitOfWork;
        _systemLogService = systemLogService;
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var branchStock = await _unitOfWork.BranchStockRepository.GetByIdAsync(id);
        if (branchStock == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = branchStock.Adapt<BranchStockResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetListAsync(BranchStockQuery query)
    {
        Expression<Func<BranchStock, bool>> predicate = s => true;

        if (query.ProductId.HasValue)
        {
            predicate = predicate.And(s => s.ProductId == query.ProductId.Value);
        }

        if (query.BranchId.HasValue)
        {
            predicate = predicate.And(s => s.BranchId == query.BranchId.Value);
        }

        if (query.WarehouseId.HasValue)
        {
            predicate = predicate.And(s => s.WarehouseId == query.WarehouseId.Value);
        }

        if (query.MinAvailableQty.HasValue)
        {
            predicate = predicate.And(s => s.AvailableQty >= query.MinAvailableQty.Value);
        }

        if (query.MaxAvailableQty.HasValue)
        {
            predicate = predicate.And(s => s.AvailableQty <= query.MaxAvailableQty.Value);
        }

        if (query.IsLowStock.HasValue && query.IsLowStock.Value)
        {
            predicate = predicate.And(s => s.AvailableQty <= s.ReorderPoint && s.AvailableQty > 0);
        }

        if (query.IsOutOfStock.HasValue && query.IsOutOfStock.Value)
        {
            predicate = predicate.And(s => s.AvailableQty == 0);
        }

        if (query.NeedsReorder.HasValue && query.NeedsReorder.Value)
        {
            predicate = predicate.And(s => s.AvailableQty <= s.ReorderPoint);
        }

        Func<IQueryable<BranchStock>, IOrderedQueryable<BranchStock>>? orderBy = null;
        if (!string.IsNullOrEmpty(query.SortBy))
        {
            orderBy = query.SortBy.ToLower() switch
            {
                "availableqty" => q => query.SortDescending ?
                    q.OrderByDescending(s => s.AvailableQty) : q.OrderBy(s => s.AvailableQty),
                "productname" => q => query.SortDescending ?
                    q.OrderByDescending(s => s.Product.Name) : q.OrderBy(s => s.Product.Name),
                "lastupdated" => q => query.SortDescending ?
                    q.OrderByDescending(s => s.LastUpdated) : q.OrderBy(s => s.LastUpdated),
                "reorderpoint" => q => query.SortDescending ?
                    q.OrderByDescending(s => s.ReorderPoint) : q.OrderBy(s => s.ReorderPoint),
                _ => q => query.SortDescending ?
                    q.OrderByDescending(s => s.LastUpdated) : q.OrderBy(s => s.LastUpdated)
            };
        }

        var branchStocks = await _unitOfWork.BranchStockRepository.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            predicate,
            orderBy
        );

        var totalCount = await _unitOfWork.BranchStockRepository.GetByWhere(predicate).CountAsync();
        var response = branchStocks.Adapt<List<BranchStockResponse>>();

        var pagedResult = new PagedResult<BranchStockResponse>
        {
            DataList = response,
            TotalCount = totalCount,
            PageIndex = query.PageNumber,
            PageSize = query.PageSize
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, pagedResult);
    }

    public async Task<IBusinessResult> GetByProductAndBranchAsync(Guid productId, Guid branchId, Guid? warehouseId = null)
    {
        Expression<Func<BranchStock, bool>> predicate = s =>
            s.ProductId == productId && s.BranchId == branchId;

        if (warehouseId.HasValue)
        {
            predicate = predicate.And(s => s.WarehouseId == warehouseId.Value);
        }

        var branchStock = await _unitOfWork.BranchStockRepository.GetByWhere(predicate)
            .FirstOrDefaultAsync();

        if (branchStock == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = branchStock.Adapt<BranchStockResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetLowStockItemsAsync(Guid? branchId = null)
    {
        Expression<Func<BranchStock, bool>> predicate = s =>
            s.AvailableQty <= s.ReorderPoint && s.AvailableQty > 0;

        if (branchId.HasValue)
        {
            predicate = predicate.And(s => s.BranchId == branchId.Value);
        }

        var lowStockItems = await _unitOfWork.BranchStockRepository.GetByWhere(predicate)
            .OrderBy(s => s.AvailableQty)
            .ToListAsync();

        var response = lowStockItems.Adapt<List<BranchStockResponse>>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetItemsNeedingReorderAsync(Guid? branchId = null)
    {
        Expression<Func<BranchStock, bool>> predicate = s => s.AvailableQty <= s.ReorderPoint;

        if (branchId.HasValue)
        {
            predicate = predicate.And(s => s.BranchId == branchId.Value);
        }

        var reorderItems = await _unitOfWork.BranchStockRepository.GetByWhere(predicate)
            .OrderBy(s => s.AvailableQty)
            .ToListAsync();

        var response = reorderItems.Adapt<IEnumerable<BranchStockResponse>>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> CreateAsync(BranchStockRequest request, Guid accountId)
    {
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Check if stock record already exists for product, branch, and warehouse
        var existingStock = await _unitOfWork.BranchStockRepository.GetByWhere(s =>
            s.ProductId == request.ProductId &&
            s.BranchId == request.BranchId &&
            s.WarehouseId == request.WarehouseId)
            .FirstOrDefaultAsync();

        if (existingStock != null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Đã tồn tại bản ghi tồn kho cho sản phẩm, chi nhánh và kho này");

        var branchStock = request.Adapt<BranchStock>();
        branchStock.Id = Guid.NewGuid();
        branchStock.LastUpdated = DateTime.UtcNow;

        await _unitOfWork.BranchStockRepository.AddAsync(branchStock);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("Create", "BranchStock", branchStock.Id, accountId,
                            null, JsonSerializer.Serialize(branchStock),
                            $"Tạo mới tồn kho - Sản phẩm: {request.ProductId}, Chi nhánh: {request.BranchId}, Kho: {request.WarehouseId}");

        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG);
    }

    public async Task<IBusinessResult> UpdateAsync(BranchStockRequest request, Guid accountId)
    {
        if (request.Id.HasValue == false || request.Id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var branchStock = await _unitOfWork.BranchStockRepository.GetByIdAsync(request.Id);
        if (branchStock == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var oldValue = JsonSerializer.Serialize(branchStock);

        request.Adapt(branchStock);
        branchStock.LastUpdated = DateTime.Now;

        await LogSystemAction("Update", "BranchStock", request.Id.Value, accountId,
                                oldValue, JsonSerializer.Serialize(branchStock),
                                $"Cập nhật tồn kho - Sản phẩm: {request.ProductId}, Chi nhánh: {request.BranchId}");

        _unitOfWork.BranchStockRepository.Update(branchStock);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
    }

    public async Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var branchStock = await _unitOfWork.BranchStockRepository.GetByIdAsync(id);
        if (branchStock == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var oldValue = JsonSerializer.Serialize(branchStock);

        //Save changes
        _unitOfWork.BranchStockRepository.Delete(branchStock);
        await _unitOfWork.SaveChangesAsync();

        //Create systemLog
        await LogSystemAction("Delete", "BranchStock", id, accountId,
                               oldValue, null,
                               $"Xóa tồn kho - Sản phẩm: {branchStock.ProductId}, Chi nhánh: {branchStock.BranchId}");

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }

    private async Task LogSystemAction(string action, string entityType, Guid entityId, Guid accountId,
                                     string? oldValue, string? newValue, string description)
    {
        var account = await _unitOfWork.AccountRepository
            .GetByWhere(acc => acc.Id == accountId)
            .AsNoTracking().FirstOrDefaultAsync();
        var logRequest = new SystemLogRequest
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValue = oldValue,
            NewValue = newValue,
            Description = description,
            AccountId = accountId,
            AccountName = account != null ? account.Name : null,
            Module = "Product",
            LogLevel = "INFO"
        };

        await _systemLogService.CreateAsync(logRequest);
    }
}