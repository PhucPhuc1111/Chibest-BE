using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Common.Enums;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.PurchaseOrder;
using Microsoft.EntityFrameworkCore;

namespace Chibest.Service.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PurchaseOrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region CREATE
        public async Task<IBusinessResult> AddPurchaseOrder(PurchaseOrderCreate request)
        {
            if (request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Invalid request");

            string invoiceCode = await GenerateInvoiceCodeAsync();

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

            await _unitOfWork.BeginTransaction();

            try
            {
                foreach (var detailReq in request.PurchaseOrderDetails)
                {
                    var containerCode = GenerateContainerCode();
                    var detail = new PurchaseOrderDetail
                    {
                        Id = Guid.NewGuid(),
                        PurchaseOrderId = purchaseOrder.Id,
                        ProductId = detailReq.ProductId,
                        Quantity = detailReq.Quantity,
                        UnitPrice = detailReq.UnitPrice,
                        Discount = detailReq.Discount,
                        Note = detailReq.Note,
                        ContainerCode = containerCode,
                    };

                    purchaseOrder.PurchaseOrderDetails.Add(detail);
                }
                await _unitOfWork.PurchaseOrderRepository.AddAsync(purchaseOrder);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransaction();

                return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_CREATE_MSG, new { purchaseOrder.InvoiceCode });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransaction();
                return new BusinessResult(Const.ERROR_EXCEPTION, "Error creating Purchase Order", ex.Message);
            }
        }
        #endregion

        #region READ

        public async Task<IBusinessResult> GetPurchaseOrderById(Guid id)
        {
            // 1️⃣ Lấy phiếu nhập hàng + các quan hệ cần thiết
            var po = await _unitOfWork.PurchaseOrderRepository
                .GetByWhere(x => x.Id == id)
                .Include(x => x.PurchaseOrderDetails)
                    .ThenInclude(d => d.Product)
                .Include(x => x.Warehouse)
                .Include(x => x.Employee)
                .Include(x => x.Supplier)
                .FirstOrDefaultAsync();

            // 2️⃣ Kiểm tra tồn tại
            if (po == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu nhập hàng");

            // 3️⃣ Map sang DTO Response
            var response = new PurchaseOrderResponse
            {
                Id = po.Id,
                InvoiceCode = po.InvoiceCode,
                OrderDate = po.OrderDate,
                CreatedAt = po.CreatedAt,
                UpdatedAt = po.UpdatedAt,
                SubTotal = po.SubTotal,
                DiscountAmount = po.DiscountAmount,
                Paid = po.Paid,
                Note = po.Note,
                Status = po.Status, 
                WarehouseName = po.Warehouse?.Name,
                EmployeeName = po.Employee?.Name,
                SupplierName = po.Supplier?.Name,

                PurchaseOrderDetails = po.PurchaseOrderDetails.Select(d => new PurchaseOrderDetailResponse
                {
                    Id = d.Id,
                    ContainerCode = d.ContainerCode,
                    Quantity = d.Quantity,
                    ActualQuantity = d.ActualQuantity,
                    UnitPrice = d.UnitPrice,
                    Discount = d.Discount,
                    Note = d.Note,
                    ProductName = d.Product?.Name,
                    Sku = d.Product?.Sku ?? string.Empty
                }).ToList()
            };
            return new BusinessResult(Const.HTTP_STATUS_OK, "Lấy dữ liệu phiếu nhập thành công", response);
        }


        public async Task<IBusinessResult> GetPurchaseOrderList(int pageIndex, int pageSize, string search)
        {
            string searchTerm = search?.ToLower() ?? string.Empty;

            var POs = await _unitOfWork.PurchaseOrderRepository.GetPagedAsync(
                pageIndex,
                pageSize,
                x => string.IsNullOrEmpty(searchTerm) || x.InvoiceCode.ToLower().Contains(searchTerm)
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
        #endregion

        #region UPDATE
        public async Task<IBusinessResult> UpdateAsync(Guid id, PurchaseOrderUpdate request)
        {
            var purchaseOrder = await _unitOfWork.PurchaseOrderRepository
                .GetByWhere(x => x.Id == id).Include(po => po.PurchaseOrderDetails).FirstOrDefaultAsync();

            if (purchaseOrder == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu nhập hàng");

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
            _unitOfWork.PurchaseOrderRepository.Update(purchaseOrder);
            await _unitOfWork.SaveChangesAsync();
            return new BusinessResult(Const.SUCCESS, "Cập nhật thành công");
        }

        #endregion

        #region CODE GENERATION
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
            string randomPart = new Random().Next(10, 99).ToString(); 
            return $"CTN{timePart}{randomPart}";
        }
        #endregion

    }
}
