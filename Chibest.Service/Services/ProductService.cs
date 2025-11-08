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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
                p.Name.ToLower().Contains(query.SearchTerm.ToLower()) ||
                p.Sku.ToLower().Contains(query.SearchTerm.ToLower()) ||
                p.Description != null && p.Description.ToLower().Contains(query.SearchTerm.ToLower()));
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

        Func<IQueryable<Product>, IOrderedQueryable<Product>>? orderBy = null;
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

        // Include both ProductPriceHistories and BranchStocks
        var products = await _unitOfWork.ProductRepository.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            predicate,
            orderBy,
            include: q => q.Include(p => p.ProductPriceHistories)
            .Include(p => p.BranchStocks)
            .Include(p => p.Category)
        );

        var response = products.Select(product =>
        {
            var latestPrice = GetLatestPriceHistory(product.ProductPriceHistories, query.BranchId);

            var branchStock = query.BranchId.HasValue
                ? product.BranchStocks?.FirstOrDefault(bs => bs.BranchId == query.BranchId.Value)
                : null;

            return new ProductResponse
            {
                Id = product.Id,
                AvartarUrl = product.AvatarUrl,
                Sku = product.Sku,
                Name = product.Name,
                Description = product.Description,
                Color = product.Color,
                Size = product.Size,
                Style = product.Style,
                Brand = product.Brand,
                Material = product.Material,
                Weight = product.Weight,
                IsMaster = product.IsMaster,
                Status = product.Status,
                ParentSku = product.ParentSku,
                CategoryName = product.Category?.Name,
                CostPrice = latestPrice?.CostPrice,
                SellingPrice = latestPrice?.SellingPrice,
                StockQuantity = branchStock?.AvailableQty ?? 0
            };
        }).ToList();


        var totalCount = await _unitOfWork.ProductRepository.GetByWhere(predicate).CountAsync();
        var pagedResult = new PagedResult<ProductResponse>
        {
            DataList = response,
            TotalCount = totalCount,
            PageIndex = query.PageNumber,
            PageSize = query.PageSize
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, pagedResult);
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id, Guid? branchId)
    {
        var product = await _unitOfWork.ProductRepository
            .GetByWhere(p => p.Id == id)
            .Include(p => p.ProductPriceHistories)
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        var branchStock = branchId.HasValue ?
            await _unitOfWork.BranchStockRepository
                .GetByWhere(bs => bs.ProductId == id && bs.BranchId == branchId.Value)
                .AsNoTracking()
                .FirstOrDefaultAsync()
            : null;
        if (product == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var latestPrice = GetLatestPriceHistory(product.ProductPriceHistories, branchId);

        var response = new ProductResponse
        {
            Id = product.Id,
            AvartarUrl = product.AvatarUrl,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            Color = product.Color,
            Size = product.Size,
            Style = product.Style,
            Brand = product.Brand,
            Material = product.Material,
            Weight = product.Weight,
            IsMaster = product.IsMaster,
            Status = product.Status,
            ParentSku = product.ParentSku,
            CategoryName = product.Category.Name,
            CostPrice = latestPrice?.CostPrice,
            SellingPrice = latestPrice?.SellingPrice,
            StockQuantity = branchStock?.AvailableQty ?? 0
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }


    public async Task<IBusinessResult> GetBySKUAsync(string sku, [FromForm] Guid? branchId)
    {
        var product = await _unitOfWork.ProductRepository.GetByWhere(p => p.Sku == sku)
            .Include(p => p.ProductPriceHistories)
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        var branchStock = branchId.HasValue
            ? await _unitOfWork.BranchStockRepository
                .GetByWhere(bs => bs.ProductId == product.Id && bs.BranchId == branchId)
                .AsNoTracking()
                .FirstOrDefaultAsync()
            : null;
        if (product == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var latestPrice = GetLatestPriceHistory(product.ProductPriceHistories, branchId);

        var response = new ProductResponse
        {
            Id = product.Id,
            AvartarUrl = product.AvatarUrl,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            Color = product.Color,
            Size = product.Size,
            Style = product.Style,
            Brand = product.Brand,
            Material = product.Material,
            Weight = product.Weight,
            IsMaster = product.IsMaster,
            Status = product.Status,
            ParentSku = product.ParentSku,
            CategoryName = product.Category.Name,
            CostPrice = latestPrice?.CostPrice,
            SellingPrice = latestPrice?.SellingPrice,
            StockQuantity = branchStock?.AvailableQty ?? 0
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> CreateAsync(ProductRequest request, Guid accountId)
    {
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Check SKU trùng
        var existingSku = await _unitOfWork.ProductRepository
            .GetByWhere(p => p.Sku == request.Sku)
            .FirstOrDefaultAsync();

        if (existingSku != null)
            return new BusinessResult(Const.HTTP_STATUS_CONFLICT, "SKU đã tồn tại");


        var product = request.Adapt<Product>();
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.Now;
        product.UpdatedAt = DateTime.Now;

        await _unitOfWork.ProductRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        if (request.SellingPrice.HasValue && request.CostPrice.HasValue)
        {
            var priceHistory = new ProductPriceHistory
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                BranchId = request.BranchId,
                SellingPrice = request.SellingPrice.Value,
                CostPrice = request.CostPrice.Value,
                EffectiveDate = request.EffectiveDate ?? DateTime.Now,
                ExpiryDate = request.ExpiryDate,
                CreatedAt = DateTime.Now,
                CreatedBy = accountId,
                Note = "Giá khởi tạo sản phẩm"
            };

            await _unitOfWork.ProductPriceHistoryRepository.AddAsync(priceHistory);
            await _unitOfWork.SaveChangesAsync();
        }

        var productLog = product.Adapt<ProductRequest>(); // hoặc ProductResponse
        await LogSystemAction("Create", "Product", product.Id, accountId,
            null, JsonSerializer.Serialize(productLog),
            $"Tạo mới sản phẩm: {product.Name} (SKU: {product.Sku})");
        var response = product.Adapt<ProductResponse>();
        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG, response);
    }


    public async Task<IBusinessResult> UpdateAsync(ProductRequest request, Guid accountId)
    {
        if (request.Id.HasValue == false || request.Id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var existing = await _unitOfWork.ProductRepository.GetByIdAsync(request.Id);
        if (existing == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var oldValue = JsonSerializer.Serialize(existing);
        var oldName = existing.Name;

        request.Adapt(existing);
        existing.UpdatedAt = DateTime.Now;

        _unitOfWork.ProductRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("Update", "Product", request.Id.Value, accountId,
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
        existing.UpdatedAt = DateTime.Now;

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

    private static ProductPriceHistory? GetLatestPriceHistory(IEnumerable<ProductPriceHistory>? priceHistories, Guid? branchId)
    {
        if (priceHistories == null)
            return null;

        ProductPriceHistory? SelectLatest(IEnumerable<ProductPriceHistory> source) =>
            source
                .OrderByDescending(h => h.EffectiveDate)
                .ThenByDescending(h => h.CreatedAt)
                .FirstOrDefault();

        if (branchId.HasValue)
        {
            var branchMatch = SelectLatest(priceHistories.Where(h => h.BranchId == branchId));
            if (branchMatch != null)
                return branchMatch;

            var globalMatch = SelectLatest(priceHistories.Where(h => h.BranchId == null));
            if (globalMatch != null)
                return globalMatch;
        }
        else
        {
            var globalMatch = SelectLatest(priceHistories.Where(h => h.BranchId == null));
            if (globalMatch != null)
                return globalMatch;
        }

        return SelectLatest(priceHistories);
    }
}