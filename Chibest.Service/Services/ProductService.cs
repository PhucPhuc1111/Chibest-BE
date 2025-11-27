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

        predicate = predicate.And(p => p.IsMaster);

        var searchTerm = query.SearchTerm?.Trim();
        if (!string.IsNullOrEmpty(searchTerm))
        {
            var searchLower = searchTerm.ToLowerInvariant();
            predicate = predicate.And(p =>
                p.Name.ToLower().Contains(searchLower) ||
                p.Sku.ToLower().Contains(searchLower) ||
                (p.Description != null && p.Description.ToLower().Contains(searchLower)));
        }

        if (!string.IsNullOrEmpty(query.Status))
        {
            predicate = predicate.And(p => p.Status == query.Status);
        }

        if (query.CategoryId.HasValue)
        {
            predicate = predicate.And(p => p.CategoryId == query.CategoryId.Value);
        }

        if (query.SizeId.HasValue)
        {
            predicate = predicate.And(p => p.SizeId == query.SizeId.Value);
        }

        if (query.ColorId.HasValue)
        {
            predicate = predicate.And(p => p.ColorId == query.ColorId.Value);
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

        Func<IQueryable<Product>, IQueryable<Product>> includeQuery = q =>
        {
            q = q.Include(p => p.ProductPriceHistories)
                 .Include(p => p.Category);

            q = query.BranchId.HasValue
                ? q.Include(p => p.BranchStocks.Where(bs => bs.BranchId == query.BranchId.Value))
                : q.Include(p => p.BranchStocks);

            return q;
        };

        var products = await _unitOfWork.ProductRepository.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            predicate,
            orderBy,
            include: includeQuery
        );

        var masterSkus = products
            .Select(p => p.Sku)
            .Where(sku => !string.IsNullOrWhiteSpace(sku))
            .ToList();

        Dictionary<string, List<ProductChildResponse>> childrenLookup = new();

        if (masterSkus.Any())
        {
            Func<IQueryable<Product>, IQueryable<Product>> childIncludeQuery = q =>
            {
                q = q.Include(p => p.ProductPriceHistories);
                q = query.BranchId.HasValue
                    ? q.Include(p => p.BranchStocks.Where(bs => bs.BranchId == query.BranchId.Value))
                    : q.Include(p => p.BranchStocks);
                return q;
            };

            var childProducts = await _unitOfWork.ProductRepository.GetAllAsync(
                predicate: p => !string.IsNullOrEmpty(p.ParentSku) &&
                                masterSkus.Contains(p.ParentSku) &&
                                p.IsMaster == false,
                include: childIncludeQuery
            );

            childrenLookup = childProducts
                .Where(p => !string.IsNullOrEmpty(p.ParentSku))
                .GroupBy(p => p.ParentSku!)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(child => MapChildProduct(child, query.BranchId)).ToList()
                );
        }

        var response = products.Select(product =>
        {
            var latestPrice = GetLatestPriceHistory(product.ProductPriceHistories, query.BranchId);

            var branchStock = query.BranchId.HasValue
                ? product.BranchStocks?.FirstOrDefault(bs => bs.BranchId == query.BranchId.Value)
                : null;

            childrenLookup.TryGetValue(product.Sku, out var childList);
            var children = childList ?? new List<ProductChildResponse>();
            var stockQuantity = children.Sum(c => c.StockQuantity);
            if (!children.Any())
            {
                stockQuantity = branchStock?.AvailableQty ?? 0;
            }

            return new ProductListResponse
            {
                Id = product.Id,
                AvartarUrl = product.AvatarUrl!,
                VideoUrl = product.VideoUrl!,
                Sku = product.Sku!,
                Name = product.Name!,
                IsMaster = product.IsMaster,
                Status = product.Status!,
                Description = product.Description!,
                Note = product.Note!,
                Style = product.Style!,
                Material = product.Material!,
                Weight = product.Weight,
                ChildrenNo = children.Count,
                Children = children,
                CostPrice = latestPrice?.CostPrice,
                SellingPrice = latestPrice?.SellingPrice,
                StockQuantity = stockQuantity
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

        Func<IQueryable<Product>, IQueryable<Product>> includeQuery = q =>
        {
            q = q.Include(p => p.ProductPriceHistories);
            q = branchId.HasValue
                ? q.Include(p => p.BranchStocks.Where(bs => bs.BranchId == branchId.Value))
                : q.Include(p => p.BranchStocks);
            return q;
        };

        var variants = await _unitOfWork.ProductRepository.GetAllAsync(
            predicate: p => p.ParentSku == parentSku && p.IsMaster == false,
            include: includeQuery
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
                AvartarUrl = variant.AvatarUrl!,
                VideoUrl = variant.VideoUrl!,
                Sku = variant.Sku!,
                Name = variant.Name!,
                IsMaster = variant.IsMaster,
                Status = variant.Status!,
                Description = variant.Description!,
                Note = variant.Note!,
                Style = variant.Style!,
                Material = variant.Material!,
                Weight = variant.Weight,
                ChildrenNo = 0, 
                CostPrice = latestPrice?.CostPrice,
                SellingPrice = latestPrice?.SellingPrice,
                StockQuantity = branchStock?.AvailableQty ?? 0,
            };
        }).ToList();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> UpdateProductFieldsAsync(
        Guid productId,
        IFormFile? avatarFile = null,
        IFormFile? videoFile = null,
        decimal? costPrice = null,
        decimal? sellingPrice = null,
        string? name = null,
        string? status = null,
        string? description = null)
    {
        if (productId == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Id sản phẩm không hợp lệ.");

        if (avatarFile is null && videoFile is null && costPrice is null &&
            sellingPrice is null && name is null && status is null && description is null)
        {
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không có dữ liệu để cập nhật.");
        }

        var product = await _unitOfWork.ProductRepository
            .GetByWhere(p => p.Id == productId)
            .Include(p => p.ProductPriceHistories)
            .FirstOrDefaultAsync();

        if (product == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var now = DateTime.Now;
        var productChanged = false;

        if (avatarFile != null)
        {
            string savedPath;
            try
            {
                savedPath = await _fileService.SaveProductImageAsync(avatarFile, product.Sku);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, $"Cập nhật ảnh thất bại: {ex.Message}");
            }

            if (!string.Equals(product.AvatarUrl, savedPath, StringComparison.Ordinal))
            {
                product.AvatarUrl = savedPath;
            }
            productChanged = true;
        }

        if (videoFile != null)
        {
            string savedPath;
            try
            {
                savedPath = await _fileService.SaveProductVideoAsync(videoFile, product.Sku);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, $"Cập nhật video thất bại: {ex.Message}");
            }

            if (!string.Equals(product.VideoUrl, savedPath, StringComparison.Ordinal))
            {
                product.VideoUrl = savedPath;
            }
            productChanged = true;
        }

        if (name != null && !string.Equals(product.Name, name, StringComparison.Ordinal))
        {
            product.Name = name;
            productChanged = true;
        }

        if (status != null && !string.Equals(product.Status, status, StringComparison.Ordinal))
        {
            product.Status = status;
            productChanged = true;
        }

        if (description != null && !string.Equals(product.Description, description, StringComparison.Ordinal))
        {
            product.Description = description;
            productChanged = true;
        }

        var priceChanged = false;
        if (costPrice.HasValue || sellingPrice.HasValue)
        {
            var latestPrice = GetLatestPriceHistory(product.ProductPriceHistories, null);
            var resolvedCost = costPrice ?? latestPrice?.CostPrice;
            var resolvedSell = sellingPrice ?? latestPrice?.SellingPrice;

            if (!resolvedCost.HasValue || !resolvedSell.HasValue)
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Giá bán và giá vốn phải được xác định trước khi cập nhật.");
            }

            var currentCost = latestPrice?.CostPrice;
            var currentSell = latestPrice?.SellingPrice;

            if (!currentCost.HasValue || !currentSell.HasValue ||
                currentCost.Value != resolvedCost.Value ||
                currentSell.Value != resolvedSell.Value)
            {
                priceChanged = true;
                await _unitOfWork.ProductPriceHistoryRepository.AddAsync(new ProductPriceHistory
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    CostPrice = resolvedCost.Value,
                    SellingPrice = resolvedSell.Value,
                    EffectiveDate = now,
                    CreatedAt = now,
                    Note = "Manual price update",
                    BranchId = null
                });
            }
        }

        if (!productChanged && !priceChanged)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không có thay đổi nào được áp dụng.");

        if (productChanged)
        {
            product.UpdatedAt = now;
            _unitOfWork.ProductRepository.Update(product);
        }

        await _unitOfWork.SaveChangesAsync();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
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
        Func<IQueryable<Product>, IQueryable<Product>> includeQuery = q =>
        {
            q = q.Include(p => p.ProductPriceHistories)
                 .Include(p => p.Category);

            q = query.BranchId.HasValue
                ? q.Include(p => p.BranchStocks.Where(bs => bs.BranchId == query.BranchId.Value))
                : q.Include(p => p.BranchStocks);

            return q;
        };

        var products = await _unitOfWork.ProductRepository.GetPagedAsync(
            query.PageNumber = 1,
            query.PageSize = 5,
            predicate,
            include: includeQuery
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
                AvartarUrl = product.AvatarUrl!,
                VideoUrl = product.VideoUrl!,
                Sku = product.Sku!,
                Name = product.Name!,
                IsMaster = product.IsMaster,
                Status = product.Status!,
                Description = product.Description!,
                Note = product.Note!,
                Style = product.Style!,
                Material = product.Material!,
                Weight = product.Weight,
                CostPrice = latestPrice?.CostPrice,
                SellingPrice = latestPrice?.SellingPrice,
                StockQuantity = branchStock?.AvailableQty ?? 0
            };
        }).ToList();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id, Guid? branchId)
    {
        IQueryable<Product> productQuery = _unitOfWork.ProductRepository
            .GetByWhere(p => p.Id == id)
            .Include(p => p.ProductPriceHistories)
            .Include(p => p.Category)
            .Include(p => p.Size)
            .Include(p => p.Color);

        productQuery = branchId.HasValue
            ? productQuery.Include(p => p.BranchStocks.Where(bs => bs.BranchId == branchId.Value))
            : productQuery.Include(p => p.BranchStocks);

        var product = await productQuery
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (product == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var latestPrice = GetLatestPriceHistory(product.ProductPriceHistories, branchId);

        var branchStock = branchId.HasValue
            ? product.BranchStocks?.FirstOrDefault(bs => bs.BranchId == branchId.Value)
            : product.BranchStocks?.FirstOrDefault();

        var response = new ProductResponse
        {
            Id = product.Id,
            AvartarUrl = product.AvatarUrl!,
            VideoUrl = product.VideoUrl,
            Sku = product.Sku!,
            Name = product.Name!,
            Description = product.Description!,
            Color = product.Color!.Code,
            Size = product.Size!.Code,
            Style = product.Style!,
            Material = product.Material!,
            Weight = product.Weight,
            IsMaster = product.IsMaster,
            Status = product.Status!,
            ParentSku = product.ParentSku!,
            CategoryName = product.Category!.Name,
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
        var normalizedBaseSku = baseSku.ToUpperInvariant();
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

        var candidateSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { normalizedBaseSku };
        foreach (var (color, size) in variantDefinitions)
        {
            var variantSku = BuildVariantSku(normalizedBaseSku, color, size);
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
        parentProduct.Sku = normalizedBaseSku;
        parentProduct.BarCode = normalizedBaseSku;
        parentProduct.Name = baseName;
        parentProduct.AvatarUrl = avatarUrl;
        parentProduct.VideoUrl = videoUrl;
        parentProduct.Material = request.Material;
        parentProduct.Status = (request.Status != null) ? request.Status : "NonCommercial";
        parentProduct.Style = request.Style;
        parentProduct.Weight = request.Weight;
        parentProduct.Description = request.Description;
        parentProduct.Note = request.Note;
        parentProduct.CreatedAt = now;
        parentProduct.UpdatedAt = now;
        var normalizedParentSku = request.ParentSku?.Trim().ToUpperInvariant();
        parentProduct.ParentSku = generateVariants ? null : normalizedParentSku;
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
                var variantSku = BuildVariantSku(normalizedBaseSku, color, size);
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
                    ParentSku = normalizedBaseSku
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

            baseSku = baseSku.ToUpperInvariant();

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

        static string NormalizeCode(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var firstChar = value.Trim()[0].ToString();

            firstChar = RemoveDiacritics(firstChar);

            return firstChar.ToUpperInvariant();
        }
        static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();

            foreach (var ch in normalized)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }


    }

    public async Task<IBusinessResult> DeleteAsync(IEnumerable<Guid> productIds)
    {
        if (productIds == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.FAIL_DELETE_MSG);

        var distinctIds = productIds.Distinct().ToList();
        if (!distinctIds.Any())
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.FAIL_DELETE_MSG);

        var products = await _unitOfWork.ProductRepository
            .GetAllAsync(predicate: product => distinctIds.Contains(product.Id));

        var productsToDelete = products.ToList();
        if (!productsToDelete.Any())
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var masterSkus = productsToDelete
            .Where(p => p.IsMaster && !string.IsNullOrWhiteSpace(p.Sku))
            .Select(p => p.Sku.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        if (masterSkus.Any())
        {
            var childProducts = await _unitOfWork.ProductRepository
                .GetByWhere(p => p.ParentSku != null && masterSkus.Contains(p.ParentSku!.ToUpper()))
                .ToListAsync();

            if (childProducts.Any())
            {
                var existingIds = new HashSet<Guid>(productsToDelete.Select(p => p.Id));
                foreach (var child in childProducts)
                {
                    if (existingIds.Add(child.Id))
                    {
                        productsToDelete.Add(child);
                    }
                }
            }
        }

        var assetPaths = productsToDelete
            .SelectMany(p => new[] { p.AvatarUrl, p.VideoUrl })
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var assetPath in assetPaths)
        {
            _fileService.DeletePrivateFile(assetPath);
        }

        var oldValue = JsonSerializer.Serialize(productsToDelete);

        await _unitOfWork.ProductRepository.DeleteRangeAsync(productsToDelete);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }

    private static ProductChildResponse MapChildProduct(Product product, Guid? branchId)
    {
        var latestPrice = GetLatestPriceHistory(product.ProductPriceHistories, branchId);

        var branchStock = branchId.HasValue
            ? product.BranchStocks?.FirstOrDefault(bs => bs.BranchId == branchId.Value)
            : null;

        return new ProductChildResponse
        {
            Id = product.Id,
            AvartarUrl = product.AvatarUrl ?? string.Empty,
            VideoUrl = product.VideoUrl ?? string.Empty,
            Sku = product.Sku,
            Name = product.Name,
            Status = product.Status,
            Description = product.Description!,
            Note = product.Note!,
            Style = product.Style!,
            Material = product.Material!,
            Weight = product.Weight,
            ColorId = product.ColorId,
            SizeId = product.SizeId,
            CostPrice = latestPrice?.CostPrice,
            SellingPrice = latestPrice?.SellingPrice,
            StockQuantity = branchStock?.AvailableQty ?? 0
        };
    }

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

        var materialized = priceHistories as IList<ProductPriceHistory> ?? priceHistories.ToList();
        if (materialized.Count == 0)
            return null;

        var now = DateTime.Now;
        var ordered = materialized
            .OrderByDescending(h => h.EffectiveDate)
            .ThenByDescending(h => h.CreatedAt)
            .ToList();

        ProductPriceHistory? Pick(Func<ProductPriceHistory, bool> predicate, bool activeOnly)
        {
            var filtered = ordered.Where(predicate);
            if (activeOnly)
            {
                filtered = filtered.Where(h => h.EffectiveDate <= now && (h.ExpiryDate == null || h.ExpiryDate > now));
            }

            return filtered.FirstOrDefault();
        }

        if (branchId.HasValue)
        {
            var branchIdValue = branchId.Value;
            var branchActive = Pick(h => h.BranchId == branchIdValue, true);
            if (branchActive != null)
                return branchActive;

            var globalActive = Pick(h => h.BranchId == null, true);
            if (globalActive != null)
                return globalActive;

            var branchAny = Pick(h => h.BranchId == branchIdValue, false);
            if (branchAny != null)
                return branchAny;
        }

        return Pick(h => h.BranchId == null, true) ?? Pick(h => h.BranchId == null, false);
    }
}