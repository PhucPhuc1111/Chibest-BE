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
                    x.ProductId == productId &&
                    x.WarehouseId == warehouseId);

            // 3️⃣ Nếu chưa có, tạo mới
            if (branchStock == null)
            {
                branchStock = new BranchStock
                {
                    Id = Guid.NewGuid(),
                    BranchId = (Guid)branchId,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    AvailableQty = Math.Max(0, deltaAvailableQty),
                    ReservedQty = Math.Max(0, deltaReservedQty),
                    InTransitQty = Math.Max(0, deltaInTransitQty),
                    DefectiveQty = Math.Max(0, deltaDefectiveQty),
                    TotalQty = Math.Max(0, deltaAvailableQty + deltaReservedQty + deltaInTransitQty + deltaDefectiveQty),
                    MinimumStock = 0,
                    MaximumStock = 0,
                    ReorderPoint = 0,
                    ReorderQty = 0,
                    CurrentSellingPrice = 0,
                    LastUpdated = DateTime.Now
                };

                await _context.BranchStocks.AddAsync(branchStock);
            }
            else
            {
                // 4️⃣ Cập nhật tồn kho
                branchStock.AvailableQty += deltaAvailableQty;
                branchStock.ReservedQty += deltaReservedQty;
                branchStock.InTransitQty += deltaInTransitQty;
                branchStock.DefectiveQty += deltaDefectiveQty;

                // Không cho âm tồn kho
                branchStock.AvailableQty = Math.Max(0, branchStock.AvailableQty);
                branchStock.ReservedQty = Math.Max(0, branchStock.ReservedQty);
                branchStock.InTransitQty = Math.Max(0, branchStock.InTransitQty);
                branchStock.DefectiveQty = Math.Max(0, branchStock.DefectiveQty);

                branchStock.TotalQty =
                    branchStock.AvailableQty +
                    branchStock.ReservedQty +
                    branchStock.InTransitQty +
                    branchStock.DefectiveQty;

                branchStock.LastUpdated = DateTime.Now;

                _context.BranchStocks.Update(branchStock);
            }

            // 5️⃣ Lưu DB
            await _context.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Cập nhật tồn kho thành công", branchStock);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, ex.Message);
        }
    }

}