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

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemLogService _systemLogService;

    public ProductService(IUnitOfWork unitOfWork, ISystemLogService systemLogService)
    {
        _unitOfWork = unitOfWork;
        _systemLogService = systemLogService;
    }

    public async Task<IBusinessResult> GetListAsync(ProductQuery query)
    {
        Expression<Func<Product, bool>> predicate = p => true;

        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            predicate = predicate.And(p =>
                p.Name.Contains(query.SearchTerm) ||
                p.Sku.Contains(query.SearchTerm) ||
                p.Description.Contains(query.SearchTerm));
        }

        if (!string.IsNullOrEmpty(query.Status))
        {
            predicate = predicate.And(p => p.Status == query.Status);
        }

        if (query.CategoryId.HasValue)
        {
            predicate = predicate.And(p => p.CategoryId == query.CategoryId.Value);
        }

        if (query.IsMaster.HasValue)
        {
            predicate = predicate.And(p => p.IsMaster == query.IsMaster.Value);
        }

        if (!string.IsNullOrEmpty(query.Brand))
        {
            predicate = predicate.And(p => p.Brand == query.Brand);
        }

        Func<IQueryable<Product>, IOrderedQueryable<Product>> orderBy = null;
        if (!string.IsNullOrEmpty(query.SortBy))
        {
            orderBy = query.SortBy.ToLower() switch
            {
                "name" => q => query.SortDescending ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name),
                "sku" => q => query.SortDescending ? q.OrderByDescending(p => p.Sku) : q.OrderBy(p => p.Sku),
                "createdat" => q => query.SortDescending ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt),
                _ => q => query.SortDescending ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt)
            };
        }

        var products = await _unitOfWork.ProductRepository.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            predicate,
            orderBy
        );

        var totalCount = await _unitOfWork.ProductRepository.GetByWhere(predicate).CountAsync();
        var response = products.Adapt<List<ProductResponse>>();

        var pagedResult = new PagedResult<ProductResponse>
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
        var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
        if (product == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = product.Adapt<ProductResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetBySKUAsync(string sku)
    {
        var product = await _unitOfWork.ProductRepository.GetByWhere(p => p.Sku == sku)
            .FirstOrDefaultAsync();

        if (product == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = product.Adapt<ProductResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> CreateAsync(ProductRequest request, Guid accountId)
    {
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Check if SKU already exists
        var existingSku = await _unitOfWork.ProductRepository.GetByWhere(p => p.Sku == request.Sku)
            .FirstOrDefaultAsync();
        if (existingSku != null)
            return new BusinessResult(Const.HTTP_STATUS_CONFLICT, "SKU đã tồn tại");

        var product = request.Adapt<Product>();
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ProductRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("Create", "Product", product.Id, accountId,
                            null, JsonSerializer.Serialize(product),
                            $"Tạo mới sản phẩm: {product.Name} (SKU: {product.Sku})");

        var response = product.Adapt<ProductResponse>();
        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG, response);
    }

    public async Task<IBusinessResult> UpdateAsync(ProductRequest request, Guid accountId)
    {
        var existing = await _unitOfWork.ProductRepository.GetByIdAsync(request.Id);
        if (existing == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var oldValue = JsonSerializer.Serialize(existing);
        var oldName = existing.Name;

        request.Adapt(existing);
        existing.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.ProductRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("Update", "Product", request.Id, accountId,
                            oldValue, JsonSerializer.Serialize(existing),
                            $"Cập nhật sản phẩm: {oldName} → {existing.Name}");

        var response = existing.Adapt<ProductResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG, response);
    }

    public async Task<IBusinessResult> UpdateStatusAsync(Guid id, Guid accountId, string status)
    {
        var existing = await _unitOfWork.ProductRepository.GetByIdAsync(id);
        if (existing == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var oldStatus = existing.Status;
        existing.Status = status;
        existing.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.ProductRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("UpdateStatus", "Product", id, accountId,
                            oldStatus, status,
                            $"Thay đổi trạng thái sản phẩm: {oldStatus} → {status}");

        var response = existing.Adapt<ProductResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG, response);
    }

    public async Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId)
    {
        var existing = await _unitOfWork.ProductRepository.GetByIdAsync(id);
        if (existing == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var oldValue = JsonSerializer.Serialize(existing);

        _unitOfWork.ProductRepository.Delete(existing);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("Delete", "Product", id, accountId,
                            oldValue, null,
                            $"Xóa sản phẩm: {existing.Name} (SKU: {existing.Sku})");

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
            Module = "Product",
            LogLevel = "INFO"
        };

        await _systemLogService.CreateAsync(logRequest);
    }
}