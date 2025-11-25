using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Common.Enums;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.FranchiseOrder;
using Microsoft.EntityFrameworkCore;
using Chibest.Service.Utilities;
using System.Linq.Expressions;

namespace Chibest.Service.Services;

public class FranchiseService : IFranchiseService
{
    private readonly IUnitOfWork _unitOfWork;

    public FranchiseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> CreateFranchiseOrderAsync(FranchiseOrderCreate request)
    {
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Invalid request");

        if (request.BranchId == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Vui lòng chọn chi nhánh franchise");

        if (request.FranchiseOrderDetails == null || !request.FranchiseOrderDetails.Any())
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Đơn franchise phải có ít nhất một sản phẩm");

        var branch = await _unitOfWork.BranchRepository
            .GetByWhere(b => b.Id == request.BranchId)
            .Select(b => new { b.Id, b.IsFranchise, b.Name })
            .FirstOrDefaultAsync();

        if (branch == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy chi nhánh");

        string invoiceCode = string.IsNullOrWhiteSpace(request.InvoiceCode)
            ? await GenerateFranchiseOrderCodeAsync()
            : request.InvoiceCode!;

        decimal totalMoney = request.TotalMoney > 0
            ? request.TotalMoney
            : CalculateOrderTotal(request.FranchiseOrderDetails);

        var franchiseInvoice = new FranchiseInvoice
        {
            Id = Guid.NewGuid(),
            Code = await GenerateFranchiseInvoiceCodeAsync(),
            OrderDate = request.OrderDate,
            Status = OrderStatus.Draft.ToString(),
            BranchId = request.SellerId
        };

        var franchiseOrder = new FranchiseOrder
        {
            Id = Guid.NewGuid(),
            InvoiceCode = invoiceCode,
            OrderDate = request.OrderDate,
            TotalMoney = totalMoney,
            Status = OrderStatus.Draft.ToString(),
            BranchId = request.BranchId,
            FranchiseInvoiceId = franchiseInvoice.Id
        };

        var details = request.FranchiseOrderDetails.Select(detail => new FranchiseOrderDetail
        {
            Id = Guid.NewGuid(),
            FranchiseOrderId = franchiseOrder.Id,
            ProductId = detail.ProductId,
            Quantity = detail.Quantity,
            UnitPrice = detail.UnitPrice,
            CommissionFee = detail.CommissionFee,
            Note = detail.Note
        }).ToList();

        franchiseInvoice.TotalMoney = franchiseOrder.TotalMoney;

        await _unitOfWork.FranchiseInvoiceRepository.AddAsync(franchiseInvoice);
        await _unitOfWork.FranchiseOrderRepository.AddAsync(franchiseOrder);
        await _unitOfWork.FranchiseOrderDetailRepository.AddRangeAsync(details);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_CREATE_MSG, new { franchiseOrder.InvoiceCode });
    }

    public async Task<IBusinessResult> UpdateFranchiseOrderAsync(Guid id, FranchiseOrderUpdate request)
    {
        var order = await _unitOfWork.FranchiseOrderRepository
            .GetByWhere(x => x.Id == id)
            .Include(x => x.FranchiseOrderDetails)
            .FirstOrDefaultAsync();

        if (order == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy đơn franchise");

        var oldStatus = order.Status;

        foreach (var detailReq in request.FranchiseOrderDetails)
        {
            var detail = order.FranchiseOrderDetails.FirstOrDefault(d => d.Id == detailReq.Id);
            if (detail != null)
            {
                detail.ActualQuantity = detailReq.ActualQuantity ?? detail.ActualQuantity;
                detail.UnitPrice = detailReq.UnitPrice;
                detail.CommissionFee = detailReq.CommissionFee;
                detail.Note = detailReq.Note;
            }
        }

        order.TotalMoney = request.TotalMoney > 0 ? request.TotalMoney : CalculateOrderTotal(order.FranchiseOrderDetails);
        order.Status = request.Status.ToString();

        _unitOfWork.FranchiseOrderRepository.Update(order);
        _unitOfWork.FranchiseOrderDetailRepository.UpdateRange(order.FranchiseOrderDetails.ToList());

        if (request.Status == OrderStatus.Done && oldStatus != OrderStatus.Done.ToString())
        {
            var stockResult = await AdjustBranchStockAsync(order);
            if (stockResult.StatusCode != Const.HTTP_STATUS_OK)
                return stockResult;

            if (order.TotalMoney > 0)
            {
                var debtResult = await _unitOfWork.BranchDebtRepository.AddBranchTransactionAsync(
                    order.BranchId,
                    "TransferIn",
                    order.TotalMoney,
                    $"Công nợ từ hoá đơn uỷ quyền #{order.InvoiceCode}");

                if (debtResult.StatusCode != Const.HTTP_STATUS_OK)
                    return debtResult;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return new BusinessResult(Const.HTTP_STATUS_OK, "Cập nhật đơn uỷ quyền thành công");
    }

    public async Task<IBusinessResult> DeleteFranchiseOrderAsync(Guid id)
    {
        var order = await _unitOfWork.FranchiseOrderRepository
            .GetByWhere(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (order == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy đơn franchise");

        if (order.Status == OrderStatus.Done.ToString())
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không thể xóa đơn franchise đã hoàn thành");

        _unitOfWork.FranchiseOrderRepository.Delete(order);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, "Xóa đơn franchise thành công");
    }

    public async Task<IBusinessResult> GetFranchiseOrderByIdAsync(Guid id)
    {
        var order = await _unitOfWork.FranchiseOrderRepository
            .GetByWhere(x => x.Id == id)
            .Select(x => new FranchiseOrderResponse
            {
                Id = x.Id,
                InvoiceCode = x.InvoiceCode,
                OrderDate = x.OrderDate,
                TotalMoney = x.TotalMoney,
                Status = x.Status,
                BranchName = x.Branch != null ? x.Branch.Name : null,
                FranchiseOrderDetails = x.FranchiseOrderDetails.Select(d => new FranchiseOrderDetailResponse
                {
                    Id = d.Id,
                    Quantity = d.Quantity,
                    ActualQuantity = d.ActualQuantity,
                    UnitPrice = d.UnitPrice,
                    CommissionFee = d.CommissionFee,
                    Note = d.Note,
                    ProductName = d.Product != null ? d.Product.Name : null,
                    Sku = d.Product != null ? d.Product.Sku : string.Empty
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (order == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy đơn franchise");

        return new BusinessResult(Const.HTTP_STATUS_OK, "Lấy dữ liệu đơn franchise thành công", order);
    }

    public async Task<IBusinessResult> GetFranchiseOrderListAsync(int pageIndex, int pageSize, string? search = null, DateTime? fromDate = null, DateTime? toDate = null, string? status = null, Guid? branchId = null)
    {
        Expression<Func<FranchiseOrder, bool>> filter = x => true;

        if (!string.IsNullOrWhiteSpace(search))
        {
            string searchTerm = search.ToLower();
            filter = filter.And(x => x.InvoiceCode.ToLower().Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            string statusTerm = status.ToLower();
            filter = filter.And(x => x.Status.ToLower().Contains(statusTerm));
        }

        if (fromDate.HasValue)
        {
            filter = filter.And(x => x.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            DateTime endDate = toDate.Value.AddDays(1).AddSeconds(-1);
            filter = filter.And(x => x.OrderDate <= endDate);
        }

        if (branchId.HasValue && branchId.Value != Guid.Empty)
        {
            Guid branchFilter = branchId.Value;
            filter = filter.And(x => x.BranchId == branchFilter);
        }

        var orders = await _unitOfWork.FranchiseOrderRepository
            .GetByWhere(filter)
            .Select(order => new FranchiseOrderList
            {
                Id = order.Id,
                InvoiceCode = order.InvoiceCode,
                OrderDate = order.OrderDate,
                TotalMoney = order.TotalMoney,
                Status = order.Status,
                BranchName = order.Branch != null ? order.Branch.Name : null
            })
            .OrderByDescending(x => x.OrderDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (!orders.Any())
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, orders);
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

    private static decimal CalculateOrderTotal(IEnumerable<FranchiseOrderDetail> details)
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
}

