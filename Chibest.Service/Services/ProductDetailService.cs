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

public class ProductDetailService : IProductDetailService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemLogService _systemLogService;

    public ProductDetailService(IUnitOfWork unitOfWork, ISystemLogService systemLogService)
    {
        _unitOfWork = unitOfWork;
        _systemLogService = systemLogService;
    }

    public async Task<IBusinessResult> GetListAsync(ProductDetailQuery query)
    {
        Expression<Func<ProductDetail, bool>> predicate = p => true;

        if (!string.IsNullOrEmpty(query.ChipCode))
        {
            predicate = predicate.And(p => p.ChipCode.Contains(query.ChipCode));
        }

        if (query.ProductId.HasValue)
        {
            predicate = predicate.And(p => p.ProductId == query.ProductId.Value);
        }

        if (query.BranchId.HasValue)
        {
            predicate = predicate.And(p => p.BranchId == query.BranchId.Value);
        }

        if (query.WarehouseId.HasValue)
        {
            predicate = predicate.And(p => p.WarehouseId == query.WarehouseId.Value);
        }

        if (!string.IsNullOrEmpty(query.Status))
        {
            predicate = predicate.And(p => p.Status == query.Status);
        }

        if (query.SupplierId.HasValue)
        {
            predicate = predicate.And(p => p.SupplierId == query.SupplierId.Value);
        }

        if (query.ImportDateFrom.HasValue)
        {
            predicate = predicate.And(p => p.ImportDate >= query.ImportDateFrom.Value);
        }

        if (query.ImportDateTo.HasValue)
        {
            predicate = predicate.And(p => p.ImportDate <= query.ImportDateTo.Value);
        }

        Func<IQueryable<ProductDetail>, IOrderedQueryable<ProductDetail>> orderBy = null;
        if (!string.IsNullOrEmpty(query.SortBy))
        {
            orderBy = query.SortBy.ToLower() switch
            {
                "chipcode" => q => query.SortDescending ? q.OrderByDescending(p => p.ChipCode) : q.OrderBy(p => p.ChipCode),
                "importdate" => q => query.SortDescending ? q.OrderByDescending(p => p.ImportDate) : q.OrderBy(p => p.ImportDate),
                "createdat" => q => query.SortDescending ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt),
                _ => q => query.SortDescending ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt)
            };
        }

        var productDetails = await _unitOfWork.ProductDetailRepository.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            predicate,
            orderBy
        );

        var totalCount = await _unitOfWork.ProductDetailRepository.GetByWhere(predicate).CountAsync();
        var response = productDetails.Adapt<List<ProductDetailResponse>>();

        var pagedResult = new PagedResult<ProductDetailResponse>
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
        var productDetail = await _unitOfWork.ProductDetailRepository.GetByIdAsync(id);
        if (productDetail == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = productDetail.Adapt<ProductDetailResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetByChipCodeAsync(string chipCode)
    {
        var productDetail = await _unitOfWork.ProductDetailRepository.GetByWhere(p => p.ChipCode == chipCode)
            .FirstOrDefaultAsync();

        if (productDetail == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = productDetail.Adapt<ProductDetailResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> CreateAsync(ProductDetailRequest request, Guid accountId)
    {
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Check if ChipCode already exists
        if (!string.IsNullOrEmpty(request.ChipCode))
        {
            var existingChip = await _unitOfWork.ProductDetailRepository.GetByWhere(p => p.ChipCode == request.ChipCode)
                .FirstOrDefaultAsync();
            if (existingChip != null)
                return new BusinessResult(Const.HTTP_STATUS_CONFLICT, "ChipCode đã tồn tại");
        }

        var productDetail = request.Adapt<ProductDetail>();
        productDetail.Id = Guid.NewGuid();
        productDetail.CreatedAt = DateTime.UtcNow;
        productDetail.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ProductDetailRepository.AddAsync(productDetail);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("Create", "ProductDetail", productDetail.Id, accountId,
                            null, JsonSerializer.Serialize(productDetail),
                            $"Tạo mới chi tiết sản phẩm: {productDetail.ChipCode}");

        var response = productDetail.Adapt<ProductDetailResponse>();
        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG, response);
    }

    public async Task<IBusinessResult> UpdateAsync(ProductDetailRequest request, Guid accountId)
    {
        var existing = await _unitOfWork.ProductDetailRepository.GetByIdAsync(request.Id);
        if (existing == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var oldValue = JsonSerializer.Serialize(existing);

        request.Adapt(existing);
        existing.UpdatedAt = DateTime.Now;

        _unitOfWork.ProductDetailRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("Update", "ProductDetail", request.Id, accountId,
                            oldValue, JsonSerializer.Serialize(existing),
                            $"Cập nhật chi tiết sản phẩm: {existing.ChipCode}");

        var response = existing.Adapt<ProductDetailResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG, response);
    }

    public async Task<IBusinessResult> UpdateStatusAsync(Guid id, Guid accountId, string status)
    {
        var existing = await _unitOfWork.ProductDetailRepository.GetByIdAsync(id);
        if (existing == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var oldStatus = existing.Status;
        existing.Status = status;
        existing.UpdatedAt = DateTime.Now;

        _unitOfWork.ProductDetailRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("UpdateStatus", "ProductDetail", id, accountId,
                            oldStatus, status,
                            $"Thay đổi trạng thái chi tiết sản phẩm: {oldStatus} → {status}");

        var response = existing.Adapt<ProductDetailResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG, response);
    }

    public async Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId)
    {
        var existing = await _unitOfWork.ProductDetailRepository.GetByIdAsync(id);
        if (existing == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var oldValue = JsonSerializer.Serialize(existing);

        _unitOfWork.ProductDetailRepository.Delete(existing);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("Delete", "ProductDetail", id, accountId,
                            oldValue, null,
                            $"Xóa chi tiết sản phẩm: {existing.ChipCode}");

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }

    private async Task LogSystemAction(string action, string entityType, Guid entityId, Guid accountId,
                                     string oldValue, string newValue, string description)
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
            Module = "ProductDetail",
            LogLevel = "INFO"
        };

        await _systemLogService.CreateAsync(logRequest);
    }
}
