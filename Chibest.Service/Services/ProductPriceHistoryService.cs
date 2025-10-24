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

public class ProductPriceHistoryService : IProductPriceHistoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemLogService _systemLogService;

    public ProductPriceHistoryService(
        IUnitOfWork unitOfWork,
        ISystemLogService systemLogService)
    {
        _unitOfWork = unitOfWork;
        _systemLogService = systemLogService;
    }

    public async Task<IBusinessResult> GetListAsync(ProductPriceHistoryQuery query)
    {
        Expression<Func<ProductPriceHistory, bool>> predicate = p => true;

        if (query.ProductId.HasValue)
        {
            predicate = predicate.And(p => p.ProductId == query.ProductId.Value);
        }

        if (query.BranchId.HasValue)
        {
            predicate = predicate.And(p => p.BranchId == query.BranchId.Value);
        }

        if (query.CreatedBy.HasValue)
        {
            predicate = predicate.And(p => p.CreatedBy == query.CreatedBy.Value);
        }

        if (query.EffectiveDateFrom.HasValue)
        {
            predicate = predicate.And(p => p.EffectiveDate >= query.EffectiveDateFrom.Value);
        }

        if (query.EffectiveDateTo.HasValue)
        {
            predicate = predicate.And(p => p.EffectiveDate <= query.EffectiveDateTo.Value);
        }

        if (query.ExpiryDateFrom.HasValue)
        {
            predicate = predicate.And(p => p.ExpiryDate >= query.ExpiryDateFrom.Value);
        }

        if (query.ExpiryDateTo.HasValue)
        {
            predicate = predicate.And(p => p.ExpiryDate <= query.ExpiryDateTo.Value);
        }

        if (query.IsActive.HasValue)
        {
            var now = DateTime.UtcNow;
            if (query.IsActive.Value)
            {
                predicate = predicate.And(p => p.EffectiveDate <= now &&
                    (p.ExpiryDate == null || p.ExpiryDate > now));
            }
            else
            {
                predicate = predicate.And(p => p.EffectiveDate > now ||
                    (p.ExpiryDate != null && p.ExpiryDate <= now));
            }
        }

        if (!string.IsNullOrEmpty(query.Note))
        {
            predicate = predicate.And(p => p.Note.Contains(query.Note));
        }

        Func<IQueryable<ProductPriceHistory>, IOrderedQueryable<ProductPriceHistory>> orderBy = null;
        if (!string.IsNullOrEmpty(query.SortBy))
        {
            orderBy = query.SortBy.ToLower() switch
            {
                "effectivedate" => q => query.SortDescending ?
                    q.OrderByDescending(p => p.EffectiveDate) : q.OrderBy(p => p.EffectiveDate),
                "sellingprice" => q => query.SortDescending ?
                    q.OrderByDescending(p => p.SellingPrice) : q.OrderBy(p => p.SellingPrice),
                "createdat" => q => query.SortDescending ?
                    q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt),
                _ => q => query.SortDescending ?
                    q.OrderByDescending(p => p.EffectiveDate) : q.OrderBy(p => p.EffectiveDate)
            };
        }

        var priceHistories = await _unitOfWork.ProductPriceHistoryRepository.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            predicate,
            orderBy
        );

        var totalCount = await _unitOfWork.ProductPriceHistoryRepository.GetByWhere(predicate).CountAsync();
        var response = priceHistories.Adapt<List<ProductPriceHistoryResponse>>();

        var pagedResult = new PagedResult<ProductPriceHistoryResponse>
        {
            DataList = response,
            TotalCount = totalCount,
            PageIndex = query.PageNumber,
            PageSize = query.PageSize
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, pagedResult);
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id)
    {
        var priceHistory = await _unitOfWork.ProductPriceHistoryRepository.GetByIdAsync(id);
        if (priceHistory == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = priceHistory.Adapt<ProductPriceHistoryResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetCurrentPricesAsync(Guid? branchId = null)
    {
        var now = DateTime.UtcNow;

        Expression<Func<ProductPriceHistory, bool>> predicate = p =>
            p.EffectiveDate <= now &&
            (p.ExpiryDate == null || p.ExpiryDate > now);

        if (branchId.HasValue)
        {
            predicate = predicate.And(p => p.BranchId == branchId.Value || p.BranchId == null);
        }
        else
        {
            predicate = predicate.And(p => p.BranchId == null);
        }

        // Get the latest price for each product
        var currentPrices = await _unitOfWork.ProductPriceHistoryRepository.GetByWhere(predicate)
            .GroupBy(p => p.ProductId)
            .Select(g => g.OrderByDescending(p => p.EffectiveDate).ThenByDescending(p => p.CreatedAt).First())
            .ToListAsync();

        var response = currentPrices.Adapt<IEnumerable<ProductPriceHistoryResponse>>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetByProductIdAsync(Guid productId)
    {
        var priceHistories = await _unitOfWork.ProductPriceHistoryRepository.GetByWhere(p => p.ProductId == productId)
            .OrderByDescending(p => p.EffectiveDate)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        var response = priceHistories.Adapt<IEnumerable<ProductPriceHistoryResponse>>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetByBranchIdAsync(Guid branchId)
    {
        var priceHistories = await _unitOfWork.ProductPriceHistoryRepository.GetByWhere(p =>
                p.BranchId == branchId || p.BranchId == null)
            .OrderByDescending(p => p.EffectiveDate)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        var response = priceHistories.Adapt<IEnumerable<ProductPriceHistoryResponse>>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> CreateAsync(ProductPriceHistoryRequest request, Guid accountId)
    {
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Validate effective date
        if (request.EffectiveDate < DateTime.UtcNow.AddDays(-1))
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Ngày hiệu lực không thể trong quá khứ");

        // Check for overlapping price periods for the same product and branch
        var overlappingPrices = await _unitOfWork.ProductPriceHistoryRepository.GetByWhere(p =>
            p.ProductId == request.ProductId &&
            p.BranchId == request.BranchId &&
            p.EffectiveDate <= request.ExpiryDate &&
            (p.ExpiryDate == null || p.ExpiryDate >= request.EffectiveDate))
            .AnyAsync();

        if (overlappingPrices)
            return new BusinessResult(Const.HTTP_STATUS_CONFLICT, "Đã tồn tại giá trong khoảng thời gian này");

        var priceHistory = request.Adapt<ProductPriceHistory>();
        priceHistory.Id = Guid.NewGuid();
        priceHistory.CreatedBy = accountId;
        priceHistory.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.ProductPriceHistoryRepository.AddAsync(priceHistory);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("Create", "ProductPriceHistory", priceHistory.Id, accountId,
                            null, JsonSerializer.Serialize(priceHistory),
                            $"Tạo mới lịch sử giá cho sản phẩm {request.ProductId} - Giá: {request.SellingPrice}");

        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG);
    }

    public async Task<IBusinessResult> UpdateAsync(ProductPriceHistoryRequest request, Guid accountId)
    {
        var existing = await _unitOfWork.ProductPriceHistoryRepository.GetByIdAsync(request.Id);
        if (existing == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var oldValue = JsonSerializer.Serialize(existing);
        var oldPrice = existing.SellingPrice;
        var oldEffectiveDate = existing.EffectiveDate;

        // Check for overlapping price periods (excluding current record)
        var overlappingPrices = await _unitOfWork.ProductPriceHistoryRepository.GetByWhere(p =>
            p.Id != request.Id &&
            p.ProductId == request.ProductId &&
            p.BranchId == request.BranchId &&
            p.EffectiveDate <= request.ExpiryDate &&
            (p.ExpiryDate == null || p.ExpiryDate >= request.EffectiveDate))
            .AnyAsync();

        if (overlappingPrices)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Đã tồn tại giá trong khoảng thời gian này");

        request.Adapt(existing);

        _unitOfWork.ProductPriceHistoryRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        var changes = $"Cập nhật giá từ {oldPrice} → {existing.SellingPrice}, " +
                     $"Ngày hiệu lực từ {oldEffectiveDate:dd/MM/yyyy} → {existing.EffectiveDate:dd/MM/yyyy}";

        await LogSystemAction("Update", "ProductPriceHistory", request.Id, accountId,
                            oldValue, JsonSerializer.Serialize(existing),
                            changes);

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
    }

    public async Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId)
    {
        var existing = await _unitOfWork.ProductPriceHistoryRepository.GetByIdAsync(id);
        if (existing == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var oldValue = JsonSerializer.Serialize(existing);

        _unitOfWork.ProductPriceHistoryRepository.Delete(existing);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("Delete", "ProductPriceHistory", id, accountId,
                            oldValue, null,
                            $"Xóa lịch sử giá cho sản phẩm {existing.ProductId} - Giá: {existing.SellingPrice}");

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }

    private async Task LogSystemAction(string action, string entityType, Guid entityId, Guid accountId,
                                     string oldValue, string? newValue, string description)
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
            Module = "ProductPriceHistory",
            LogLevel = "INFO"
        };

        await _systemLogService.CreateAsync(logRequest);
    }
}
