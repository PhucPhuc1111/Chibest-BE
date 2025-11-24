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
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using ZXing;
using ZXing.Common;
using ImageSharpImage = SixLabors.ImageSharp.Image;

namespace Chibest.Service.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;

    public ProductService(IUnitOfWork unitOfWork, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
    }
    public async Task<IBusinessResult> GetMasterListAsync(ProductQuery query)
    {
        Expression<Func<Product, bool>> predicate = p => true;

        predicate = predicate.And(p => p.IsMaster == true);

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

        var products = await _unitOfWork.ProductRepository.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            predicate,
            orderBy,
            include: q => q.Include(p => p.ProductPriceHistories)
                          .Include(p => p.BranchStocks)
                          .Include(p => p.Category)
        );

        var masterSkus = products.Select(p => p.Sku).ToList();

        var variantCounts = await _unitOfWork.ProductRepository
            .GetAllAsync(p => masterSkus.Contains(p.ParentSku) && p.IsMaster == false)
            .ContinueWith(task => task.Result
                .GroupBy(p => p.ParentSku)
                .ToDictionary(g => g.Key, g => g.Count()));

        var response = products.Select(product =>
        {
            var latestPrice = GetLatestPriceHistory(product.ProductPriceHistories, query.BranchId);

            var branchStock = query.BranchId.HasValue
                ? product.BranchStocks?.FirstOrDefault(bs => bs.BranchId == query.BranchId.Value)
                : null;

            variantCounts.TryGetValue(product.Sku, out int childrenNo);

            return new ProductListResponse
            {
                Id = product.Id,
                AvartarUrl = product.AvatarUrl,
                Sku = product.Sku,
                Name = product.Name,
                IsMaster = product.IsMaster,
                Status = product.Status,
                ChildrenNo = childrenNo,
                CostPrice = latestPrice?.CostPrice,
                SellingPrice = latestPrice?.SellingPrice,
                StockQuantity = branchStock?.AvailableQty ?? 0
            };
        }).ToList();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetVariantsByParentSkuAsync(string parentSku, Guid? branchId = null)
    {
        if (string.IsNullOrEmpty(parentSku))
        {
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "ParentSku is required");
        }

        var variants = await _unitOfWork.ProductRepository.GetAllAsync(
            predicate: p => p.ParentSku == parentSku && p.IsMaster == false,
            include: q => q.Include(p => p.ProductPriceHistories)
                          .Include(p => p.BranchStocks)
        );

        var response = variants.Select(variant =>
        {
            var latestPrice = GetLatestPriceHistory(variant.ProductPriceHistories, branchId);

            var branchStock = branchId.HasValue
                ? variant.BranchStocks?.FirstOrDefault(bs => bs.BranchId == branchId.Value)
                : variant.BranchStocks?.FirstOrDefault();

            return new ProductListResponse
            {
                Id = variant.Id,
                AvartarUrl = variant.AvatarUrl,
                Sku = variant.Sku,
                Name = variant.Name,
                IsMaster = variant.IsMaster,
                Status = variant.Status,
                ChildrenNo = 0, 
                CostPrice = latestPrice?.CostPrice,
                SellingPrice = latestPrice?.SellingPrice,
                StockQuantity = branchStock?.AvailableQty ?? 0,
            };
        }).ToList();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
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

        // Include both ProductPriceHistories and BranchStocks
        var products = await _unitOfWork.ProductRepository.GetPagedAsync(
            query.PageNumber = 1,
            query.PageSize = 5,
            predicate,
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

            return new ProductListResponse
            {
                Id = product.Id,
                AvartarUrl = product.AvatarUrl,
                Sku = product.Sku,
                Name = product.Name,
                IsMaster = product.IsMaster,
                Status = product.Status,
                CostPrice = latestPrice?.CostPrice,
                SellingPrice = latestPrice?.SellingPrice,
                StockQuantity = branchStock?.AvailableQty ?? 0
            };
        }).ToList();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id, Guid? branchId)
    {
        var product = await _unitOfWork.ProductRepository
            .GetByWhere(p => p.Id == id)
            .Include(p => p.ProductPriceHistories)
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Include(p => p.Color)
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
            VideoUrl = product.VideoUrl,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            Color = product.Color.Code,
            Size = product.Size.Code,
            Style = product.Style,
            Material = product.Material,
            Weight = product.Weight,
            IsMaster = product.IsMaster,
            Status = product.Status,
            ParentSku = product.ParentSku,
            CategoryName = product.Category.Name,
            CostPrice = latestPrice?.CostPrice,
            SellingPrice = latestPrice?.SellingPrice,
            StockQuantity = branchStock?.AvailableQty ?? 0,
            Note = product.Note
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GenerateProductBarcodeAsync(Guid productId, Guid? branchId)
    {
        if (productId == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Id sản phẩm không hợp lệ.");

        var product = await _unitOfWork.ProductRepository
            .GetByWhere(p => p.Id == productId)
            .Include(p => p.ProductPriceHistories)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (product == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var barcodeValue = !string.IsNullOrWhiteSpace(product.BarCode)
            ? product.BarCode!
            : product.Sku;

        if (string.IsNullOrWhiteSpace(barcodeValue))
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Sản phẩm chưa được cấu hình barcode.");

        var latestPrice = GetLatestPriceHistory(product.ProductPriceHistories, branchId);

        var response = new ProductBarcodeResponse
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Barcode = barcodeValue,
            BarcodeImageBase64 = GenerateCode128Barcode(barcodeValue),
            SellingPrice = latestPrice?.SellingPrice,
            Currency = "VND"
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }
    
    public async Task<IBusinessResult> CreateAsync(ProductRequest request, Guid accountId)
    {
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        if (string.IsNullOrWhiteSpace(request.Sku) || string.IsNullOrWhiteSpace(request.Name))
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "SKU và tên sản phẩm là bắt buộc.");

        var baseSku = request.Sku.Trim();
        var baseName = request.Name.Trim();

        string? avatarUrl = request.AvatarUrl;
        string? videoUrl = request.VideoUrl;

        if (request.AvatarFile != null)
        {
            try
            {
                avatarUrl = await _fileService.SaveProductImageAsync(request.AvatarFile, baseSku);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, $"Lưu ảnh sản phẩm thất bại: {ex.Message}");
            }
        }

        if (request.VideoFile != null)
        {
            try
            {
                videoUrl = await _fileService.SaveProductVideoAsync(request.VideoFile, baseSku);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, $"Lưu video sản phẩm thất bại: {ex.Message}");
            }
        }

        request.AvatarUrl = avatarUrl;
        request.VideoUrl = videoUrl;

        var variantColorIds = request.ColorIds?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList() ?? new List<Guid>();
        var variantSizeIds = request.SizeIds?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList() ?? new List<Guid>();

        var generateVariants = variantColorIds.Any() || variantSizeIds.Any();

        List<Color> colors = new();
        List<Size> sizes = new();

        if (generateVariants)
        {
            if (variantColorIds.Any())
            {
                colors = await _unitOfWork.ColorRepository
                    .GetByWhere(c => variantColorIds.Contains(c.Id))
                    .AsNoTracking()
                    .ToListAsync();

                if (colors.Count != variantColorIds.Count)
                    return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Một hoặc nhiều màu sắc không tồn tại.");

                if (colors.Any(c => string.IsNullOrWhiteSpace(c.Code) || string.IsNullOrWhiteSpace(c.Code)))
                    return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Vui lòng cấu hình đầy đủ mã và tên cho màu sắc.");
            }

            if (variantSizeIds.Any())
            {
                sizes = await _unitOfWork.SizeRepository
                    .GetByWhere(s => variantSizeIds.Contains(s.Id))
                    .AsNoTracking()
                    .ToListAsync();

                if (sizes.Count != variantSizeIds.Count)
                    return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Một hoặc nhiều size không tồn tại.");

                if (sizes.Any(s => string.IsNullOrWhiteSpace(s.Code)))
                    return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Vui lòng cấu hình đầy đủ mã cho size.");
            }
        }

        var variantDefinitions = new List<(Color? color, Size? size)>();
        if (generateVariants)
        {
            if (colors.Any() && sizes.Any())
            {
                foreach (var color in colors)
                {
                    foreach (var size in sizes)
                    {
                        variantDefinitions.Add((color, size));
                    }
                }
            }
            else if (colors.Any())
            {
                foreach (var color in colors)
                {
                    variantDefinitions.Add((color, null));
                }
            }
            else if (sizes.Any())
            {
                foreach (var size in sizes)
                {
                    variantDefinitions.Add((null, size));
                }
            }
        }

        var candidateSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { baseSku };
        foreach (var (color, size) in variantDefinitions)
        {
            var variantSku = BuildVariantSku(baseSku, color, size);
            if (!candidateSkus.Add(variantSku))
                return new BusinessResult(Const.HTTP_STATUS_CONFLICT, $"SKU biến thể bị trùng: {variantSku}");
        }

        var duplicatedSkus = await _unitOfWork.ProductRepository
            .GetByWhere(p => candidateSkus.Contains(p.Sku))
            .Select(p => p.Sku)
            .ToListAsync();
        
        if (duplicatedSkus.Any())
            return new BusinessResult(Const.HTTP_STATUS_CONFLICT, $"SKU đã tồn tại: {string.Join(", ", duplicatedSkus)}");

        var now = DateTime.Now;
        var parentProduct = request.Adapt<Product>();
        parentProduct.Id = Guid.NewGuid();
        parentProduct.Sku = baseSku;
        parentProduct.BarCode = baseSku;
        parentProduct.Name = baseName;
        parentProduct.AvatarUrl = avatarUrl;
        parentProduct.VideoUrl = videoUrl;
        parentProduct.Status = "UnAvailable";
        parentProduct.CreatedAt = now;
        parentProduct.UpdatedAt = now;
        parentProduct.ParentSku = generateVariants ? null : request.ParentSku;
        parentProduct.IsMaster = generateVariants || request.IsMaster;

        if (generateVariants)
        {
            parentProduct.ColorId = null;
            parentProduct.SizeId = null;
        }

        await _unitOfWork.ProductRepository.AddAsync(parentProduct);

        var variants = new List<Product>();
        if (variantDefinitions.Any())
        {
            foreach (var (color, size) in variantDefinitions)
            {
                var variantSku = BuildVariantSku(baseSku, color, size);
                var variant = new Product
                {
                    Id = Guid.NewGuid(),
                    Sku = variantSku,
                    Name = BuildVariantName(baseName, color, size),
                    Description = parentProduct.Description,
                    AvatarUrl = parentProduct.AvatarUrl,
                    VideoUrl = parentProduct.VideoUrl,
                    BarCode = variantSku,
                    ColorId = color?.Id,
                    SizeId = size?.Id,
                    Style = parentProduct.Style,
                    Material = parentProduct.Material,
                    Weight = parentProduct.Weight,
                    IsMaster = false,
                    Status = parentProduct.Status,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CategoryId = parentProduct.CategoryId,
                    ParentSku = baseSku
                };

                variants.Add(variant);
            }

            await _unitOfWork.ProductRepository.AddRangeAsync(variants);
        }

        var priceHistories = new List<ProductPriceHistory>();
        if (request.SellingPrice.HasValue && request.CostPrice.HasValue)
        {
            var effectiveDate = request.EffectiveDate ?? DateTime.Now.AddDays(-1);

            priceHistories.Add(new ProductPriceHistory
            {
                Id = Guid.NewGuid(),
                ProductId = parentProduct.Id,
                BranchId = null,
                SellingPrice = request.SellingPrice.Value,
                CostPrice = request.CostPrice.Value,
                EffectiveDate = effectiveDate,
                ExpiryDate = request.ExpiryDate,
                CreatedAt = now,
                Note = "Giá khởi tạo sản phẩm"
            });

            foreach (var variant in variants)
            {
                priceHistories.Add(new ProductPriceHistory
                {
                    Id = Guid.NewGuid(),
                    ProductId = variant.Id,
                    BranchId = null,
                    SellingPrice = request.SellingPrice.Value,
                    CostPrice = request.CostPrice.Value,
                    EffectiveDate = effectiveDate,
                    ExpiryDate = request.ExpiryDate,
                    CreatedAt = now,
                    Note = "Giá khởi tạo sản phẩm"
                });
            }
        }

        if (priceHistories.Any())
            await _unitOfWork.ProductPriceHistoryRepository.AddRangeAsync(priceHistories);

        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG);

        static string BuildVariantSku(string baseSku, Color? color, Size? size)
        {
            var suffix = string.Empty;

            if (color != null)
                suffix += NormalizeCode(color.Code);

            if (size != null)
                suffix += NormalizeCode(size.Code);

            return string.IsNullOrEmpty(suffix) ? baseSku : $"{baseSku}{suffix}";
        }

        static string BuildVariantName(string baseName, Color? color, Size? size)
        {
            var segments = new List<string>();

            if (color != null)
                segments.Add(color.Code?.Trim() ?? string.Empty);

            if (size != null)
                segments.Add(size.Code?.Trim() ?? string.Empty);

            var filteredSegments = segments.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return filteredSegments.Any()
                ? $"{baseName} - {string.Join(" - ", filteredSegments)}"
                : baseName;
        }

        static string NormalizeCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Mã không hợp lệ.", nameof(code));

            return code.Trim().Replace(" ", string.Empty).ToUpperInvariant();
        }
    }

    public async Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId)
    {
        var existing = await _unitOfWork.ProductRepository.GetByIdAsync(id);
        if (existing == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var oldValue = JsonSerializer.Serialize(existing);

        _unitOfWork.ProductRepository.Delete(existing);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }

    //public async Task<IBusinessResult> ImportProductsFromExcelAsync(IFormFile file, Guid accountId)
    //{
    //    if (file == null || file.Length == 0)
    //        return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "File import không hợp lệ.");

    //    using var memoryStream = new MemoryStream();
    //    await file.CopyToAsync(memoryStream);
    //    memoryStream.Position = 0;

    //    using var workbook = new XLWorkbook(memoryStream);
    //    var worksheet = workbook.Worksheets
    //        .FirstOrDefault(ws => string.Equals(ws.Name, "ProductTemplate", StringComparison.OrdinalIgnoreCase))
    //        ?? workbook.Worksheets.FirstOrDefault();

    //    if (worksheet == null)
    //        return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không tìm thấy sheet dữ liệu trong file.");

    //    var headerRow = worksheet.FirstRowUsed();
    //    if (headerRow == null)
    //        return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "File import không có dữ liệu.");

    //    var headerLookup = headerRow.CellsUsed()
    //        .ToDictionary(cell => NormalizeHeader(cell.GetString()), cell => cell.Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);

    //    int? colCategory = GetColumn(headerLookup, "nhóm hàng");
    //    int? colSku = GetColumn(headerLookup, "mã hàng");
    //    int? colName = GetColumn(headerLookup, "tên hàng");
    //    int? colBrand = GetColumn(headerLookup, "thương hiệu");
    //    int? colSellPrice = GetColumn(headerLookup, "giá bán");
    //    int? colCostPrice = GetColumn(headerLookup, "giá vốn");
    //    int? colSize = GetColumn(headerLookup, "size");
    //    int? colStyle = GetColumn(headerLookup, "style");
    //    int? colColor = GetColumn(headerLookup, "màu");
    //    int? colParentSku = GetColumn(headerLookup, "parentsku");
    //    int? colIsMaster = GetColumn(headerLookup, "ismaster");
    //    int? colImages = GetColumn(headerLookup, "hình ảnh", "hình ảnh");
    //    int? colStatus = GetColumn(headerLookup, "đang kinh");
    //    int? colDescription = GetColumn(headerLookup, "mô tả");

    //    if (colCategory == null || colSku == null || colName == null)
    //        return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Thiếu cột bắt buộc trong file import (Nhóm hàng, Mã hàng, Tên hàng).");

    //    var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
    //    if (lastRow <= headerRow.RowNumber())
    //        return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "File import không có dữ liệu.");

    //    var categories = await _unitOfWork.CategoryRepository
    //        .GetAll()
    //        .Select(c => new { c.Id, c.Name })
    //        .ToListAsync();

    //    var categoryLookup = categories.ToDictionary(
    //        c => c.Name.Trim().ToLowerInvariant(),
    //        c => c.Id);

    //    var importRows = new List<ProductImportRow>();
    //    var seenSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    //    var errors = new List<string>();

    //    for (int rowNumber = headerRow.RowNumber() + 1; rowNumber <= lastRow; rowNumber++)
    //    {
    //        var row = worksheet.Row(rowNumber);
    //        if (!row.CellsUsed().Any())
    //            continue;

    //        var sku = GetCellString(row, colSku).Trim();
    //        if (string.IsNullOrWhiteSpace(sku))
    //        {
    //            errors.Add($"Dòng {rowNumber}: Thiếu Mã hàng.");
    //            continue;
    //        }

    //        if (!seenSkus.Add(sku))
    //        {
    //            errors.Add($"Dòng {rowNumber}: Mã hàng '{sku}' bị trùng trong file.");
    //            continue;
    //        }

    //        var categoryName = GetCellString(row, colCategory).Trim();
    //        if (string.IsNullOrWhiteSpace(categoryName))
    //        {
    //            errors.Add($"Dòng {rowNumber}: Thiếu Nhóm hàng.");
    //            continue;
    //        }

    //        var productName = GetCellString(row, colName).Trim();
    //        if (string.IsNullOrWhiteSpace(productName))
    //        {
    //            errors.Add($"Dòng {rowNumber}: Thiếu Tên hàng.");
    //            continue;
    //        }

    //        importRows.Add(new ProductImportRow
    //        {
    //            RowNumber = rowNumber,
    //            CategoryName = categoryName,
    //            Sku = sku,
    //            Name = productName,
    //            Brand = ToNullIfEmpty(GetCellString(row, colBrand)),
    //            SellingPrice = ParseDecimal(GetCellString(row, colSellPrice)),
    //            CostPrice = ParseDecimal(GetCellString(row, colCostPrice)),
    //            Size = ToNullIfEmpty(GetCellString(row, colSize)),
    //            Style = ToNullIfEmpty(GetCellString(row, colStyle)),
    //            Color = ToNullIfEmpty(GetCellString(row, colColor)),
    //            ParentSku = ToNullIfEmpty(GetCellString(row, colParentSku)),
    //            IsMaster = ParseBoolean(GetCellString(row, colIsMaster)),
    //            AvatarUrl = ExtractFirstImage(GetCellString(row, colImages)),
    //            Status = !string.IsNullOrWhiteSpace(GetCellString(row, colStatus))
    //                ? GetCellString(row, colStatus).Trim()
    //                : "Active",
    //            Description = ToNullIfEmpty(GetCellString(row, colDescription))
    //        });
    //    }

    //    if (!importRows.Any())
    //        return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không có dữ liệu hợp lệ trong file import.", errors);

    //    var skuList = importRows.Select(r => r.Sku).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    //    var existingProducts = await _unitOfWork.ProductRepository
    //        .GetByWhere(p => skuList.Contains(p.Sku))
    //        .ToListAsync();

    //    var existingLookup = existingProducts.ToDictionary(p => p.Sku, StringComparer.OrdinalIgnoreCase);
    //    var priceHistories = new List<ProductPriceHistory>();
    //    int created = 0;
    //    int updated = 0;

    //    foreach (var item in importRows)
    //    {
    //        if (!categoryLookup.TryGetValue(item.CategoryName.Trim().ToLowerInvariant(), out var categoryId))
    //        {
    //            errors.Add($"Dòng {item.RowNumber}: Không tìm thấy nhóm hàng '{item.CategoryName}'.");
    //            continue;
    //        }

    //        if (existingLookup.TryGetValue(item.Sku, out var existingProduct))
    //        {
    //            var oldSnapshot = CreateProductLogPayload(existingProduct);

    //            existingProduct.Name = item.Name;
    //            existingProduct.Brand = item.Brand;
    //            existingProduct.Size = item.Size;
    //            existingProduct.Style = item.Style;
    //            existingProduct.Color = item.Color;
    //            existingProduct.ParentSku = item.ParentSku;
    //            existingProduct.IsMaster = item.IsMaster;
    //            existingProduct.AvatarUrl = item.AvatarUrl;
    //            existingProduct.Description = item.Description;
    //            existingProduct.Status = item.Status;
    //            existingProduct.CategoryId = categoryId;
    //            existingProduct.UpdatedAt = DateTime.Now;

    //            _unitOfWork.ProductRepository.Update(existingProduct);
    //            existingLookup[item.Sku] = existingProduct;

    //            if (item.SellingPrice.HasValue && item.CostPrice.HasValue)
    //            {
    //                priceHistories.Add(new ProductPriceHistory
    //                {
    //                    Id = Guid.NewGuid(),
    //                    ProductId = existingProduct.Id,
    //                    SellingPrice = item.SellingPrice.Value,
    //                    CostPrice = item.CostPrice.Value,
    //                    EffectiveDate = DateTime.Now,
    //                    CreatedAt = DateTime.Now,
    //                    CreatedBy = accountId,
    //                    Note = "Import Excel"
    //                });
    //            }

    //            updated++;
    //        }
    //        else
    //        {
    //            var now = DateTime.Now;
    //            var newProduct = new Product
    //            {
    //                Id = Guid.NewGuid(),
    //                Sku = item.Sku,
    //                Name = item.Name,
    //                Brand = item.Brand,
    //                Size = item.Size,
    //                Style = item.Style,
    //                Color = item.Color,
    //                ParentSku = item.ParentSku,
    //                IsMaster = item.IsMaster,
    //                AvatarUrl = item.AvatarUrl,
    //                Description = item.Description,
    //                Status = item.Status,
    //                CategoryId = categoryId,
    //                Weight = 0,
    //                Material = null,
    //                CreatedAt = now,
    //                UpdatedAt = now
    //            };

    //            await _unitOfWork.ProductRepository.AddAsync(newProduct);
    //            existingLookup[item.Sku] = newProduct;

    //            if (item.SellingPrice.HasValue && item.CostPrice.HasValue)
    //            {
    //                priceHistories.Add(new ProductPriceHistory
    //                {
    //                    Id = Guid.NewGuid(),
    //                    ProductId = newProduct.Id,
    //                    SellingPrice = item.SellingPrice.Value,
    //                    CostPrice = item.CostPrice.Value,
    //                    EffectiveDate = now,
    //                    CreatedAt = now,
    //                    CreatedBy = accountId,
    //                    Note = "Import Excel"
    //                });
    //            }

    //            created++;
    //        }
    //    }

    //    if (priceHistories.Any())
    //        await _unitOfWork.ProductPriceHistoryRepository.AddRangeAsync(priceHistories);

    //    if (created > 0 || updated > 0 || priceHistories.Any())
    //        await _unitOfWork.SaveChangesAsync();

    //    var message = errors.Any()
    //        ? "Import hoàn tất với một số lỗi. Vui lòng kiểm tra chi tiết."
    //        : "Import sản phẩm thành công.";

    //    var resultPayload = new
    //    {
    //        Created = created,
    //        Updated = updated,
    //        Errors = errors
    //    };

    //    return new BusinessResult(Const.HTTP_STATUS_OK, message, resultPayload);
    //}


    private static string GenerateCode128Barcode(string content)
    {
        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Height = 90,
                Width = Math.Max(content.Length * 18, 260),
                Margin = 0,
                PureBarcode = true
            }
        };

        var pixelData = writer.Write(content.Trim());

        using var image = ImageSharpImage.LoadPixelData<Rgba32>(pixelData.Pixels, pixelData.Width, pixelData.Height);
        using var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        return $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
    }

    private static ProductPriceHistory? GetLatestPriceHistory(IEnumerable<ProductPriceHistory>? priceHistories, Guid? branchId)
    {
        if (priceHistories == null)
            return null;

        var now = DateTime.Now;

        ProductPriceHistory? SelectLatest(IEnumerable<ProductPriceHistory> source) =>
            source
                .OrderByDescending(h => h.EffectiveDate)
                .ThenByDescending(h => h.CreatedAt)
                .FirstOrDefault();

        ProductPriceHistory? SelectLatestActive(IEnumerable<ProductPriceHistory> source) =>
            SelectLatest(source.Where(h => h.EffectiveDate <= now &&
                                            (h.ExpiryDate == null || h.ExpiryDate > now)));

        if (branchId.HasValue)
        {
            var branchSpecificActive = SelectLatestActive(priceHistories.Where(h => h.BranchId == branchId.Value));
            if (branchSpecificActive != null)
                return branchSpecificActive;

            var globalActive = SelectLatestActive(priceHistories.Where(h => h.BranchId == null));
            if (globalActive != null)
                return globalActive;

            var branchSpecific = SelectLatest(priceHistories.Where(h => h.BranchId == branchId.Value));
            if (branchSpecific != null)
                return branchSpecific;
        }

        var latestGlobalActive = SelectLatestActive(priceHistories.Where(h => h.BranchId == null));
        if (latestGlobalActive != null)
            return latestGlobalActive;

        if (branchId.HasValue)
        {
            var branchFallback = SelectLatest(priceHistories.Where(h => h.BranchId == branchId.Value));
            if (branchFallback != null)
                return branchFallback;
        }

        return SelectLatest(priceHistories.Where(h => h.BranchId == null));
    }
}