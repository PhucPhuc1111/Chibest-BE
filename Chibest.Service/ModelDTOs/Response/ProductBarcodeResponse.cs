using System;

namespace Chibest.Service.ModelDTOs.Response;

public class ProductBarcodeResponse
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Barcode { get; set; } = null!;
    public string BarcodeImageBase64 { get; set; } = null!;
    public decimal? SellingPrice { get; set; }
    public string Currency { get; set; } = "VND";
}

