using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Common.Enums;
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
using static Chibest.Service.ModelDTOs.Stock.PurchaseReturn.create;
using static Chibest.Service.ModelDTOs.Stock.PurchaseReturn.id;
using static Chibest.Service.ModelDTOs.Stock.PurchaseReturn.list;
using static Chibest.Service.ModelDTOs.Stock.PurchaseReturn.update;

namespace Chibest.Service.Services
{
    public class PurchaseReturnService : IPurchaseReturnService
    {
        private readonly IUnitOfWork _unitOfWork;
        public PurchaseReturnService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IBusinessResult> AddPurchaseReturn(PurchaseReturnCreate request)
        {
            if (request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Invalid request");

            string invoiceCode = request.InvoiceCode;
            if (string.IsNullOrEmpty(invoiceCode))
                invoiceCode = await GenerateInvoiceCodeAsync(); 

            var purchaseReturn = new PurchaseReturn
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

            var returnDetails = request.PurchaseReturnDetails.Select(detailReq => new PurchaseReturnDetail
            {
                Id = Guid.NewGuid(),
                ContainerCode = GenerateContainerCode(),
                PurchaseReturnId = purchaseReturn.Id,
                ProductId = detailReq.ProductId,
                Quantity = detailReq.Quantity,
                UnitPrice = detailReq.UnitPrice,
                ReturnPrice = detailReq.ReturnPrice,
                Note = detailReq.Note
            }).ToList();
            await _unitOfWork.BeginTransaction();

            try
            {
                await _unitOfWork.PurchaseReturnRepository.AddAsync(purchaseReturn);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.BulkInsertAsync(returnDetails);

                await _unitOfWork.CommitTransaction();

                return new BusinessResult(Const.HTTP_STATUS_OK, "Tạo phiếu trả hàng thành công", new { purchaseReturn.InvoiceCode });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransaction();
                return new BusinessResult(Const.ERROR_EXCEPTION, "Lỗi khi tạo phiếu trả hàng", ex.Message);
            }
        }

        public async Task<IBusinessResult> GetPurchaseReturnById(Guid id)
        {
            var pr = await _unitOfWork.PurchaseReturnRepository
                .GetByWhere(x => x.Id == id)
                .Select(x => new PurchaseReturnResponse
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

                    FromWarehouseName = x.Warehouse != null ? x.Warehouse.Name : null,
                    ToWarehouseName = x.Supplier != null ? x.Supplier.Name : null, 

                    PurchaseReturnDetails = x.PurchaseReturnDetails.Select(d => new PurchaseReturnDetailResponse
                    {
                        Id = d.Id,
                        ContainerCode = d.ContainerCode,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice,
                        ReturnPrice = d.ReturnPrice,
                        Note = d.Note,
                        ProductName = d.Product != null ? d.Product.Name : null,
                        Sku = d.Product != null ? d.Product.Sku : string.Empty
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (pr == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu trả hàng");

            return new BusinessResult(Const.HTTP_STATUS_OK, "Lấy dữ liệu phiếu trả hàng thành công", pr);
        }

        public async Task<IBusinessResult> GetPurchaseReturnList(
    int pageIndex,
    int pageSize,
    string search,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    string status = null)
        {
            Expression<Func<PurchaseReturn, bool>> filter = x => true;

            if (!string.IsNullOrEmpty(search))
            {
                string searchTerm = search.ToLower();
                Expression<Func<PurchaseReturn, bool>> searchFilter = x => x.InvoiceCode.ToLower().Contains(searchTerm);
                filter = filter.And(searchFilter);
            }

            if (!string.IsNullOrEmpty(status))
            {
                string statusTerm = status.ToLower();
                Expression<Func<PurchaseReturn, bool>> statusFilter = x => x.Status.ToLower().Contains(statusTerm);
                filter = filter.And(statusFilter);
            }

            if (fromDate.HasValue)
            {
                Expression<Func<PurchaseReturn, bool>> fromDateFilter = x => x.OrderDate >= fromDate.Value;
                filter = filter.And(fromDateFilter);
            }

            if (toDate.HasValue)
            {
                DateTime endDate = toDate.Value.AddDays(1).AddSeconds(-1);
                Expression<Func<PurchaseReturn, bool>> toDateFilter = x => x.OrderDate <= endDate;
                filter = filter.And(toDateFilter);
            }

            var purchaseReturns = await _unitOfWork.PurchaseReturnRepository.GetPagedAsync(
                pageIndex,
                pageSize,
                filter
            );

            if (purchaseReturns == null || !purchaseReturns.Any())
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }

            var responseList = purchaseReturns.Select(pr => new PurchaseReturnList
            {
                Id = pr.Id,
                InvoiceCode = pr.InvoiceCode,
                OrderDate = pr.OrderDate,
                SubTotal = pr.SubTotal,
                Status = pr.Status
            }).ToList();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, responseList);
        }

        public async Task<IBusinessResult> UpdatePurchaseReturnAsync(Guid id, OrderStatus status)
        {
            var purchaseReturn = await _unitOfWork.PurchaseReturnRepository
                .GetByWhere(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (purchaseReturn == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu trả hàng");

            purchaseReturn.Status = status.ToString();
            purchaseReturn.UpdatedAt = DateTime.Now;

            await _unitOfWork.BeginTransaction();

            try
            {
                _unitOfWork.PurchaseReturnRepository.Update(purchaseReturn);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransaction();

                return new BusinessResult(Const.SUCCESS, "Cập nhật trạng thái phiếu trả hàng thành công");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransaction();
                return new BusinessResult(Const.ERROR_EXCEPTION, "Lỗi khi cập nhật phiếu trả hàng", ex.Message);
            }
        }


        private async Task<string> GenerateInvoiceCodeAsync()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string prefix = "THN" + datePart;

            var latest = await _unitOfWork.PurchaseReturnRepository
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
