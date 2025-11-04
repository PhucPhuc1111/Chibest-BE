namespace Chibest.Service.ModelDTOs.Request;

public class ExcelExportRequest
{
    /// <summary>
    /// Danh sách các cột của ProductExportView muốn export.
    /// Ví dụ: ["Name", "Sku", "Description"]
    /// </summary>
    public List<string>? ProductExportViewColumns { get; set; }
}