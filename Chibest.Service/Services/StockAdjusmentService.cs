using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Chibest.Service.ModelDTOs.Stock.StockAdjustment.create;
using static Chibest.Service.ModelDTOs.Stock.StockAdjustment.id;
using static Chibest.Service.ModelDTOs.Stock.StockAdjustment.list;

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
                ApprovedBy = request.ApprovedBy,
                Note = request.Note,
                Status = "Chờ Duyệt",
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
            await _unitOfWork.BeginTransaction();

            try
            {
                await _unitOfWork.StockAdjusmentRepository.AddAsync(stockAdjustment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.BulkInsertAsync(adjustmentDetails);
                await _unitOfWork.CommitTransaction();

                return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_CREATE_MSG, new
                {
                    stockAdjustment.AdjustmentCode,
                    TotalValueChange = stockAdjustment.TotalValueChange
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransaction();
                return new BusinessResult(Const.ERROR_EXCEPTION, "Error creating Stock Adjustment", ex.Message);
            }
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
                TotalValueChange = (decimal)adj.TotalValueChange,
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

            var latest = await _unitOfWork.PurchaseOrderRepository
                .GetByWhere(x => x.InvoiceCode.StartsWith(prefix))
                .OrderByDescending(x => x.InvoiceCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (latest != null)
            {
                string lastNumberPart = latest.InvoiceCode.Substring(prefix.Length);
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D4}";
        }
    }
}
