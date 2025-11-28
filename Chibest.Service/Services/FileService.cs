using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Response;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Chibest.Service.Services;

public class FileService : IFileService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly long _imageMaxSizeByte;
    private readonly long _videoMaxSizeByte;
    private readonly int _compressionQuality;
    private readonly string[] _allowedImageExtensions;
    private readonly string[] _allowedVideoExtensions;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly string _privateStoragePath;
    private readonly IConfiguration _configuration;// For excel mapping config
    private readonly Dictionary<string, Func<ProductExportView, object?>> _columnMap;// For excel mapping config
    private static readonly string[] DefaultImageExtensions = new[]
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".heic", ".heif"
    };
    private static readonly string[] DefaultVideoExtensions = new[]
    {
        ".mp4", ".mov", ".mkv", ".avi", ".webm", ".m4v", ".wmv", ".flv", ".3gp", ".mpeg", ".mpg"
    };

    public FileService(IUnitOfWork unitOfWork, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IContentTypeProvider contentTypeProvider)
    {
        _unitOfWork = unitOfWork;
        //Convert from MB -> Mb -> b
        _imageMaxSizeByte = (long.TryParse(Environment.GetEnvironmentVariable("Image_Max_Size_MB"), out var MB)
            ? MB : 2) * 1024 * 1024;
        _compressionQuality = int.TryParse(Environment.GetEnvironmentVariable("Image_Compressiom_Quality_Percent"), out var percent) ? percent : 75;
        _videoMaxSizeByte = (long.TryParse(Environment.GetEnvironmentVariable("Video_Max_Size_MB"), out var videoMB)
            ? videoMB : 1024) * 1024 * 1024; //1,048,576

        _allowedImageExtensions = BuildExtensionList(Environment.GetEnvironmentVariable("Image_Type"), DefaultImageExtensions);
        _allowedVideoExtensions = BuildExtensionList(Environment.GetEnvironmentVariable("Video_Type"), DefaultVideoExtensions);
        _contentTypeProvider = contentTypeProvider;

        // Get folder publish
        var contentRoot = webHostEnvironment.ContentRootPath;// Example "C:\inetpub\wwwroot\YourApiApp"

        var storageDirectory = Path.Combine(contentRoot, "..", "ChiBest_PrivateStorage");// Example "C:\inetpub\wwwroot\ChiBest_PrivateStorage"

        _privateStoragePath = Path.GetFullPath(storageDirectory);// Example "C:\inetpub\ChiBest_PrivateStorage"
        // Make sure folder exist
        if (!Directory.Exists(_privateStoragePath))
            Directory.CreateDirectory(_privateStoragePath);

        // ====================================================================================
        // ================================[ For excel mapping ]===============================
        _configuration = configuration;

        //
        _columnMap = new Dictionary<string, Func<ProductExportView, object?>>(StringComparer.OrdinalIgnoreCase)
        {
            // Product columns
            { "Id", p => p.Id },
            { "Sku", p => p.Sku },
            { "Name", p => p.Name },
            { "Description", p => p.Description },
            { "AvatarUrl", p => p.AvatarUrl },
            { "Color", p => p.Color },
            { "Size", p => p.Size },
            { "Style", p => p.Style },
            { "Brand", p => p.Brand },
            { "Material", p => p.Material },
            { "Weight", p => p.Weight },
            { "IsMaster", p => p.IsMaster },
            { "Status", p => p.Status },
            { "CreatedAt", p => p.CreatedAt },
            { "UpdatedAt", p => p.UpdatedAt },
            { "CategoryId", p => p.CategoryId },
            { "ParentSku", p => p.ParentSku },

            // PriceHistory columns
            { "SellingPrice", p => p.SellingPrice },
            { "CostPrice", p => p.CostPrice },
            { "EffectiveDate", p => p.EffectiveDate },
            { "ExpiryDate", p => p.ExpiryDate },
            { "Note", p => p.Note },
            { "CreatedBy", p => p.CreatedBy },
            { "BranchId", p => p.BranchId },


            // Category Info columns
            { "CategoryName", p => p.CategoryName },
        };
    }

    //====================================================================================
    public async Task<string> SaveImageAsync(IFormFile imageFile, string fileName, string category)
    {
        // --- Validate input ---
        if (imageFile == null || imageFile.Length == 0)
            throw new Exception("Image null");

        if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(category))
            throw new Exception("Name or Category null");
        // ----------------------

        var relativePath = Path.Combine("images", category.ToLower() + "s");
        var uploadPath = Path.Combine(_privateStoragePath, relativePath);

        // --- Validate file type ---
        if (_allowedImageExtensions.Length == 0 || _allowedImageExtensions.Any(arr => string.IsNullOrWhiteSpace(arr)))
            throw new Exception("Image types allow is null");

        var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension) || !_allowedImageExtensions.Contains(fileExtension))
            throw new Exception("Image type not allow");
        // --------------------------

        // Handle physic location
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        fileName = Path.GetFileNameWithoutExtension(fileName);
        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrEmpty(safeFileName) || safeFileName != fileName)
            throw new Exception("File name contain invalid character");

        string finalFileName;
        string physicalFilePath;

        // Handle Size
        if (imageFile.Length <= _imageMaxSizeByte)// Valid Size -> Override if exist
        {
            finalFileName = $"{fileName}{fileExtension}";
            physicalFilePath = Path.Combine(uploadPath, finalFileName);

            using (var fileStream = new FileStream(physicalFilePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
        }
        else// File size larger than allow
        {
            // Use .jpg for make sure compress size
            finalFileName = $"{fileName}.jpg";
            physicalFilePath = Path.Combine(uploadPath, finalFileName);

            // use ImageSharp for compress
            using (var image = await Image.LoadAsync(imageFile.OpenReadStream()))
            {
                // (Tùy chọn) có thể giới hạn kích thước tối đa ở đây nếu muốn
                // image.Mutate(x => x.Resize(new ResizeOptions
                // {
                //     Mode = ResizeMode.Max,
                //     Size = new Size(1920, 1080) // Ví dụ: giới hạn tối đa 1920x1080
                // }));

                var encoder = new JpegEncoder
                {
                    Quality = _compressionQuality
                };

                using (var fileStream = new FileStream(physicalFilePath, FileMode.Create))
                {
                    await image.SaveAsJpegAsync(fileStream, encoder);
                }
            }
        }
        // if return path error: $"{relativePath}/{finalFileName}";
        return Path.Combine(relativePath, finalFileName);
    }

    public async Task<string> SaveProductImageAsync(IFormFile imageFile, string sku)
    {
        if (imageFile == null || imageFile.Length == 0)
            throw new Exception("Image null");

        if (string.IsNullOrWhiteSpace(sku))
            throw new Exception("SKU null");

        if (_allowedImageExtensions.Length == 0 || _allowedImageExtensions.Any(arr => string.IsNullOrWhiteSpace(arr)))
            throw new Exception("Image types allow is null");

        var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension) || !_allowedImageExtensions.Contains(fileExtension))
            throw new Exception("Image type not allow");

        var safeSku = NormalizeFileSegment(sku);
        var relativeFolder = "ProductImages";
        var uploadPath = Path.Combine(_privateStoragePath, relativeFolder);

        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        var relativePath = Path.Combine(relativeFolder, $"{safeSku}.png");
        var physicalFilePath = Path.Combine(uploadPath, $"{safeSku}.png");

        using var imageStream = imageFile.OpenReadStream();
        using var image = await Image.LoadAsync(imageStream);

        var encoder = new PngEncoder
        {
            CompressionLevel = PngCompressionLevel.Level6
        };

        using (var fileStream = new FileStream(physicalFilePath, FileMode.Create))
        {
            await image.SaveAsPngAsync(fileStream, encoder);
        }

        return relativePath;
    }

    public async Task<string> SaveProductVideoAsync(IFormFile videoFile, string sku)
    {
        if (videoFile == null || videoFile.Length == 0)
            throw new Exception("Video null");

        if (string.IsNullOrWhiteSpace(sku))
            throw new Exception("SKU null");

        if (videoFile.Length > _videoMaxSizeByte)
            throw new Exception("Video vượt quá dung lượng cho phép (1GB).");

        var fileExtension = Path.GetExtension(videoFile.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension) || !_allowedVideoExtensions.Contains(fileExtension))
            throw new Exception("Video type not allow");

        var safeSku = NormalizeFileSegment(sku);
        var relativeFolder = "ProductVideos";
        var uploadPath = Path.Combine(_privateStoragePath, relativeFolder);

        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        var relativePath = Path.Combine(relativeFolder, $"{safeSku}{fileExtension}");
        var physicalFilePath = Path.Combine(uploadPath, $"{safeSku}{fileExtension}");

        using (var fileStream = new FileStream(physicalFilePath, FileMode.Create))
        {
            await videoFile.CopyToAsync(fileStream);
        }

        return relativePath;
    }

    public (Stream FileStream, string ContentType) GetImageFile(string relativePath)
    {
        const string fallbackRelativePath = "images/noimage.png";

        string Normalize(string? path) =>
            string.IsNullOrWhiteSpace(path)
                ? fallbackRelativePath
                : path.Replace('\\', '/').TrimStart('/', '\\');

        var normalizedPath = Normalize(relativePath);
        var physicalFilePath = Path.Combine(_privateStoragePath, normalizedPath);

        if (!File.Exists(physicalFilePath))
        {
            normalizedPath = fallbackRelativePath;
            physicalFilePath = Path.Combine(_privateStoragePath, normalizedPath);

        }

        if (!_contentTypeProvider.TryGetContentType(physicalFilePath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        Stream fileStream = new FileStream(physicalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return (fileStream, contentType);
    }

    public (Stream FileStream, string ContentType, long FileLength) GetVideoFile(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("Đường dẫn file là bắt buộc.", nameof(relativePath));

        var physicalFilePath = Path.Combine(_privateStoragePath, relativePath);
        if (!System.IO.File.Exists(physicalFilePath))
            throw new ArgumentException("Tên file không hợp lệ.");

        if (!_contentTypeProvider.TryGetContentType(physicalFilePath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        var fileInfo = new FileInfo(physicalFilePath);
        var fileStream = new FileStream(physicalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return (fileStream, contentType, fileInfo.Length);
    }

    public void DeletePrivateFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        var normalized = relativePath
            .Replace('\\', '/')
            .TrimStart('/', '\\');

        if (string.IsNullOrWhiteSpace(normalized))
            return;

        var fullPath = Path.GetFullPath(Path.Combine(_privateStoragePath, normalized));

        if (!fullPath.StartsWith(_privateStoragePath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid storage path.");

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    public async Task<byte[]> ExportProductsToExcelAsync(ExcelExportRequest request)
    {
        // 1. Get data from database
        var productData = await GetFlatProductDataAsync();

        // 2. Default columns
        var defaultProductCols = new List<string> { "Sku", "Name", "SellingPrice", "CostPrice" };

        // Nếu request.ProductColumns là null/rỗng => dùng list mặc định
        var requestCols = (request.ProductExportViewColumns != null && request.ProductExportViewColumns.Any())
            ? request.ProductExportViewColumns
            : defaultProductCols;

        // Lọc ra những cột có tồn tại trong _columnMap
        var headers = requestCols.Distinct(StringComparer.OrdinalIgnoreCase)
                                 .Where(c => _columnMap.ContainsKey(c))
                                 .ToList();

        // 3. Tạo file Excel
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Products");

            // --- TÌM CỘT AVATAR ĐỂ SET CHIỀU RỘNG ---
            int avatarColIndex = headers.FindIndex(
                h => h.Equals("AvatarUrl", StringComparison.OrdinalIgnoreCase));

            if (avatarColIndex != -1)
                // Set chiều rộng cột ảnh (ví dụ: 15)
                worksheet.Column(avatarColIndex + 1).Width = 15;

            // 3.1 Ghi Headers
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            }

            // 3.2 Ghi Dữ liệu
            for (int row = 0; row < productData.Count; row++)
            {
                var item = productData[row];

                // --- SET CHIỀU CAO HÀNG NẾU CÓ ẢNH ---
                if (avatarColIndex != -1 && !string.IsNullOrWhiteSpace(item.AvatarUrl))
                {
                    // Set chiều cao hàng (ví dụ: 75)
                    worksheet.Row(row + 2).Height = 75;
                }

                for (int col = 0; col < headers.Count; col++)
                {
                    var cell = worksheet.Cell(row + 2, col + 1);

                    // Lấy tên cột từ header
                    string header = headers[col];

                    // --- LOGIC XỬ LÝ HÌNH ẢNH ---
                    if (col == avatarColIndex && !string.IsNullOrWhiteSpace(item.AvatarUrl))
                    {
                        try
                        {
                            // Gọi hàm helper của bạn để lấy stream
                            var (fileStream, contentType) = GetImageFile(item.AvatarUrl);

                            using (fileStream)
                            {
                                // Thêm ảnh vào worksheet
                                var picture = worksheet.AddPicture(fileStream);

                                // Di chuyển ảnh vào cell và set kích thước
                                picture.MoveTo(cell);
                                picture.Width = 90; // Kích thước cố định
                                picture.Height = 90;
                            }
                        }
                        catch (Exception)
                        {
                            // Nếu file không tồn tại hoặc lỗi, ghi text
                            cell.Value = "Lỗi tạo insert ảnh phía server";
                            cell.Style.Font.FontColor = XLColor.Red;
                        }
                    }
                    else
                    {
                        // --- Ghi dữ liệu text/số bình thường ---
                        var value = _columnMap[header](item);
                        cell.Value = XLCellValue.FromObject(value);
                    }
                }
            }

            // 3.3 Tự động căn chỉnh các cột *không phải* cột ảnh
            if (avatarColIndex != -1)
            {
                worksheet.Columns(1, avatarColIndex).AdjustToContents();
                worksheet.Columns(avatarColIndex + 2, headers.Count).AdjustToContents();
            }
            else
            {
                worksheet.Columns().AdjustToContents();
            }

            // 3.4 Lưu ra MemoryStream và trả về byte[]
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }

    private async Task<List<ProductExportView>> GetFlatProductDataAsync()
    {
        var query = _unitOfWork.ProductRepository.GetAll();

        // Giả định:
        // 1. Entity 'Product' (p) có các trường: Id, Sku, Name, Description, 
        //    AvatarUrl, Color, Size, Style, Brand, Material, Weight, IsMaster, 
        //    Status (enum), CreatedAt, UpdatedAt, CategoryId.
        // 2. 'Product' có navigation 'Parent' (để lấy ParentSku).
        // 3. 'Product' có navigation 'ProductPriceHistories'.

        var flatQuery = query
        // Select query usage
        .Select(pro => new
        {
            Product = pro,
            ProductCategory = pro.Category,
            ProductPrice = pro.ProductPriceHistories
                            .Where(h => h.ExpiryDate == null)
                            .OrderByDescending(h => h.EffectiveDate)
                            .FirstOrDefault()
        })
        // Map to ProductExportView
        .Select(que => new ProductExportView
        {
            // --- Map trường Product ---
            Id = que.Product.Id,
            Sku = que.Product.Sku,
            Name = que.Product.Name,
            Description = que.Product.Description,
            AvatarUrl = que.Product.AvatarUrl, // Lấy URL (đường dẫn tương đối)
            Color = que.Product.Color != null ? que.Product.Color.Code : null,
            Size = que.Product.Size != null ? que.Product.Size.Code : null,
            Style = que.Product.Style,
            Material = que.Product.Material,
            Weight = que.Product.Weight,
            IsMaster = que.Product.IsMaster,
            Status = que.Product.Status,
            CreatedAt = que.Product.CreatedAt,
            UpdatedAt = que.Product.UpdatedAt,
            CategoryId = que.Product.CategoryId,
            ParentSku = que.Product.ParentSku,

            // Map trường Product Price (dùng sub-query)
            // Lấy giá có ExpiryDate == null, và là giá mới nhất
            SellingPrice = que.ProductPrice != null ?
                que.ProductPrice.SellingPrice : null,

            CostPrice = que.ProductPrice != null ?
                que.ProductPrice.CostPrice : null,

            EffectiveDate = que.ProductPrice != null ?
                que.ProductPrice.EffectiveDate : null,

            ExpiryDate = que.ProductPrice != null ?
                que.ProductPrice.ExpiryDate : null,

            Note = que.ProductPrice != null ?
                que.ProductPrice.Note : null,

            BranchId = que.ProductPrice != null ?
                que.ProductPrice.BranchId : null,

            // Map trường Product Category (dùng sub-query)
            CategoryName = que.ProductCategory != null ?
                que.ProductCategory.Name : null,
        });

        return await flatQuery.ToListAsync();
    }

    private static string[] BuildExtensionList(string? envValue, string[] defaults)
    {
        IEnumerable<string> Parse(string? source) =>
            string.IsNullOrWhiteSpace(source)
                ? Array.Empty<string>()
                : Regex.Split(source, ",")
                    .Select(ext => ext.Trim().ToLowerInvariant())
                    .Where(ext => !string.IsNullOrWhiteSpace(ext));

        return defaults
            .Concat(Parse(envValue))
            .Select(ext => ext.StartsWith(".") ? ext : "." + ext)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string NormalizeFileSegment(string value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new Exception("File name contain invalid character");

        if (trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new Exception("File name contain invalid character");

        return trimmed;
    }
}