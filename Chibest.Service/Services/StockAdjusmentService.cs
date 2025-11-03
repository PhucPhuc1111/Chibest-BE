using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using static Chibest.Service.ModelDTOs.Stock.StockAdjustment.create;
using static Chibest.Service.ModelDTOs.Stock.StockAdjustment.id;
using static Chibest.Service.ModelDTOs.Stock.StockAdjustment.list;
using static Chibest.Service.ModelDTOs.Stock.StockAdjustment.update;

namespace Chibest.Service.Services
{
    public class StockAdjusmentService : IStockAdjusmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        public StockAdjusmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IBusinessResult> AddStockAdjustment(StockAdjustmentCreate request)
        {
            if (request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Invalid request");

            string adjustmentCode = request.AdjustmentCode;
            if (string.IsNullOrWhiteSpace(adjustmentCode))
                adjustmentCode = await GenerateInvoiceCodeAsync();

            var stockAdjustment = new StockAdjustment
            {
                Id = Guid.NewGuid(),
                AdjustmentCode = adjustmentCode,
                AdjustmentDate = request.AdjustmentDate == default ? DateTime.Now : request.AdjustmentDate,
                AdjustmentType = request.AdjustmentType.ToString(),
                BranchId = request.BranchId,
                WarehouseId = request.WarehouseId,
                EmployeeId = request.EmployeeId,
                Note = request.Note,
                Status = request.Status != null ? request.Status : "Lưu Tạm",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var adjustmentDetails = new List<StockAdjustmentDetail>();
            decimal totalValueChange = 0;

            foreach (var detailReq in request.StockAdjustmentDetails)
            {
                int diffQty = detailReq.ActualQty - detailReq.SystemQty;
                decimal valueChange = diffQty * detailReq.UnitCost;

                totalValueChange += valueChange;

                adjustmentDetails.Add(new StockAdjustmentDetail
                {
                    Id = Guid.NewGuid(),
                    StockAdjustmentId = stockAdjustment.Id,
                    ProductId = detailReq.ProductId,
                    SystemQty = detailReq.SystemQty,
                    ActualQty = detailReq.ActualQty,
                    UnitCost = detailReq.UnitCost,
                    Reason = null,
                    Note = null
                });
            }

            stockAdjustment.TotalValueChange = totalValueChange;

                await _unitOfWork.StockAdjusmentRepository.AddAsync(stockAdjustment);
                await _unitOfWork.StockAdjustmentDetailRepository.AddRangeAsync(adjustmentDetails);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_CREATE_MSG, new
                {
                    stockAdjustment.AdjustmentCode,
                    TotalValueChange = stockAdjustment.TotalValueChange
                });
            
        }

        public async Task<IBusinessResult> UpdateStockAdjustment(Guid id, StockAdjustmentUpdate request)
        {
            if (request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Invalid request");

            var stockAdjustment = await _unitOfWork.StockAdjusmentRepository
                .GetByWhere(x => x.Id == id)
                .Include(x => x.StockAdjustmentDetails)
                .FirstOrDefaultAsync();

            if (stockAdjustment == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu điều chỉnh tồn kho");

            if (stockAdjustment.Status != "Lưu Tạm")
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Chỉ có thể cập nhật phiếu điều chỉnh ở trạng thái Lưu Tạm");

            stockAdjustment.AdjustmentType = request.AdjustmentType?.ToString() ?? stockAdjustment.AdjustmentType;
            stockAdjustment.Note = request.Note ?? stockAdjustment.Note;
            stockAdjustment.Status = request.Status ?? stockAdjustment.Status;
            stockAdjustment.UpdatedAt = DateTime.Now;

            decimal totalValueChange = 0;
            foreach (var detailReq in request.StockAdjustmentDetails)
            {
                var detail = stockAdjustment.StockAdjustmentDetails
                    .FirstOrDefault(x => x.Id == detailReq.Id);

                if (detail != null)
                {
                    detail.ProductId = detailReq.ProductId;
                    detail.SystemQty = detailReq.SystemQty;
                    detail.ActualQty = detailReq.ActualQty;
                    detail.UnitCost = detailReq.UnitCost;
                    detail.Reason = detailReq.Reason ?? detail.Reason;
                    detail.Note = detailReq.Note ?? detail.Note;

                    int diffQty = detail.ActualQty - detail.SystemQty;
                    decimal valueChange = diffQty * detail.UnitCost;

                    detail.DifferenceQty = diffQty;
                    detail.TotalValueChange = valueChange;
                    totalValueChange += valueChange;
                }
            }

            stockAdjustment.TotalValueChange = totalValueChange;

                _unitOfWork.StockAdjusmentRepository.Update(stockAdjustment);
                
                var detailsToUpdate = stockAdjustment.StockAdjustmentDetails.ToList();
                _unitOfWork.StockAdjustmentDetailRepository.UpdateRange(detailsToUpdate);

                if (request.Status == "Hoàn Thành" || request.Status == "Đã Duyệt")
                {
                    foreach (var detail in stockAdjustment.StockAdjustmentDetails)
                    {
                        if (detail.DifferenceQty != 0)
                        {
                            var result = await _unitOfWork.BranchStockRepository.UpdateBranchStockAsync(
                                warehouseId: (Guid)stockAdjustment.WarehouseId,
                                productId: detail.ProductId,
                                deltaAvailableQty: detail.DifferenceQty ?? 0
                            );

                            if (result.StatusCode != Const.HTTP_STATUS_OK)
                            {
                                return new BusinessResult(Const.ERROR_EXCEPTION,
                                    $"Lỗi cập nhật tồn kho cho sản phẩm {detail.ProductId}: {result.Message}");
                            }
                        }
                    }
                    stockAdjustment.ApprovedBy = request.ApprovebyId;
                    stockAdjustment.ApprovedAt = DateTime.Now;
                    await _unitOfWork.SaveChangesAsync();
                }

                return new BusinessResult(Const.HTTP_STATUS_OK, "Cập nhật phiếu điều chỉnh tồn kho thành công", new
                {
                    stockAdjustment.AdjustmentCode,
                    TotalValueChange = stockAdjustment.TotalValueChange
                });

            
        }

        public async Task<IBusinessResult> GetStockAdjustmentList(
    int pageIndex,
    int pageSize,
    string? search,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    string? status = null,
    string? adjustmentType = null)
        {
            Expression<Func<StockAdjustment, bool>> filter = x => true;

            if (!string.IsNullOrEmpty(search))
            {
                string searchTerm = search.ToLower();
                Expression<Func<StockAdjustment, bool>> searchFilter =
                    x => x.AdjustmentCode.ToLower().Contains(searchTerm);
                filter = filter.And(searchFilter);
            }

            if (!string.IsNullOrEmpty(status))
            {
                string statusTerm = status.ToLower();
                Expression<Func<StockAdjustment, bool>> statusFilter =
                    x => x.Status.ToLower().Contains(statusTerm);
                filter = filter.And(statusFilter);
            }

            if (!string.IsNullOrEmpty(adjustmentType))
            {
                string typeTerm = adjustmentType.ToLower();
                Expression<Func<StockAdjustment, bool>> typeFilter =
                    x => x.AdjustmentType.ToLower().Contains(typeTerm);
                filter = filter.And(typeFilter);
            }

            if (fromDate.HasValue)
            {
                Expression<Func<StockAdjustment, bool>> fromDateFilter =
                    x => x.AdjustmentDate >= fromDate.Value;
                filter = filter.And(fromDateFilter);
            }

            if (toDate.HasValue)
            {
                DateTime endDate = toDate.Value.AddDays(1).AddSeconds(-1);
                Expression<Func<StockAdjustment, bool>> toDateFilter =
                    x => x.AdjustmentDate <= endDate;
                filter = filter.And(toDateFilter);
            }

            var adjustments = await _unitOfWork.StockAdjusmentRepository.GetPagedAsync(
                pageIndex,
                pageSize,
                filter,
                orderBy: q => q.OrderByDescending(x => x.AdjustmentDate)
            );

            if (adjustments == null || !adjustments.Any())
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }

            var responseList = adjustments.Select(adj => new StockAdjustmentList
            {
                Id = adj.Id,
                AdjustmentCode = adj.AdjustmentCode,
                AdjustmentDate = adj.AdjustmentDate,
                AdjustmentType = adj.AdjustmentType,
                TotalValueChange = adj.TotalValueChange,
                Status = adj.Status
            }).ToList();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, responseList);
        }

        public async Task<IBusinessResult> GetStockAdjustmentById(Guid id)
        {
            var adjustment = await _unitOfWork.StockAdjusmentRepository
                .GetByWhere(x => x.Id == id)
                .Select(x => new StockAdjustmentResponse
                {
                    Id = x.Id,
                    AdjustmentCode = x.AdjustmentCode,
                    AdjustmentDate = x.AdjustmentDate,
                    AdjustmentType = x.AdjustmentType,
                    BranchName = x.Branch != null ? x.Branch.Name : null,
                    WarehouseName = x.Warehouse != null ? x.Warehouse.Name : null,
                    EmployeeName = x.Employee != null ? x.Employee.Name : null,
                    ApproveName = x.ApprovedByNavigation != null ? x.ApprovedByNavigation.Name : null,
                    ApprovedAt = x.ApprovedAt,
                    TotalValueChange = x.TotalValueChange,
                    Status = x.Status,
                    Reason = x.Reason,
                    Note = x.Note,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,

                    StockAdjustmentDetails = x.StockAdjustmentDetails.Select(d => new StockAdjustmentDetailResponse
                    {
                        Id = d.Id,
                        SystemQty = d.SystemQty,
                        ActualQty = d.ActualQty,
                        DifferenceQty = d.DifferenceQty,
                        UnitCost = d.UnitCost,
                        TotalValueChange = d.TotalValueChange,
                        Reason = d.Reason,
                        Note = d.Note,
                        ProductName = d.Product != null ? d.Product.Name : null,
                        Sku = d.Product != null ? d.Product.Sku : string.Empty
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (adjustment == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu điều chỉnh tồn kho");

            return new BusinessResult(Const.HTTP_STATUS_OK, "Lấy dữ liệu phiếu điều chỉnh tồn kho thành công", adjustment);
        }
        private async Task<string> GenerateInvoiceCodeAsync()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string prefix = "KK" + datePart;

            var latest = await _unitOfWork.StockAdjusmentRepository
                .GetByWhere(x => x.AdjustmentCode.StartsWith(prefix))
                .OrderByDescending(x => x.AdjustmentCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (latest != null)
            {
                string lastNumberPart = latest.AdjustmentCode.Substring(prefix.Length);
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D4}";
        }
        
        public async Task<IBusinessResult> DeleteStockAdjustment(Guid id)
        {
            var stockAdjustment = await _unitOfWork.StockAdjusmentRepository
                .GetByWhere(x => x.Id == id)
                .Include(x => x.StockAdjustmentDetails)
                .FirstOrDefaultAsync();

            if (stockAdjustment == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu kiểm kho");

            if (stockAdjustment.Status != "Lưu Tạm")
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không thể xóa phiếu kiểm kho đã duyệt/hoàn thành");

            _unitOfWork.StockAdjusmentRepository.Delete(stockAdjustment);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Xóa phiếu kiểm kho kho thành công");
        }
    }
}