using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Common.Enums;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.FranchiseOrder;
using Chibest.Service.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chibest.Service.Services;

public class FranchiseInvoiceService : IFranchiseInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;

    public FranchiseInvoiceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> CreateInvoiceAsync(FranchiseInvoiceCreate request)
    {
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Invalid request");

        if (request.BranchId == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Vui lòng chọn chi nhánh");

        if (request.FranchiseOrders == null || !request.FranchiseOrders.Any())
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Hóa đơn cần ít nhất một đơn franchise");

        var branch = await _unitOfWork.BranchRepository
            .GetByWhere(b => b.Id == request.BranchId)
            .Select(b => new { b.Id, b.IsFranchise })
            .FirstOrDefaultAsync();

        if (branch == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy chi nhánh");

        if (branch.IsFranchise)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Chỉ chi nhánh trực thuộc mới được tạo hóa đơn này");

        string invoiceCode = string.IsNullOrWhiteSpace(request.Code)
            ? await GenerateFranchiseInvoiceCodeAsync()
            : request.Code!;

        DateTime invoiceDate = request.OrderDate == default ? DateTime.Now : request.OrderDate;
        string invoiceStatus = string.IsNullOrWhiteSpace(request.Status) ? OrderStatus.Draft.ToString() : request.Status!;

        var franchiseInvoice = new FranchiseInvoice
        {
            Id = Guid.NewGuid(),
            Code = invoiceCode,
            OrderDate = invoiceDate,
            Status = invoiceStatus,
            BranchId = request.BranchId
        };

        var orders = new List<FranchiseOrder>();
        var orderDetails = new List<FranchiseOrderDetail>();

        foreach (var orderReq in request.FranchiseOrders)
        {
            if (orderReq.FranchiseOrderDetails == null || !orderReq.FranchiseOrderDetails.Any())
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Mỗi đơn franchise cần ít nhất một sản phẩm");

            var branchId = orderReq.BranchId == Guid.Empty ? request.BranchId : orderReq.BranchId;

            string orderCode = string.IsNullOrWhiteSpace(orderReq.InvoiceCode)
                ? await GenerateFranchiseOrderCodeAsync()
                : orderReq.InvoiceCode!;

            decimal orderTotal = orderReq.TotalMoney > 0
                ? orderReq.TotalMoney
                : CalculateOrderTotal(orderReq.FranchiseOrderDetails);

            var order = new FranchiseOrder
            {
                Id = Guid.NewGuid(),
                InvoiceCode = orderCode,
                OrderDate = orderReq.OrderDate,
                BranchId = branchId,
                TotalMoney = orderTotal,
                Status = OrderStatus.Draft.ToString(),
                FranchiseInvoiceId = franchiseInvoice.Id
            };

            var details = orderReq.FranchiseOrderDetails.Select(detail => new FranchiseOrderDetail
            {
                Id = Guid.NewGuid(),
                FranchiseOrderId = order.Id,
                ProductId = detail.ProductId,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                CommissionFee = detail.CommissionFee,
                Note = detail.Note
            }).ToList();

            orders.Add(order);
            orderDetails.AddRange(details);
        }

        franchiseInvoice.TotalMoney = orders.Sum(x => x.TotalMoney);

        await _unitOfWork.FranchiseInvoiceRepository.AddAsync(franchiseInvoice);
        await _unitOfWork.FranchiseOrderRepository.AddRangeAsync(orders);

        if (orderDetails.Any())
        {
            await _unitOfWork.FranchiseOrderDetailRepository.AddRangeAsync(orderDetails);
        }

        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, "Tạo hóa đơn franchise thành công", new
        {
            InvoiceCode = franchiseInvoice.Code,
            FranchiseOrderCodes = orders.Select(o => o.InvoiceCode).ToList()
        });
    }

    public async Task<IBusinessResult> UpdateInvoiceStatusAsync(Guid invoiceId, OrderStatus status)
    {
        var invoice = await _unitOfWork.FranchiseInvoiceRepository
            .GetByWhere(x => x.Id == invoiceId)
            .Include(x => x.FranchiseOrders)
                .ThenInclude(o => o.FranchiseOrderDetails)
            .FirstOrDefaultAsync();

        if (invoice == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy hóa đơn franchise");

        string currentStatus = invoice.Status;
        invoice.Status = status.ToString();

        if (status == OrderStatus.Done && currentStatus != OrderStatus.Done.ToString())
        {
            var pendingOrders = invoice.FranchiseOrders
                .Where(o => o.Status != OrderStatus.Done.ToString())
                .ToList();

            foreach (var order in pendingOrders)
            {
                var adjustmentResult = await AdjustBranchStockAsync(order);
                if (adjustmentResult.StatusCode != Const.HTTP_STATUS_OK)
                    return adjustmentResult;

                order.Status = OrderStatus.Done.ToString();
            }

            if (pendingOrders.Any())
            {
                _unitOfWork.FranchiseOrderRepository.UpdateRange(pendingOrders);
                var outstandingAmount = pendingOrders.Sum(o => o.TotalMoney);

                if (outstandingAmount > 0)
                {
                    var debtResult = await _unitOfWork.BranchDebtRepository.AddBranchTransactionAsync(
                        invoice.BranchId,
                        "TransferIn",
                        outstandingAmount,
                        $"Công nợ từ hóa đơn franchise #{invoice.Code}");

                    if (debtResult.StatusCode != Const.HTTP_STATUS_OK)
                        return debtResult;
                }
            }
        }

        _unitOfWork.FranchiseInvoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, "Cập nhật trạng thái hóa đơn franchise thành công");
    }

    public async Task<IBusinessResult> GetInvoiceListAsync(int pageIndex, int pageSize, string? search = null, DateTime? fromDate = null, DateTime? toDate = null, string? status = null, Guid? branchId = null)
    {
        Expression<Func<FranchiseInvoice, bool>> filter = x => true;

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

        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            var branchFilter = branchId.Value;
            filter = filter.And(x => x.BranchId == branchFilter);
        }

        var invoices = await _unitOfWork.FranchiseInvoiceRepository
            .GetByWhere(filter)
            .Select(x => new FranchiseInvoiceList
            {
                Id = x.Id,
                Code = x.Code,
                OrderDate = x.OrderDate,
                Status = x.Status,
                BranchId = x.BranchId,
                BranchName = x.Branch != null ? x.Branch.Name : null,
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
        var invoice = await _unitOfWork.FranchiseInvoiceRepository
            .GetByWhere(x => x.Id == id)
            .Include(x => x.Branch)
            .Include(x => x.FranchiseOrders)
                .ThenInclude(o => o.FranchiseOrderDetails)
                    .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync();

        if (invoice == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy hóa đơn franchise");

        var response = new FranchiseInvoiceResponse
        {
            Id = invoice.Id,
            Code = invoice.Code,
            OrderDate = invoice.OrderDate,
            Status = invoice.Status,
            BranchId = invoice.BranchId,
            BranchName = invoice.Branch?.Name,
            TotalMoney = invoice.TotalMoney,
            FranchiseOrders = invoice.FranchiseOrders.Select(MapFranchiseOrder).ToList()
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> DeleteInvoiceAsync(Guid id)
    {
        var invoice = await _unitOfWork.FranchiseInvoiceRepository
            .GetByWhere(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (invoice == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy hóa đơn franchise");

        if (invoice.Status == OrderStatus.Done.ToString())
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không thể xóa hóa đơn đã hoàn thành");

        _unitOfWork.FranchiseInvoiceRepository.Delete(invoice);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }

    private async Task<string> GenerateFranchiseInvoiceCodeAsync()
    {
        string datePart = DateTime.Now.ToString("yyyyMMdd");
        string prefix = "FI" + datePart;

        var latest = await _unitOfWork.FranchiseInvoiceRepository
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

    private async Task<string> GenerateFranchiseOrderCodeAsync()
    {
        string datePart = DateTime.Now.ToString("yyyyMMdd");
        string prefix = "FO" + datePart;

        var latest = await _unitOfWork.FranchiseOrderRepository
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

    private static decimal CalculateOrderTotal(IEnumerable<FranchiseOrderDetailCreate> details)
    {
        if (details == null)
            return 0m;

        decimal total = 0m;

        foreach (var detail in details)
        {
            total += (detail.UnitPrice + detail.CommissionFee) * detail.Quantity;
        }

        return total;
    }

    private async Task<IBusinessResult> AdjustBranchStockAsync(FranchiseOrder order)
    {
        if (order.FranchiseOrderDetails == null || !order.FranchiseOrderDetails.Any())
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);

        foreach (var detail in order.FranchiseOrderDetails)
        {
            var actual = detail.ActualQuantity ?? detail.Quantity;
            if (actual <= 0)
                continue;

            var result = await _unitOfWork.BranchStockRepository.UpdateBranchStockAsync(
                branchId: order.BranchId,
                productId: detail.ProductId,
                deltaAvailableQty: -actual);

            if (result.StatusCode != Const.HTTP_STATUS_OK)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION,
                    $"Lỗi cập nhật tồn kho cho sản phẩm {detail.ProductId}: {result.Message}");
            }
        }

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
    }

    private static FranchiseOrderResponse MapFranchiseOrder(FranchiseOrder order)
    {
        return new FranchiseOrderResponse
        {
            Id = order.Id,
            InvoiceCode = order.InvoiceCode,
            OrderDate = order.OrderDate,
            TotalMoney = order.TotalMoney,
            Status = order.Status,
            BranchName = order.Branch?.Name,
            FranchiseOrderDetails = order.FranchiseOrderDetails.Select(detail => new FranchiseOrderDetailResponse
            {
                Id = detail.Id,
                Quantity = detail.Quantity,
                ActualQuantity = detail.ActualQuantity,
                UnitPrice = detail.UnitPrice,
                CommissionFee = detail.CommissionFee,
                Note = detail.Note,
                ProductName = detail.Product != null ? detail.Product.Name : null,
                Sku = detail.Product != null ? detail.Product.Sku : string.Empty
            }).ToList()
        };
    }
}

