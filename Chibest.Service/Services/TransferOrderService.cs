using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Common.Enums;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.PurchaseOrder;
using Chibest.Service.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Chibest.Service.ModelDTOs.Stock.TransferOrder.create;
using static Chibest.Service.ModelDTOs.Stock.TransferOrder.id;
using static Chibest.Service.ModelDTOs.Stock.TransferOrder.list;
using static Chibest.Service.ModelDTOs.Stock.TransferOrder.update;

namespace Chibest.Service.Services
{
    public class TransferOrderService : ITransferOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TransferOrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IBusinessResult> AddTransferOrder(TransferOrderCreate request)
        {
            if (request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Invalid request");

            string invoiceCode = request.InvoiceCode;
            if (invoiceCode == null)
                invoiceCode = await GenerateInvoiceCodeAsync();

            var transferOrder = new TransferOrder
            {
                Id = Guid.NewGuid(),
                InvoiceCode = invoiceCode,
                OrderDate = request.OrderDate,
                DiscountAmount = request.DiscountAmount,
                SubTotal = request.SubTotal,
                Paid = request.Paid,
                Note = request.Note,
                PayMethod = request.PayMethod,
                FromWarehouseId = request.FromWarehouseId,
                ToWarehouseId = request.ToWarehouseId,
                EmployeeId = request.EmployeeId,
                Status = OrderStatus.Draft.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            
            var transferDetails = request.TransferOrderDetails.Select(detailReq => new TransferOrderDetail
            {
                Id = Guid.NewGuid(),
                ContainerCode = GenerateContainerCode(),
                TransferOrderId = transferOrder.Id,
                ProductId = detailReq.ProductId,
                Quantity = detailReq.Quantity,
                UnitPrice = detailReq.UnitPrice,
                Discount = detailReq.Discount,
                Note = detailReq.Note
            }).ToList();

            await _unitOfWork.BeginTransaction();

            try
            {
                await _unitOfWork.TransferOrderRepository.AddAsync(transferOrder);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.BulkInsertAsync(transferDetails);

                await _unitOfWork.CommitTransaction();

                return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_CREATE_MSG, new { transferOrder.InvoiceCode });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransaction();
                return new BusinessResult(Const.ERROR_EXCEPTION, "Error creating Transfer Order", ex.Message);
            }
        }

        public async Task<IBusinessResult> GetTransferOrderList(int pageIndex, int pageSize, string search,
    DateTime? fromDate = null, DateTime? toDate = null, string status = null)
        {
            Expression<Func<TransferOrder, bool>> filter = x => true;

            if (!string.IsNullOrEmpty(search))
            {
                string searchTerm = search.ToLower();
                Expression<Func<TransferOrder, bool>> searchFilter = x => x.InvoiceCode.ToLower().Contains(searchTerm);
                filter = filter.And(searchFilter);
            }

            if (!string.IsNullOrEmpty(status))
            {
                string statusTerm = status.ToLower();
                Expression<Func<TransferOrder, bool>> statusFilter = x => x.Status.ToLower().Contains(statusTerm);
                filter = filter.And(statusFilter);
            }

            if (fromDate.HasValue)
            {
                Expression<Func<TransferOrder, bool>> fromDateFilter = x => x.OrderDate >= fromDate.Value;
                filter = filter.And(fromDateFilter);
            }

            if (toDate.HasValue)
            {
                DateTime endDate = toDate.Value.AddDays(1).AddSeconds(-1);
                Expression<Func<TransferOrder, bool>> toDateFilter = x => x.OrderDate <= endDate;
                filter = filter.And(toDateFilter);
            }

            var transferOrders = await _unitOfWork.TransferOrderRepository.GetPagedAsync(
                pageIndex,
                pageSize,
                filter
            );

            if (transferOrders == null || !transferOrders.Any())
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }

            var responseList = transferOrders.Select(to => new TransferOrderList
            {
                Id = to.Id,
                InvoiceCode = to.InvoiceCode,
                OrderDate = to.OrderDate,
                SubTotal = to.SubTotal,
                Status = to.Status,
            }).ToList();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, responseList);
        }

        public async Task<IBusinessResult> GetTransferOrderById(Guid id)
        {
            var transferOrder = await _unitOfWork.TransferOrderRepository
                .GetByWhere(x => x.Id == id)
                .Select(x => new TransferOrderResponse
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

                    FromWarehouseName = x.FromWarehouse != null ? x.FromWarehouse.Name : null,
                    ToWarehouseName = x.ToWarehouse != null ? x.ToWarehouse.Name : null,

                    TransferOrderDetails = x.TransferOrderDetails.Select(d => new TransferOrderDetailResponse
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

            if (transferOrder == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu chuyển kho");

            return new BusinessResult(Const.HTTP_STATUS_OK, "Lấy dữ liệu phiếu chuyển kho thành công", transferOrder);
        }

        public async Task<IBusinessResult> UpdateTransferOrderAsync(Guid id, TransferOrderUpdate request)
        {
            var transferOrder = await _unitOfWork.TransferOrderRepository
                .GetByWhere(x => x.Id == id)
                .Include(to => to.TransferOrderDetails)
                .FirstOrDefaultAsync();

            if (transferOrder == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu chuyển kho");

            foreach (var detailReq in request.TransferOrderDetails)
            {
                var detail = transferOrder.TransferOrderDetails
                    .FirstOrDefault(x => x.Id == detailReq.Id);

                if (detail != null)
                {
                    detail.ActualQuantity = detailReq.ActualQuantity ?? detail.ActualQuantity;
                }
            }

            transferOrder.Status = request.Status;
            transferOrder.UpdatedAt = DateTime.Now;

            await _unitOfWork.BeginTransaction();

            try
            {
                _unitOfWork.TransferOrderRepository.Update(transferOrder);
                await _unitOfWork.SaveChangesAsync();
                var detailsToUpdate = transferOrder.TransferOrderDetails.ToList();
                await _unitOfWork.BulkUpdateAsync(detailsToUpdate);

                await _unitOfWork.CommitTransaction();

                return new BusinessResult(Const.SUCCESS, "Cập nhật phiếu chuyển kho thành công");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransaction();
                return new BusinessResult(Const.ERROR_EXCEPTION, "Lỗi khi cập nhật phiếu chuyển kho", ex.Message);
            }
        }

        private async Task<string> GenerateInvoiceCodeAsync()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string prefix = "TRF" + datePart;

            var latest = await _unitOfWork.TransferOrderRepository
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
