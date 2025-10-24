using Chibest.Repository.Models;

namespace Chibest.Service.ModelDTOs.Request;
public class ProductPriceHistoryRequest
{
    public Guid Id { get; set; }

    public decimal SellingPrice { get; set; }

    public DateTime EffectiveDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid ProductId { get; set; }

    public Guid? BranchId { get; set; }
}