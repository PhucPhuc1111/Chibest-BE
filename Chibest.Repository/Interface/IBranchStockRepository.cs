using Chibest.Common.BusinessResult;
using Chibest.Repository.Base;
using Chibest.Repository.Models;

namespace Chibest.Repository.Interface;

public interface IBranchStockRepository : IGenericRepository<BranchStock> 
{
    Task<IBusinessResult> UpdateBranchStockAsync(
    Guid warehouseId,
    Guid productId,
    int deltaAvailableQty = 0,
    int deltaReservedQty = 0,
    int deltaInTransitQty = 0,
    int deltaDefectiveQty = 0);
}