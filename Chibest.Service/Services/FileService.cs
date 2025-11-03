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
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Chibest.Service.Services;

public class FileService : IFileService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly long _imageMaxSizeByte;
    private readonly int _compressionQuality;
    private readonly string[] _allowedImageExtensions;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly string _privateStoragePath;
    private readonly IConfiguration _configuration;// For excel mapping config
    private readonly Dictionary<string, Func<ProductExportView, object?>> _columnMap;// For excel mapping config

    public FileService(IUnitOfWork unitOfWork,IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IContentTypeProvider contentTypeProvider)
    {
        _unitOfWork = unitOfWork;
        //Convert from MB -> Mb -> b
        _imageMaxSizeByte = (long.TryParse(Environment.GetEnvironmentVariable("Image_Max_Size_MB"), out var MB)
            ? MB : 2) * 1024 * 1024;
        _compressionQuality = int.TryParse(Environment.GetEnvironmentVariable("Image_Compressiom_Quality_Percent"), out var percent) ? percent : 75;
        
        var rawExtensions = Environment.GetEnvironmentVariable("Image_Type") ?? "jpg,png";
        // Xử lý chuỗi này để đảm bảo mọi phần tử đều có dấu "."
        _allowedImageExtensions = Regex.Split(rawExtensions, ",")
            .Select(ext => ext.Trim().ToLowerInvariant()) // 1. Dọn dẹp: cắt khoảng trắng, chuyển chữ thường
            .Where(ext => !string.IsNullOrWhiteSpace(ext)) // 2. Loại bỏ các chuỗi rỗng (nếu ENV là "png,,jpg")
            .Select(ext => ext.StartsWith(".") ? ext : "." + ext) // 3. Thêm dấu "." nếu chưa có
            .ToArray();
        _contentTypeProvider = contentTypeProvider;
        _configuration = configuration;

        // Get folder publish
        var contentRoot = webHostEnvironment.ContentRootPath;// Example "C:\inetpub\wwwroot\YourApiApp"

        var storageDirectory = Path.Combine(contentRoot, "..", "ChiBest_PrivateStorage");// Example "C:\inetpub\wwwroot\ChiBest_PrivateStorage"

        _privateStoragePath = Path.GetFullPath(storageDirectory);// Example "C:\inetpub\ChiBest_PrivateStorage"
        // Make sure folder exist
        if (!Directory.Exists(_privateStoragePath))
            Directory.CreateDirectory(_privateStoragePath);

        _columnMap = new Dictionary<string, Func<ProductExportView, object?>>(StringComparer.OrdinalIgnoreCase)
            {
            // Product columns
            { "ProductId", p => p.Id },
            { "Sku", p => p.Sku },
            { "Name", p => p.Name },
            { "Description", p => p.Description },
            // ... Thêm các cột Product khác ...

            // PriceHistory columns
            { "SellingPrice", p => p.SellingPrice },
            { "CostPrice", p => p.CostPrice },
            { "PriceEffectiveDate", p => p.EffectiveDate }
            // ... Thêm các cột Price khác ...
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

    public (Stream FileStream, string ContentType) GetImageFileAsync(string relativePath)
    {
        // relativePath VD: /images/avatars/file.png
        var physicalFilePath = Path.Combine(_privateStoragePath, relativePath);

        // Find file
        if (!System.IO.File.Exists(physicalFilePath))
            throw new ArgumentException("Tên file không hợp lệ.");

        // Get ContentType (MIME type)
        if (!_contentTypeProvider.TryGetContentType(physicalFilePath, out var contentType))
        {
            // Default binary stream type
            contentType = "application/octet-stream";
        }

        // Create file stream
        Stream fileStream = new FileStream(physicalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return (fileStream, contentType);
    }

    public async Task<byte[]> ExportProductsToExcelAsync(ExcelExportRequest request)
    {
        // 1. Lấy dữ liệu "phẳng" từ database
        var productData = await GetFlatProductDataAsync();

        // 2. Xác định các cột sẽ được export dựa trên request
        var defaultProductCols = new List<string> { "Sku", "Name", "SellingPrice" };

        // Nếu request.ProductColumns là null/rỗng, dùng list mặc định
        var productCols = (request.ProductColumns != null && request.ProductColumns.Any())
            ? request.ProductColumns
            : defaultProductCols;

        // Nếu request.CurrentPriceColumns được cung cấp, hãy thêm chúng vào
        var priceCols = request.CurrentPriceColumns ?? new List<string>();

        // Gộp lại và lọc ra những cột có tồn tại trong _columnMap
        var headers = productCols.Concat(priceCols)
                                 .Distinct(StringComparer.OrdinalIgnoreCase)
                                 .Where(c => _columnMap.ContainsKey(c))
                                 .ToList();

        // 3. Tạo file Excel
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Products");

            // 3.1 Ghi Headers
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            // 3.2 Ghi Dữ liệu
            for (int row = 0; row < productData.Count; row++)
            {
                var item = productData[row];
                for (int col = 0; col < headers.Count; col++)
                {
                    // Lấy tên cột từ header
                    string header = headers[col];

                    // Dùng _columnMap để lấy giá trị tương ứng từ item
                    var value = _columnMap[header](item);

                    // Ghi vào cell (row + 2 vì header ở dòng 1)
                    worksheet.Cell(row + 2, col + 1).Value = XLCellValue.FromObject(value);
                }
            }

            // 3.3 Tự động căn chỉnh cột
            worksheet.Columns().AdjustToContents();

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
        // Giả định bạn có ProductRepository trong UnitOfWork
        var query = _unitOfWork.ProductRepository.GetAll(); // IQueryable<Product>

        // Đây là logic quan trọng:
        // 1. Select vào ProductExportViewModel
        // 2. Dùng sub-query để lấy giá hiện tại (ExpiryDate == null)
        // 3. EF Core đủ thông minh để dịch query này thành SQL (LEFT JOIN)

        var flatQuery = query.Select(p => new ProductExportView
        {
            // Map trường của Product
            Id = p.Id,
            Sku = p.Sku,
            Name = p.Name,
            Description = p.Description,
            // ...

            // Map trường của giá hiện tại (dùng sub-query)
            // Lấy giá có ExpiryDate == null, và là giá mới nhất
            SellingPrice = p.ProductPriceHistories
                            .Where(h => h.ExpiryDate == null)
                            .OrderByDescending(h => h.EffectiveDate)
                            .Select(h => (decimal?)h.SellingPrice) // Cast sang nullable
                            .FirstOrDefault(),

            CostPrice = p.ProductPriceHistories
                         .Where(h => h.ExpiryDate == null)
                         .OrderByDescending(h => h.EffectiveDate)
                         .Select(h => (decimal?)h.CostPrice)
                         .FirstOrDefault(),

            EffectiveDate = p.ProductPriceHistories
                                  .Where(h => h.ExpiryDate == null)
                                  .OrderByDescending(h => h.EffectiveDate)
                                  .Select(h => (DateTime?)h.EffectiveDate)
                                  .FirstOrDefault()
        });

        return await flatQuery.ToListAsync();
    }

    public async Task<List<T>> ImportFromExcelAsync<T>(
        IFormFile file,
        string mappingKey) where T : class, new()
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Không có file nào được tải lên.");
        }

        // 1. Lấy mapping (Eng -> Vie) từ config
        var exportMap = _configuration.GetSection($"ExcelMappings:{mappingKey}")
                                      .Get<Dictionary<string, string>>();
        if (exportMap == null)
        {
            throw new ArgumentException($"Không tìm thấy mapping cho key: {mappingKey}");
        }

        // 2. Đảo ngược mapping (Vie -> Eng) để đọc
        // Dùng StringComparer.OrdinalIgnoreCase để không phân biệt hoa thường
        var importMap = exportMap.ToDictionary(
            kvp => kvp.Value, // Key: Tên tiếng Việt
            kvp => kvp.Key,   // Value: Tên thuộc tính tiếng Anh
            StringComparer.OrdinalIgnoreCase
        );

        var list = new List<T>();
        // Lấy tất cả thuộc tính của Model T (ví dụ: Account)
        var properties = typeof(T).GetProperties().ToDictionary(p => p.Name, p => p);

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) throw new ArgumentException("Worksheet rỗng.");

                var rowCount = worksheet.Dimension.Rows;
                var colCount = worksheet.Dimension.Columns;

                // 3. Xây dựng map cột (Column Index -> PropertyInfo)
                // Đọc hàng 1 (hàng tiêu đề)
                var columnMap = new Dictionary<int, PropertyInfo>();
                for (int col = 1; col <= colCount; col++)
                {
                    var headerText = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                    if (headerText == null) continue;

                    // Dùng map (Vie -> Eng) để tìm tên thuộc tính (tiếng Anh)
                    if (importMap.TryGetValue(headerText, out var propertyName))
                    {
                        // Lấy PropertyInfo của thuộc tính đó
                        if (properties.TryGetValue(propertyName, out var propertyInfo))
                        {
                            columnMap[col] = propertyInfo;
                        }
                    }
                }

                if (!columnMap.Any())
                {
                    throw new ArgumentException("Tiêu đề file Excel không khớp với bất kỳ mapping nào.");
                }

                // 4. Đọc dữ liệu từ hàng 2
                for (int row = 2; row <= rowCount; row++)
                {
                    var newItem = new T(); // Yêu cầu 'where T : new()'
                    bool rowHasData = false;

                    foreach (var (colIndex, propInfo) in columnMap)
                    {
                        var cellValue = worksheet.Cells[row, colIndex].Value;
                        if (cellValue != null)
                        {
                            rowHasData = true;
                            try
                            {
                                // Chuyển đổi kiểu dữ liệu an toàn
                                var safeValue = ConvertToPropertyType(cellValue, propInfo.PropertyType);
                                propInfo.SetValue(newItem, safeValue);
                            }
                            catch (Exception ex)
                            {
                                // Bạn có thể log lỗi ở đây:
                                // Log($"Lỗi convert hàng {row}, cột {colIndex}: {ex.Message}");
                            }
                        }
                    }

                    if (rowHasData) // Chỉ thêm nếu hàng có dữ liệu
                    {
                        list.Add(newItem);
                    }
                }
            }
        }
        return list;
    }
    private object ConvertToPropertyType(object value, Type propertyType)
    {
        // Xử lý kiểu Nullable (ví dụ: int?, DateTime?)
        var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (value == null || value == DBNull.Value)
        {
            return null;
        }

        // Xử lý trường hợp đặc biệt: Excel lưu ngày tháng dưới dạng double (OADate)
        if (targetType == typeof(DateTime) && value is double doubleValue)
        {
            return DateTime.FromOADate(doubleValue);
        }

        // Chuyển đổi chung
        // Dùng TypeConverter để xử lý tốt hơn (ví dụ: string "true" -> bool true)
        var converter = TypeDescriptor.GetConverter(targetType);
        if (converter.CanConvertFrom(value.GetType()))
        {
            return converter.ConvertFrom(value);
        }

        // Chuyển đổi cơ bản
        return Convert.ChangeType(value, targetType);
    }
}