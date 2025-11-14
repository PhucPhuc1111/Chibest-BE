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
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

namespace Chibest.Service.Services;

public class ProductPriceHistoryService : IProductPriceHistoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductPriceHistoryService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> GetListAsync(ProductPriceHistoryQuery query, string? productName = null)
    {
        Expression<Func<ProductPriceHistory, bool>> predicate = p => true;

        if (query.ProductId.HasValue)
        {
            predicate = predicate.And(p => p.ProductId == query.ProductId.Value);
        }

        if (query.BranchId.HasValue)
        {
            var branchId = query.BranchId.Value;
            predicate = predicate.And(p => p.BranchId == null || p.BranchId == branchId);
        }
        else
        {
            predicate = predicate.And(p => p.BranchId == null);
        }

        var productNameFilter = string.IsNullOrWhiteSpace(productName) ? query.ProductName : productName;

        if (!string.IsNullOrWhiteSpace(productNameFilter))
        {
            var loweredProductName = productNameFilter.ToLower();
            predicate = predicate.And(p =>
                p.Product != null &&
                p.Product.Name != null &&
                p.Product.Name.ToLower().Contains(loweredProductName) || p.Product.Sku.ToLower().Contains(loweredProductName));
        }

        if (!string.IsNullOrWhiteSpace(query.ListCategory))
        {
            var categoryIds = query.ListCategory
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => Guid.TryParse(value, out var parsed) ? parsed : (Guid?)null)
                .Where(parsed => parsed.HasValue && parsed.Value != Guid.Empty)
                .Select(parsed => parsed!.Value)
                .Distinct()
                .ToList();

            if (categoryIds.Count > 0)
            {
                predicate = predicate.And(p =>
                    p.Product != null &&
                    categoryIds.Contains(p.Product.CategoryId));
            }
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
            var now = DateTime.Now;
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
            predicate = predicate.And(p => p.Note != null && p.Note.Contains(query.Note));
        }

        // include Product
        var queryable = _unitOfWork.ProductPriceHistoryRepository
        .GetByWhere(predicate)
        .Include(p => p.Product);

        Func<IQueryable<ProductPriceHistory>, IOrderedQueryable<ProductPriceHistory>>? orderBy = null;
        if (!string.IsNullOrEmpty(query.SortBy))
        {
            orderBy = query.SortBy.ToLower() switch
            {
                "effectivedate" => q => query.SortDescending ?
                    q.OrderByDescending(p => p.EffectiveDate) : q.OrderBy(p => p.EffectiveDate),
                "sellingprice" => q => query.SortDescending ?
                    q.OrderByDescending(p => p.SellingPrice) : q.OrderBy(p => p.SellingPrice),
                "costprice" => q => query.SortDescending ?
                    q.OrderByDescending(p => p.CostPrice) : q.OrderBy(p => p.CostPrice),
                "createdat" => q => query.SortDescending ?
                    q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt),
                _ => q => query.SortDescending ?
                    q.OrderByDescending(p => p.EffectiveDate) : q.OrderBy(p => p.EffectiveDate)
            };
        }

        // Áp dụng sorting (nếu có)
        IQueryable<ProductPriceHistory> sortedQuery = (orderBy != null) ?
            orderBy(queryable) : queryable.OrderBy(p => p.EffectiveDate);

        var totalCount = await sortedQuery.CountAsync();

        // Thay vì gọi GetPagedAsync, dùng Select và phân trang thủ công
        var responseList = await sortedQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ProductPriceHistoryResponse // <--- DÙNG SELECT
            {
                Id = p.Id,
                SellingPrice = p.SellingPrice,
                CostPrice = p.CostPrice,
                EffectiveDate = p.EffectiveDate,
                ExpiryDate = p.ExpiryDate,
                Note = p.Note,
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedBy,
                ProductId = p.ProductId,
                BranchId = p.BranchId,

                Sku = p.Product != null ? p.Product.Sku : "null",
                Name = p.Product != null ? p.Product.Name : "null"
            })
            .ToListAsync();

        var pagedResult = new PagedResult<ProductPriceHistoryResponse>
        {
            DataList = responseList,
            TotalCount = totalCount,
            PageIndex = query.PageNumber,
            PageSize = query.PageSize
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, pagedResult);
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id)
    {
        // Thay vì GetByIdAsync, dùng GetByWhere + Include + Select
        var response = await _unitOfWork.ProductPriceHistoryRepository
            .GetByWhere(p => p.Id == id)
            .Include(p => p.Product)
            .Select(p => new ProductPriceHistoryResponse
            {
                Id = p.Id,
                SellingPrice = p.SellingPrice,
                CostPrice = p.CostPrice,
                EffectiveDate = p.EffectiveDate,
                ExpiryDate = p.ExpiryDate,
                Note = p.Note,
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedBy,
                ProductId = p.ProductId,
                BranchId = p.BranchId,
                Sku = p.Product != null ? p.Product.Sku : "null",
                Name = p.Product != null ? p.Product.Name : "null"
            })
            .FirstOrDefaultAsync();

        if (response == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetCurrentPricesAsync(Guid? branchId = null)
    {
        var now = DateTime.Now;

        Expression<Func<ProductPriceHistory, bool>> predicate = p =>
            p.EffectiveDate <= now &&
            (p.ExpiryDate == null || p.ExpiryDate > now);

        // Null: Global | Not Null: Specific Branch
        predicate = predicate.And(p => p.BranchId == branchId);

        // Get the latest price for each product
        var response = await _unitOfWork.ProductPriceHistoryRepository.GetByWhere(predicate)
            .Include(p => p.Product) // <--- THÊM INCLUDE
            .GroupBy(p => p.ProductId)
            .Select(g => g.OrderByDescending(p => p.EffectiveDate).ThenByDescending(p => p.CreatedAt).First())
            .Select(p => new ProductPriceHistoryResponse // <--- THÊM SELECT
            {
                Id = p.Id,
                SellingPrice = p.SellingPrice,
                CostPrice = p.CostPrice,
                EffectiveDate = p.EffectiveDate,
                ExpiryDate = p.ExpiryDate,
                Note = p.Note,
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedBy,
                ProductId = p.ProductId,
                BranchId = p.BranchId,

                // Ánh xạ dữ liệu từ Product
                Sku = p.Product != null ? p.Product.Sku : "null",
                Name = p.Product != null ? p.Product.Name : "null"
            })
            .ToListAsync();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetByProductIdAsync(Guid productId)
    {
        var response = await _unitOfWork.ProductPriceHistoryRepository
        .GetByWhere(p => p.ProductId == productId)
        .Include(p => p.Product)
        .OrderByDescending(p => p.EffectiveDate)
        .ThenByDescending(p => p.CreatedAt)
        .Select(p => new ProductPriceHistoryResponse
        {
            Id = p.Id,
            SellingPrice = p.SellingPrice,
            CostPrice = p.CostPrice,
            EffectiveDate = p.EffectiveDate,
            ExpiryDate = p.ExpiryDate,
            Note = p.Note,
            CreatedAt = p.CreatedAt,
            CreatedBy = p.CreatedBy,
            ProductId = p.ProductId,
            BranchId = p.BranchId,
            Sku = p.Product != null ? p.Product.Sku : "null",
            Name = p.Product != null ? p.Product.Name : "null"
        })
        .ToListAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetByBranchIdAsync(Guid branchId)
    {
        var response = await _unitOfWork.ProductPriceHistoryRepository
        .GetByWhere(p => p.BranchId == branchId)
        .Include(p => p.Product)
        .OrderByDescending(p => p.EffectiveDate)
        .ThenByDescending(p => p.CreatedAt)
        .Select(p => new ProductPriceHistoryResponse
        {
            Id = p.Id,
            SellingPrice = p.SellingPrice,
            CostPrice = p.CostPrice,
            EffectiveDate = p.EffectiveDate,
            ExpiryDate = p.ExpiryDate,
            Note = p.Note,
            CreatedAt = p.CreatedAt,
            CreatedBy = p.CreatedBy,
            ProductId = p.ProductId,
            BranchId = p.BranchId,
            Sku = p.Product != null ? p.Product.Sku : "null",
            Name = p.Product != null ? p.Product.Name : "null"
        })
        .ToListAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> CreateAsync(ProductPriceHistoryRequest request, Guid accountId)
    {
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        if (request.ProductId == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "ProductId là bắt buộc.");

        // --- Validate price ---
        if (request.SellingPrice < 0 || request.CostPrice < 0)
        {
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Giá bán và giá vốn không được âm.");
        }

        // Validate effective date
        if (request.EffectiveDate.Date < DateTime.Today)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Ngày hiệu lực không thể trong quá khứ");

        if (request.ExpiryDate.HasValue && request.ExpiryDate.Value <= request.EffectiveDate)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Ngày hết hạn phải sau ngày hiệu lực.");

        var currentPrice = _unitOfWork.ProductPriceHistoryRepository
            .GetByWhere(p =>
                p.ProductId == request.ProductId &&
                p.BranchId == request.BranchId && // So sánh cả (null == null)
                p.ExpiryDate == null)
            .FirstOrDefault();

        if (currentPrice != null)
        {
            // 2. Kiểm tra xung đột ngày
            if (request.EffectiveDate <= currentPrice.EffectiveDate)
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Ngày hiệu lực mới phải sau ngày hiệu lực của giá hiện tại.");
            }

            currentPrice.ExpiryDate = request.EffectiveDate.AddMilliseconds(-1);
            _unitOfWork.ProductPriceHistoryRepository.Update(currentPrice);
        }

        // 4. Tạo giá mới
        var newPrice = request.Adapt<ProductPriceHistory>();
        newPrice.Id = Guid.NewGuid();
        newPrice.CreatedBy = accountId;
        newPrice.CreatedAt = DateTime.Now;

        await _unitOfWork.ProductPriceHistoryRepository.AddAsync(newPrice);
        await _unitOfWork.SaveChangesAsync();


        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG);
    }

    public async Task<IBusinessResult> UpdateAsync(ProductPriceHistoryRequest request, Guid accountId)
    {
        if (request.Id.HasValue == false || request.Id.Value == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var existing = await _unitOfWork.ProductPriceHistoryRepository.GetByWhere(p => p.Id == request.Id)
            .Include(p => p.Product)
            .FirstOrDefaultAsync();
        if (existing == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var oldValue = JsonSerializer.Serialize(existing);

        // --- Validation ---
        if (request.SellingPrice < 0 || request.CostPrice < 0)
        {
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Giá bán và giá vốn không được âm.");
        }
        if (request.ExpiryDate.HasValue && request.ExpiryDate.Value <= request.EffectiveDate)
        {
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Ngày hết hạn phải sau ngày hiệu lực.");
        }

        // Logic kiểm tra overlap nên dựa trên ProductId và BranchId *hiện tại*
        // của bản ghi, không phải từ request (trừ khi bạn cho phép thay đổi Product/Branch)
        var overlappingPrices = await _unitOfWork.ProductPriceHistoryRepository.GetByWhere(p =>
            p.Id != request.Id.Value &&
            p.ProductId == existing.ProductId && // Dùng ProductId của bản ghi cũ
            p.BranchId == existing.BranchId && // Dùng BranchId của bản ghi cũ
            p.EffectiveDate <= request.ExpiryDate && // So sánh với ngày từ request
            (p.ExpiryDate == null || p.ExpiryDate >= request.EffectiveDate))
            .AnyAsync();

        if (overlappingPrices)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Đã tồn tại giá trong khoảng thời gian này");

        // gán thủ công các trường được phép thay đổi.
        // Tránh ghi đè các ProductId, BranchId, CreatedAt...
        existing.SellingPrice = request.SellingPrice;
        existing.CostPrice = request.CostPrice;
        existing.EffectiveDate = request.EffectiveDate;
        existing.ExpiryDate = request.ExpiryDate;
        existing.Note = request.Note;

        //_unitOfWork.ProductPriceHistoryRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync();


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


        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }
}
