namespace Chibest.Service.ModelDTOs.Response;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string AvartarUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Color { get; set; }
    public string Size { get; set; }
    public string Style { get; set; }
    public string Material { get; set; }
    public decimal? Weight { get; set; }
    public bool IsMaster { get; set; }
    public string Status { get; set; }
    public string CategoryName { get; set; }
    public string ParentSku { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal StockQuantity { get; set; }
    public string? Note { get; set; }
}

public class ProductListResponse
{
    public Guid Id { get; set; }
    public string AvartarUrl { get; set; }
    public string VideoUrl { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public bool IsMaster { get; set; }
    public string Status { get; set; }
    public int ChildrenNo { get; set; }
    public List<ProductChildResponse> Children { get; set; } = new();
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal StockQuantity { get; set; }
}

public class ProductChildResponse
{
    public Guid Id { get; set; }
    public string AvartarUrl { get; set; } = string.Empty;
    public string VideoUrl { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal StockQuantity { get; set; }
}

