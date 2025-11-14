using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Chibest.Repository.Repositories;

public class BranchStockRepository : GenericRepository<BranchStock>, IBranchStockRepository
{
    public BranchStockRepository(ChiBestDbContext context) : base(context) { }

    public async Task<IBusinessResult> UpdateBranchStockAsync(
    Guid warehouseId,
    Guid productId,
    int deltaAvailableQty = 0,
    int deltaReservedQty = 0,
    int deltaInTransitQty = 0,
    int deltaDefectiveQty = 0)
    {
        try
        {
            var warehouse = await _context.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == warehouseId);

            if (warehouse == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, $"Không tìm thấy Warehouse ID: {warehouseId}");

            var branchId = warehouse.BranchId;

            // 2️⃣ Tìm bản ghi tồn kho
            var branchStock = await _context.BranchStocks
                .FirstOrDefaultAsync(x =>
                    x.BranchId == branchId &&
                    x.ProductId == productId);

            if (branchStock == null)
            {
                branchStock = new BranchStock
                {
                    Id = Guid.NewGuid(),
                    BranchId = (Guid)branchId,
                    ProductId = productId,
                    AvailableQty = Math.Max(0, deltaAvailableQty),
                    MinimumStock = 0,
                    MaximumStock = 0,
                    ReorderPoint = 0,
                    ReorderQty = 0,
                };

                await _context.BranchStocks.AddAsync(branchStock);
            }
            else
            {
                branchStock.AvailableQty += deltaAvailableQty;
                _context.BranchStocks.Update(branchStock);
            }

            await _context.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Cập nhật tồn kho thành công", branchStock);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, ex.Message);
        }
    }

}