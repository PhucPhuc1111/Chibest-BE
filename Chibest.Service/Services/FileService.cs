using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
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
    private readonly long _imageMaxSizeByte;
    private readonly int _compressionQuality;
    private readonly string[] _allowedImageExtensions;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly string _privateStoragePath;
    private readonly IConfiguration _configuration;// For excel mapping config

    public FileService(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IContentTypeProvider contentTypeProvider)
    {
        //Convert from MB -> Mb -> b
        _imageMaxSizeByte = (long.TryParse(Environment.GetEnvironmentVariable("Image_Max_Size_MB"), out var MB)
            ? MB : 2) * 1024 * 1024;
        _compressionQuality = int.TryParse(Environment.GetEnvironmentVariable("Image_Compressiom_Quality_Percent"), out var percent) ? percent : 75;
        _allowedImageExtensions = Regex.Split(Environment.GetEnvironmentVariable("Image_Type") ?? "", ",");
        _contentTypeProvider = contentTypeProvider;
        _configuration = configuration;

        // Get folder publish
        var contentRoot = webHostEnvironment.ContentRootPath;// Example "C:\inetpub\wwwroot\YourApiApp"

        var storageDirectory = Path.Combine(contentRoot, "..", "ChiBest_PrivateStorage");// Example "C:\inetpub\wwwroot\ChiBest_PrivateStorage"

        _privateStoragePath = Path.GetFullPath(storageDirectory);// Example "C:\inetpub\ChiBest_PrivateStorage"
        // Make sure folder exist
        if (!Directory.Exists(_privateStoragePath))
            Directory.CreateDirectory(_privateStoragePath);
    }

    //====================================================================================
    public async Task<IBusinessResult> SaveImageAsync(IFormFile imageFile, string fileName, string category)
    {
        // --- Validate input ---
        if (imageFile == null || imageFile.Length == 0)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG + " Image null");

        if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(category))
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG + " Name or Category null");
        // ----------------------

        var relativePath = Path.Combine("images", category.ToLower() + "s");
        var uploadPath = Path.Combine(_privateStoragePath, relativePath);

        // --- Validate file type ---
        if (_allowedImageExtensions.Length == 0 || _allowedImageExtensions.Any(arr => string.IsNullOrWhiteSpace(arr)))
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG + " Image types allow is null");

        var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension) || !_allowedImageExtensions.Contains(fileExtension))
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG + " Image type not allow");
        // --------------------------

        // Handle physic location
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        fileName = Path.GetFileNameWithoutExtension(fileName);
        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrEmpty(safeFileName) || safeFileName != fileName)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

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
        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG, Path.Combine(relativePath, finalFileName));
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

    public async Task<byte[]> ExportToExcelAsync<T>(
        IEnumerable<T> data,
        string mappingKey) where T : class
    {
        // 1. Đọc mapping từ config
        var headers = _configuration.GetSection($"ExcelMappings:{mappingKey}")
                                    .Get<Dictionary<string, string>>();

        if (headers == null || !headers.Any())
        {
            throw new ArgumentException($"Không tìm thấy mapping cho key: {mappingKey}");
        }

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Data");

        // 2. Lấy tên thuộc tính (tiếng Anh) theo thứ tự
        var propertyNames = headers.Keys.ToList();

        // 3. Ghi tên cột (tiếng Việt)
        for (int i = 0; i < headers.Count; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers.Values.ElementAt(i);
        }

        // ... (Định dạng header nếu muốn) ...

        // 4. Lấy PropertyInfo
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => propertyNames.Contains(p.Name))
            .ToList();

        var orderedProperties = propertyNames
            .Select(name => properties.FirstOrDefault(p => p.Name == name))
            .Where(p => p != null)
            .ToList();

        // 5. Ghi dữ liệu
        var row = 2;
        foreach (var item in data)
        {
            for (int col = 0; col < orderedProperties.Count; col++)
            {
                worksheet.Cells[row, col + 1].Value = orderedProperties[col].GetValue(item);
            }
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        return await package.GetAsByteArrayAsync();
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