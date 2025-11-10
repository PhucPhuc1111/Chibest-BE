using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;
using Chibest.Service.ModelDTOs.Response;
using Chibest.Service.Utilities;
using ClosedXML.Excel;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

    public async Task<IBusinessResult> ImportProductsFromExcelAsync(IFormFile file, Guid accountId)
    {
        if (file == null || file.Length == 0)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "File import không hợp lệ.");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var workbook = new XLWorkbook(memoryStream);
        var worksheet = workbook.Worksheets
            .FirstOrDefault(ws => string.Equals(ws.Name, "ProductTemplate", StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.FirstOrDefault();

        if (worksheet == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không tìm thấy sheet dữ liệu trong file.");

        var headerRow = worksheet.FirstRowUsed();
        if (headerRow == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "File import không có dữ liệu.");

        var headerLookup = headerRow.CellsUsed()
            .ToDictionary(cell => NormalizeHeader(cell.GetString()), cell => cell.Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);

        int? colCategory = GetColumn(headerLookup, "nhóm hàng");
        int? colSku = GetColumn(headerLookup, "mã hàng");
        int? colName = GetColumn(headerLookup, "tên hàng");
        int? colBrand = GetColumn(headerLookup, "thương hiệu");
        int? colSellPrice = GetColumn(headerLookup, "giá bán");
        int? colCostPrice = GetColumn(headerLookup, "giá vốn");
        int? colSize = GetColumn(headerLookup, "size");
        int? colStyle = GetColumn(headerLookup, "style");
        int? colColor = GetColumn(headerLookup, "màu");
        int? colParentSku = GetColumn(headerLookup, "parentsku");
        int? colIsMaster = GetColumn(headerLookup, "ismaster");
        int? colImages = GetColumn(headerLookup, "hình ảnh (url1,url2...)", "hình ảnh");
        int? colStatus = GetColumn(headerLookup, "đang kinh");
        int? colDescription = GetColumn(headerLookup, "mô tả");

        if (colCategory == null || colSku == null || colName == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Thiếu cột bắt buộc trong file import (Nhóm hàng, Mã hàng, Tên hàng).");

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        if (lastRow <= headerRow.RowNumber())
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "File import không có dữ liệu.");

        var categories = await _unitOfWork.CategoryRepository
            .GetAll()
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        var categoryLookup = categories.ToDictionary(
            c => c.Name.Trim().ToLowerInvariant(),
            c => c.Id);

        var importRows = new List<ProductImportRow>();
        var seenSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var errors = new List<string>();

        for (int rowNumber = headerRow.RowNumber() + 1; rowNumber <= lastRow; rowNumber++)
        {
            var row = worksheet.Row(rowNumber);
            if (!row.CellsUsed().Any())
                continue;

            var sku = GetCellString(row, colSku).Trim();
            if (string.IsNullOrWhiteSpace(sku))
            {
                errors.Add($"Dòng {rowNumber}: Thiếu Mã hàng.");
                continue;
            }

            if (!seenSkus.Add(sku))
            {
                errors.Add($"Dòng {rowNumber}: Mã hàng '{sku}' bị trùng trong file.");
                continue;
            }

            var categoryName = GetCellString(row, colCategory).Trim();
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                errors.Add($"Dòng {rowNumber}: Thiếu Nhóm hàng.");
                continue;
            }

            var productName = GetCellString(row, colName).Trim();
            if (string.IsNullOrWhiteSpace(productName))
            {
                errors.Add($"Dòng {rowNumber}: Thiếu Tên hàng.");
                continue;
            }

            importRows.Add(new ProductImportRow
            {
                RowNumber = rowNumber,
                CategoryName = categoryName,
                Sku = sku,
                Name = productName,
                Brand = ToNullIfEmpty(GetCellString(row, colBrand)),
                SellingPrice = ParseDecimal(GetCellString(row, colSellPrice)),
                CostPrice = ParseDecimal(GetCellString(row, colCostPrice)),
                Size = ToNullIfEmpty(GetCellString(row, colSize)),
                Style = ToNullIfEmpty(GetCellString(row, colStyle)),
                Color = ToNullIfEmpty(GetCellString(row, colColor)),
                ParentSku = ToNullIfEmpty(GetCellString(row, colParentSku)),
                IsMaster = ParseBoolean(GetCellString(row, colIsMaster)),
                AvatarUrl = ExtractFirstImage(GetCellString(row, colImages)),
                Status = !string.IsNullOrWhiteSpace(GetCellString(row, colStatus))
                    ? GetCellString(row, colStatus).Trim()
                    : "Active",
                Description = ToNullIfEmpty(GetCellString(row, colDescription))
            });
        }

        if (!importRows.Any())
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không có dữ liệu hợp lệ trong file import.", errors);

        var skuList = importRows.Select(r => r.Sku).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var existingProducts = await _unitOfWork.ProductRepository
            .GetByWhere(p => skuList.Contains(p.Sku))
            .ToListAsync();

        var existingLookup = existingProducts.ToDictionary(p => p.Sku, StringComparer.OrdinalIgnoreCase);
        var priceHistories = new List<ProductPriceHistory>();
        int created = 0;
        int updated = 0;

        foreach (var item in importRows)
        {
            if (!categoryLookup.TryGetValue(item.CategoryName.Trim().ToLowerInvariant(), out var categoryId))
            {
                errors.Add($"Dòng {item.RowNumber}: Không tìm thấy nhóm hàng '{item.CategoryName}'.");
                continue;
            }

            if (existingLookup.TryGetValue(item.Sku, out var existingProduct))
            {
                var oldSnapshot = CreateProductLogPayload(existingProduct);

                existingProduct.Name = item.Name;
                existingProduct.Brand = item.Brand;
                existingProduct.Size = item.Size;
                existingProduct.Style = item.Style;
                existingProduct.Color = item.Color;
                existingProduct.ParentSku = item.ParentSku;
                existingProduct.IsMaster = item.IsMaster;
                existingProduct.AvatarUrl = item.AvatarUrl;
                existingProduct.Description = item.Description;
                existingProduct.Status = item.Status;
                existingProduct.CategoryId = categoryId;
                existingProduct.UpdatedAt = DateTime.Now;

                _unitOfWork.ProductRepository.Update(existingProduct);
                existingLookup[item.Sku] = existingProduct;

                if (item.SellingPrice.HasValue && item.CostPrice.HasValue)
                {
                    priceHistories.Add(new ProductPriceHistory
                    {
                        Id = Guid.NewGuid(),
                        ProductId = existingProduct.Id,
                        SellingPrice = item.SellingPrice.Value,
                        CostPrice = item.CostPrice.Value,
                        EffectiveDate = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        CreatedBy = accountId,
                        Note = "Import Excel"
                    });
                }

                updated++;
                await LogSystemAction("ImportUpdate", "Product", existingProduct.Id, accountId,
                    JsonSerializer.Serialize(oldSnapshot),
                    JsonSerializer.Serialize(CreateProductLogPayload(existingProduct)),
                    $"Cập nhật sản phẩm từ import: {existingProduct.Name} (SKU: {existingProduct.Sku})");
            }
            else
            {
                var now = DateTime.Now;
                var newProduct = new Product
                {
                    Id = Guid.NewGuid(),
                    Sku = item.Sku,
                    Name = item.Name,
                    Brand = item.Brand,
                    Size = item.Size,
                    Style = item.Style,
                    Color = item.Color,
                    ParentSku = item.ParentSku,
                    IsMaster = item.IsMaster,
                    AvatarUrl = item.AvatarUrl,
                    Description = item.Description,
                    Status = item.Status,
                    CategoryId = categoryId,
                    Weight = 0,
                    Material = null,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await _unitOfWork.ProductRepository.AddAsync(newProduct);
                existingLookup[item.Sku] = newProduct;

                if (item.SellingPrice.HasValue && item.CostPrice.HasValue)
                {
                    priceHistories.Add(new ProductPriceHistory
                    {
                        Id = Guid.NewGuid(),
                        ProductId = newProduct.Id,
                        SellingPrice = item.SellingPrice.Value,
                        CostPrice = item.CostPrice.Value,
                        EffectiveDate = now,
                        CreatedAt = now,
                        CreatedBy = accountId,
                        Note = "Import Excel"
                    });
                }

                created++;
                await LogSystemAction("ImportCreate", "Product", newProduct.Id, accountId,
                    null,
                    JsonSerializer.Serialize(CreateProductLogPayload(newProduct)),
                    $"Import sản phẩm: {newProduct.Name} (SKU: {newProduct.Sku})");
            }
        }

        if (priceHistories.Any())
            await _unitOfWork.ProductPriceHistoryRepository.AddRangeAsync(priceHistories);

        if (created > 0 || updated > 0 || priceHistories.Any())
            await _unitOfWork.SaveChangesAsync();

        var message = errors.Any()
            ? "Import hoàn tất với một số lỗi. Vui lòng kiểm tra chi tiết."
            : "Import sản phẩm thành công.";

        var resultPayload = new
        {
            Created = created,
            Updated = updated,
            Errors = errors
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, message, resultPayload);
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

    private static string NormalizeHeader(string header)
    {
        if (string.IsNullOrWhiteSpace(header))
            return string.Empty;

        var normalized = string.Join(" ", header.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return normalized.ToLowerInvariant();
    }

    private static int? GetColumn(Dictionary<string, int> headerLookup, params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            var normalized = NormalizeHeader(alias);
            if (headerLookup.TryGetValue(normalized, out var column))
                return column;
        }

        return null;
    }

    private static string GetCellString(IXLRow row, int? column)
    {
        if (column == null)
            return string.Empty;

        return row.Cell(column.Value).GetString();
    }

    private static string? ToNullIfEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static decimal? ParseDecimal(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var cleaned = input.Replace("₫", string.Empty)
                           .Replace("đ", string.Empty)
                           .Replace(" ", string.Empty)
                           .Trim();

        if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantValue))
            return invariantValue;

        if (decimal.TryParse(cleaned, NumberStyles.Any, new CultureInfo("vi-VN"), out var viValue))
            return viValue;

        if (decimal.TryParse(cleaned, NumberStyles.Any, new CultureInfo("en-US"), out var enValue))
            return enValue;

        return null;
    }

    private static bool ParseBoolean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim().ToLowerInvariant();
        return normalized is "1" or "true" or "yes" or "x";
    }

    private static string? ExtractFirstImage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.FirstOrDefault();
    }

    private static object CreateProductLogPayload(Product product)
    {
        return new
        {
            product.Id,
            product.Sku,
            product.Name,
            product.Brand,
            product.Size,
            product.Style,
            product.Color,
            product.ParentSku,
            product.IsMaster,
            product.Status,
            product.CategoryId,
            product.AvatarUrl,
            product.Description,
            product.UpdatedAt
        };
    }

    private class ProductImportRow
    {
        public int RowNumber { get; set; }
        public string CategoryName { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Brand { get; set; }
        public decimal? SellingPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public string? Size { get; set; }
        public string? Style { get; set; }
        public string? Color { get; set; }
        public string? ParentSku { get; set; }
        public bool IsMaster { get; set; }
        public string? AvatarUrl { get; set; }
        public string Status { get; set; } = "Active";
        public string? Description { get; set; }
    }
}