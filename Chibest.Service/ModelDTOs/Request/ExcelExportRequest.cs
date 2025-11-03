namespace Chibest.Service.ModelDTOs.Request;

public class ExcelExportRequest
{
    /// <summary>
    /// Danh sách các cột của Product muốn export.
    /// Ví dụ: ["Name", "Sku", "Description"]
    /// Nếu null hoặc rỗng, sẽ dùng cột mặc định.
    /// </summary>
    public List<string>? ProductColumns { get; set; }

    /// <summary>
    /// Danh sách các cột từ giá hiện tại (ProductPriceHistory) muốn export.
    /// Ví dụ: ["SellingPrice", "CostPrice"]
    /// Nếu null hoặc rỗng, sẽ không kèm theo giá.
    /// </summary>
    public List<string>? CurrentPriceColumns { get; set; }
}