using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Common.Enums;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.PurchaseOrder;
using Chibest.Service.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chibest.Service.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        public PurchaseOrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IBusinessResult> AddPurchaseOrder(PurchaseOrderCreate request)
        {
            if (request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Invalid request");

            string invoiceCode = request.InvoiceCode;
            if (invoiceCode == null)
                invoiceCode = await GenerateInvoiceCodeAsync();

            var purchaseOrder = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                InvoiceCode = invoiceCode,
                OrderDate = request.OrderDate,
                DiscountAmount = request.DiscountAmount,
                SupplierId = request.SupplierId,
                SubTotal = request.SubTotal,
                Note = request.Note,
                PayMethod = request.PayMethod,
                Paid = request.Paid,
                WarehouseId = request.WarehouseId,
                EmployeeId = request.EmployeeId,
                Status = OrderStatus.Draft.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var orderDetails = request.PurchaseOrderDetails.Select(detailReq => new PurchaseOrderDetail
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = purchaseOrder.Id,
                ProductId = detailReq.ProductId,
                Quantity = detailReq.Quantity,
                UnitPrice = detailReq.UnitPrice,
                Discount = detailReq.Discount,
                Note = detailReq.Note,
                ContainerCode = GenerateContainerCode()
            }).ToList();

            await _unitOfWork.BeginTransaction();

            try
            {
                await _unitOfWork.PurchaseOrderRepository.AddAsync(purchaseOrder);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.BulkInsertAsync(orderDetails);
                await _unitOfWork.CommitTransaction();

                return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_CREATE_MSG, new { purchaseOrder.InvoiceCode });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransaction();
                return new BusinessResult(Const.ERROR_EXCEPTION, "Error creating Purchase Order", ex.Message);
            }
        }

        public async Task<IBusinessResult> GetPurchaseOrderById(Guid id)
        {
            var po = await _unitOfWork.PurchaseOrderRepository
                .GetByWhere(x => x.Id == id)
                .Select(x => new PurchaseOrderResponse
                {
                    Id = x.Id,
                    InvoiceCode = x.InvoiceCode,
                    OrderDate = x.OrderDate,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    SubTotal = x.SubTotal,
                    DiscountAmount = x.DiscountAmount,
                    Paid = x.Paid,
                    Note = x.Note,
                    Status = x.Status,

                    WarehouseName = x.Warehouse != null ? x.Warehouse.Name : null,
                    EmployeeName = x.Employee != null ? x.Employee.Name : null,
                    SupplierName = x.Supplier != null ? x.Supplier.Name : null,

                    PurchaseOrderDetails = x.PurchaseOrderDetails.Select(d => new PurchaseOrderDetailResponse
                    {
                        Id = d.Id,
                        ContainerCode = d.ContainerCode,
                        Quantity = d.Quantity,
                        ActualQuantity = d.ActualQuantity,
                        UnitPrice = d.UnitPrice,
                        Discount = d.Discount,
                        Note = d.Note,
                        ProductName = d.Product != null ? d.Product.Name : null,
                        Sku = d.Product != null ? d.Product.Sku : string.Empty
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (po == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu nhập hàng");

            return new BusinessResult(Const.HTTP_STATUS_OK, "Lấy dữ liệu phiếu nhập thành công", po);
        }
        public async Task<IBusinessResult> GetPurchaseOrderList(int pageIndex, int pageSize, string search,
    DateTime? fromDate = null, DateTime? toDate = null, string status = null)
        {
            Expression<Func<PurchaseOrder, bool>> filter = x => true;

            if (!string.IsNullOrEmpty(search))
            {
                string searchTerm = search.ToLower();
                Expression<Func<PurchaseOrder, bool>> searchFilter = x => x.InvoiceCode.ToLower().Contains(searchTerm);
                filter = filter.And(searchFilter);
            }

            if (!string.IsNullOrEmpty(status))
            {
                string statusTerm = status.ToLower();
                Expression<Func<PurchaseOrder, bool>> statusFilter = x => x.Status.ToLower().Contains(statusTerm);
                filter = filter.And(statusFilter);
            }
            if (fromDate.HasValue)
            {
                Expression<Func<PurchaseOrder, bool>> fromDateFilter = x => x.OrderDate >= fromDate.Value;
                filter = filter.And(fromDateFilter);
            }

            if (toDate.HasValue)
            {
                DateTime endDate = toDate.Value.AddDays(1).AddSeconds(-1);
                Expression<Func<PurchaseOrder, bool>> toDateFilter = x => x.OrderDate <= endDate;
                filter = filter.And(toDateFilter);
            }

            var POs = await _unitOfWork.PurchaseOrderRepository.GetPagedAsync(
                pageIndex,
                pageSize,
                filter
            );

            if (POs == null || !POs.Any())
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }

            var responseList = POs.Select(pos => new PurchaseOrderList
            {
                Id = pos.Id,
                InvoiceCode = pos.InvoiceCode,
                OrderDate = pos.OrderDate,
                SubTotal = pos.SubTotal,
                Status = pos.Status,
            }).ToList();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, responseList);
        }

        public async Task<IBusinessResult> UpdateAsync(Guid id, PurchaseOrderUpdate request)
        {
            // Lấy PO cùng với danh sách detail
            var purchaseOrder = await _unitOfWork.PurchaseOrderRepository
                .GetByWhere(x => x.Id == id)
                .Include(po => po.PurchaseOrderDetails)
                .FirstOrDefaultAsync();

            if (purchaseOrder == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu nhập hàng");

            // Cập nhật chi tiết
            foreach (var detailReq in request.PurchaseOrderDetails)
            {
                var detail = purchaseOrder.PurchaseOrderDetails
                    .FirstOrDefault(x => x.Id == detailReq.Id);

                if (detail != null)
                {
                    detail.ActualQuantity = detailReq.ActualQuantity ?? detail.ActualQuantity;
                }
            }

            purchaseOrder.Status = request.Status;
            purchaseOrder.UpdatedAt = DateTime.Now;

            await _unitOfWork.BeginTransaction();

            try
            {
                _unitOfWork.PurchaseOrderRepository.Update(purchaseOrder);
                await _unitOfWork.SaveChangesAsync();
                var detailsToUpdate = purchaseOrder.PurchaseOrderDetails.ToList();
                await _unitOfWork.BulkUpdateAsync(detailsToUpdate);
                await _unitOfWork.CommitTransaction();
                return new BusinessResult(Const.SUCCESS, "Cập nhật thành công");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransaction();
                return new BusinessResult(Const.ERROR_EXCEPTION, "Lỗi khi cập nhật phiếu nhập hàng", ex.Message);
            }
        }

        private async Task<string> GenerateInvoiceCodeAsync()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string prefix = "PO" + datePart;

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

        private string GenerateContainerCode()
        {
            string timePart = DateTime.Now.ToString("yyyyMMddHHmmss");
            string randomPart = new Random().Next(100, 999).ToString(); 
            return $"CTN{timePart}{randomPart}";
        }
    }
}