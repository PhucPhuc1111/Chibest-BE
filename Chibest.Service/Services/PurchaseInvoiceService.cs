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

namespace Chibest.Service.Services;

public class PurchaseInvoiceService : IPurchaseInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseInvoiceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> CreateInvoiceAsync(PurchaseInvoiceCreate request)
    {
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Invalid request");

        if (request.SupplierId == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "SupplierId is required");

        if (request.PurchaseOrders == null || !request.PurchaseOrders.Any())
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Danh sách phiếu nhập không được để trống");

        string invoiceCode = string.IsNullOrWhiteSpace(request.Code)
            ? await GeneratePurchaseInvoiceCodeAsync()
            : request.Code!;

        DateTime invoiceDate = request.OrderDate == default ? DateTime.Now : request.OrderDate;
        string invoiceStatus = string.IsNullOrWhiteSpace(request.Status) ? OrderStatus.Draft.ToString() : request.Status!;

        var purchaseInvoice = new PurchaseInvoice
        {
            Id = Guid.NewGuid(),
            Code = invoiceCode,
            OrderDate = invoiceDate,
            Status = invoiceStatus,
            SupplierId = request.SupplierId
        };

        var purchaseOrders = new List<PurchaseOrder>();
        var purchaseOrderDetails = new List<PurchaseOrderDetail>();

        foreach (var orderReq in request.PurchaseOrders)
        {
            if (orderReq.PurchaseOrderDetails == null || !orderReq.PurchaseOrderDetails.Any())
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Mỗi phiếu nhập phải có ít nhất một sản phẩm");

            string purchaseOrderCode = string.IsNullOrWhiteSpace(orderReq.InvoiceCode)
                ? await GeneratePurchaseOrderCodeAsync()
                : orderReq.InvoiceCode!;

            var purchaseOrder = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                InvoiceCode = purchaseOrderCode,
                OrderDate = orderReq.OrderDate,
                SupplierId = request.SupplierId,
                SubTotal = orderReq.SubTotal,
                Note = orderReq.Note,
                BranchId = orderReq.BranchId,
                EmployeeId = orderReq.EmployeeId,
                Status = OrderStatus.Draft.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                PurchaseInvoiceId = purchaseInvoice.Id
            };

            var details = orderReq.PurchaseOrderDetails.Select(detailReq => new PurchaseOrderDetail
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = purchaseOrder.Id,
                ProductId = detailReq.ProductId,
                Quantity = detailReq.Quantity,
                UnitPrice = detailReq.UnitPrice,
                ReFee = detailReq.ReFee,
                Note = detailReq.Note
            }).ToList();

            purchaseOrders.Add(purchaseOrder);
            purchaseOrderDetails.AddRange(details);
        }

        purchaseInvoice.TotalMoney = purchaseOrders.Sum(x => x.SubTotal);

        await _unitOfWork.PurchaseInvoiceRepository.AddAsync(purchaseInvoice);
        await _unitOfWork.PurchaseOrderRepository.AddRangeAsync(purchaseOrders);

        if (purchaseOrderDetails.Any())
        {
            await _unitOfWork.PurchaseOrderDetailRepository.AddRangeAsync(purchaseOrderDetails);
        }

        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, "Tạo hóa đơn nhập hàng thành công", new
        {
            InvoiceCode = purchaseInvoice.Code,
            PurchaseOrderCodes = purchaseOrders.Select(po => po.InvoiceCode).ToList()
        });
    }

    public async Task<IBusinessResult> UpdateInvoiceStatusAsync(Guid invoiceId, OrderStatus status)
    {
        var invoice = await _unitOfWork.PurchaseInvoiceRepository
            .GetByWhere(x => x.Id == invoiceId)
            .Include(x => x.PurchaseOrders)
                .ThenInclude(o => o.PurchaseOrderDetails)
            .FirstOrDefaultAsync();

        if (invoice == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy hóa đơn nhập hàng");

        string currentStatus = invoice.Status;
        invoice.Status = status.ToString();

        if (status == OrderStatus.Done && currentStatus != OrderStatus.Done.ToString())
        {
            var pendingOrders = invoice.PurchaseOrders
                .Where(po => po.Status != OrderStatus.Done.ToString())
                .ToList();

            foreach (var order in invoice.PurchaseOrders)
            {
                order.Status = OrderStatus.Done.ToString();
                order.UpdatedAt = DateTime.Now;
            }

            if (invoice.PurchaseOrders.Any())
            {
                _unitOfWork.PurchaseOrderRepository.UpdateRange(invoice.PurchaseOrders);
            }

            var outstandingAmount = pendingOrders.Sum(po => po.SubTotal);

            if (outstandingAmount > 0)
            {
                var debtResult = await _unitOfWork.SupplierDebtRepository.AddSupplierTransactionAsync(
                    invoice.SupplierId,
                    "Purchase",
                    outstandingAmount,
                    $"Công nợ từ hóa đơn nhập hàng #{invoice.Code}");

                if (debtResult.StatusCode != Const.HTTP_STATUS_OK)
                {
                    return debtResult;
                }
            }
        }

        _unitOfWork.PurchaseInvoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, "Cập nhật trạng thái hóa đơn nhập hàng thành công");
    }

    public async Task<IBusinessResult> GetInvoiceListAsync(int pageIndex, int pageSize, string? search = null, DateTime? fromDate = null, DateTime? toDate = null, string? status = null, Guid? branchId = null, Guid? supplierId = null)
    {
        Expression<Func<PurchaseInvoice, bool>> filter = x => true;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLowerInvariant();
            filter = filter.And(x => x.Code.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusLower = status.Trim().ToLowerInvariant();
            filter = filter.And(x => x.Status.ToLower().Contains(statusLower));
        }

        if (fromDate.HasValue)
        {
            filter = filter.And(x => x.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            var endDate = toDate.Value.AddDays(1).AddSeconds(-1);
            filter = filter.And(x => x.OrderDate <= endDate);
        }

        if (supplierId.HasValue && supplierId.Value != Guid.Empty)
        {
            var supplierFilter = supplierId.Value;
            filter = filter.And(x => x.SupplierId == supplierFilter);
        }

        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            var branchFilter = branchId.Value;
            filter = filter.And(x => x.PurchaseOrders.Any(po => po.BranchId == branchFilter));
        }

        var invoices = await _unitOfWork.PurchaseInvoiceRepository
            .GetByWhere(filter)
            .Select(x => new PurchaseInvoiceList
            {
                Id = x.Id,
                Code = x.Code,
                OrderDate = x.OrderDate,
                Status = x.Status,
                SupplierId = x.SupplierId,
                SupplierName = x.Supplier != null ? x.Supplier.Name : null,
                TotalMoney = x.TotalMoney
            })
            .OrderByDescending(x => x.OrderDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (!invoices.Any())
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, invoices);
    }

    public async Task<IBusinessResult> GetInvoiceByIdAsync(Guid id)
    {
        var invoice = await _unitOfWork.PurchaseInvoiceRepository
            .GetByWhere(x => x.Id == id)
            .Include(x => x.Supplier)
            .Include(x => x.PurchaseOrders)
                .ThenInclude(o => o.PurchaseOrderDetails)
                    .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync();

        if (invoice == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy hóa đơn nhập hàng");

        var response = new PurchaseInvoiceResponse
        {
            Id = invoice.Id,
            Code = invoice.Code,
            OrderDate = invoice.OrderDate,
            Status = invoice.Status,
            SupplierId = invoice.SupplierId,
            SupplierName = invoice.Supplier?.Name,
            TotalMoney = invoice.TotalMoney,
            PurchaseOrders = invoice.PurchaseOrders.Select(order => new PurchaseOrderResponse
            {
                Id = order.Id,
                InvoiceCode = order.InvoiceCode,
                OrderDate = order.OrderDate,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                SubTotal = order.SubTotal,
                Note = order.Note,
                Status = order.Status,
                BranchName = order.Branch != null ? order.Branch.Name : null,
                EmployeeName = order.Employee != null ? order.Employee.Name : null,
                SupplierName = invoice.Supplier?.Name,
                PurchaseOrderDetails = order.PurchaseOrderDetails.Select(d => new PurchaseOrderDetailResponse
                {
                    Id = d.Id,
                    Quantity = d.Quantity,
                    ActualQuantity = d.ActualQuantity,
                    ReFee = d.ReFee,
                    UnitPrice = d.UnitPrice,
                    Note = d.Note,
                    ProductName = d.Product != null ? d.Product.Name : null,
                    Sku = d.Product != null ? d.Product.Sku : string.Empty
                }).ToList()
            }).ToList()
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> DeleteInvoiceAsync(Guid id)
    {
        var invoice = await _unitOfWork.PurchaseInvoiceRepository
            .GetByWhere(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (invoice == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy hóa đơn nhập hàng");

        if (invoice.Status == OrderStatus.Done.ToString())
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không thể xóa hóa đơn đã hoàn thành");

        _unitOfWork.PurchaseInvoiceRepository.Delete(invoice);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }

    private async Task<string> GeneratePurchaseInvoiceCodeAsync()
    {
        string datePart = DateTime.Now.ToString("yyyyMMdd");
        string prefix = "PI" + datePart;

        var latest = await _unitOfWork.PurchaseInvoiceRepository
            .GetByWhere(x => x.Code.StartsWith(prefix))
            .OrderByDescending(x => x.Code)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (latest != null)
        {
            string lastNumberPart = latest.Code.Substring(prefix.Length);
            if (int.TryParse(lastNumberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    private async Task<string> GeneratePurchaseOrderCodeAsync()
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
}

