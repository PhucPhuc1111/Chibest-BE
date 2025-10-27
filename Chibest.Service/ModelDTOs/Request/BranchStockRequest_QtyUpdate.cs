namespace Chibest.Service.ModelDTOs.Request;

public class BranchStockRequest_QtyUpdate
{
    public int AvailableQty { get; set; }
    public int ReservedQty { get; set; }
    public int InTransitQty { get; set; }
    public int DefectiveQty { get; set; }
}