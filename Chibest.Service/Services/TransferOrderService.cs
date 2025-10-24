using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Common.Enums;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Linq.Expressions;
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
                TransferOrderId = transferOrder.Id,
                ProductId = detailReq.ProductId,
                Quantity = detailReq.Quantity,
                UnitPrice = detailReq.UnitPrice,
                ExtraFee = detailReq.ExtraFee,
                CommissionFee = detailReq.CommissionFee,
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
                        Quantity = d.Quantity,
                        ActualQuantity = d.ActualQuantity,
                        CommissionFee = d.CommissionFee,
                        ExtraFee = d.ExtraFee,
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

            transferOrder.Status = request.Status.ToString();
            transferOrder.UpdatedAt = DateTime.Now;

            await _unitOfWork.BeginTransaction();

            try
            {
                _unitOfWork.TransferOrderRepository.Update(transferOrder);
                await _unitOfWork.SaveChangesAsync();
                if (request.Status == OrderStatus.Received)
                {
                    foreach (var detail in transferOrder.TransferOrderDetails)
                    {
                        if (detail.ActualQuantity.HasValue && detail.ActualQuantity.Value > 0)
                        {
                            int qty = detail.ActualQuantity.Value;

                            var decreaseResult = await _unitOfWork.BranchStockRepository.UpdateBranchStockAsync(
                                warehouseId: (Guid)transferOrder.FromWarehouseId,
                                productId: detail.ProductId,
                                deltaAvailableQty: -qty
                            );

                            if (decreaseResult.StatusCode != Const.SUCCESS)
                            {
                                await _unitOfWork.RollbackTransaction();
                                return new BusinessResult(Const.ERROR_EXCEPTION,
                                    $"Lỗi khi giảm tồn kho tại kho nguồn cho sản phẩm {detail.ProductId}: {decreaseResult.Message}");
                            }
                            var increaseResult = await _unitOfWork.BranchStockRepository.UpdateBranchStockAsync(
                                warehouseId: (Guid)transferOrder.ToWarehouseId,
                                productId: detail.ProductId,
                                deltaAvailableQty: qty
                            );

                            if (increaseResult.StatusCode != Const.SUCCESS)
                            {
                                await _unitOfWork.RollbackTransaction();
                                return new BusinessResult(Const.ERROR_EXCEPTION,
                                    $"Lỗi khi tăng tồn kho tại kho đích cho sản phẩm {detail.ProductId}: {increaseResult.Message}");
                            }
                        }
                    }
                }

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
            string randomPart = new Random().Next(100, 999).ToString();
            return $"{prefix}{nextNumber:D4}{randomPart}";
        }

        public async Task<IBusinessResult> ReadTransferDetailFromExcel(IFormFile file)
        {
            ExcelPackage.License.SetNonCommercialOrganization("Chibest");
            var result = new List<TransferOrderDetailResponse>();

            if (file == null || file.Length == 0)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "File không hợp lệ hoặc trống.");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var sheet = package.Workbook.Worksheets["TransferDetailTemplate"];
                    if (sheet == null)
                        return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không tìm thấy sheet 'TransferDetailTemplate' trong file Excel.");

                    int startRow = 2;
                    int row = startRow;

                    while (true)
                    {
                        var productCode = sheet.Cells[row, 1].Text?.Trim();
                        if (string.IsNullOrEmpty(productCode))
                            break; 

                        var quantity = ParseInt(sheet.Cells[row, 2].Text);
                        var transferPrice = ParseDecimal(sheet.Cells[row, 3].Text);
                        var commissionFee = ParseDecimal(sheet.Cells[row, 4].Text);
                        var extrafee = ParseDecimal(sheet.Cells[row, 5].Text);

                        var product = await _unitOfWork.ProductRepository.GetByWhere(x => x.Sku == productCode).FirstOrDefaultAsync();

                        if (product == null)
                        {
                            Console.WriteLine($"⚠️ Không tìm thấy sản phẩm có mã: {productCode}");
                            row++;
                            continue;
                        }

                        result.Add(new TransferOrderDetailResponse
                        {
                            ProductName = product.Name,
                            Sku = product.Sku,
                            Quantity = quantity,
                            UnitPrice = transferPrice,
                            CommissionFee = commissionFee,
                            ExtraFee = extrafee
                        });

                        row++;
                    }
                }
            }

            return new BusinessResult(Const.HTTP_STATUS_OK, "Đọc file thành công.", result);
        }

        private decimal ParseDecimal(string input)
            => decimal.TryParse(input, out var value) ? value : 0;

        private int ParseInt(string input)
            => int.TryParse(input, out var value) ? value : 0;

    }
}


