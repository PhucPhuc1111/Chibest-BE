using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Common.Enums;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Stock.PurchaseOrder;
using Chibest.Service.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
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
                ReFee = detailReq.ReFee,
                Note = detailReq.Note,
            }).ToList();

            await _unitOfWork.PurchaseOrderRepository.AddAsync(purchaseOrder);
            await _unitOfWork.PurchaseOrderDetailRepository.AddRangeAsync(orderDetails);

            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_CREATE_MSG, new { purchaseOrder.InvoiceCode });
            
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
                        Quantity = d.Quantity,
                        ActualQuantity = d.ActualQuantity,
                        ReFee = d.ReFee,
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
        public async Task<IBusinessResult> GetPurchaseOrderList(
    int pageIndex,
    int pageSize,
    string search,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    string status = null)
        {
            Expression<Func<PurchaseOrder, bool>> filter = x => true;

            if (!string.IsNullOrEmpty(search))
            {
                string searchTerm = search.ToLower();
                Expression<Func<PurchaseOrder, bool>> searchFilter =
                    x => x.InvoiceCode.ToLower().Contains(searchTerm)
                      || (x.Supplier != null && x.Supplier.Name.ToLower().Contains(searchTerm));
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

            var POs = await _unitOfWork.PurchaseOrderRepository
                .GetByWhere(filter)
                .Select(pos => new PurchaseOrderList
                {
                    Id = pos.Id,
                    InvoiceCode = pos.InvoiceCode,
                    OrderDate = pos.OrderDate,
                    SubTotal = pos.SubTotal,
                    Status = pos.Status,
                    SupplierName = pos.Supplier != null ? pos.Supplier.Name : null
                })
                .OrderByDescending(x => x.OrderDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (POs == null || !POs.Any())
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, POs);
        }

        public async Task<IBusinessResult> UpdateAsync(Guid id, PurchaseOrderUpdate request)
        {
            var purchaseOrder = await _unitOfWork.PurchaseOrderRepository
                .GetByWhere(x => x.Id == id)
                .Include(po => po.PurchaseOrderDetails)
                .FirstOrDefaultAsync();

            if (purchaseOrder == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu nhập hàng");

            foreach (var detailReq in request.PurchaseOrderDetails)
            {
                var detail = purchaseOrder.PurchaseOrderDetails
                    .FirstOrDefault(x => x.Id == detailReq.Id);

                if (detail != null)
                {
                    detail.ActualQuantity = detailReq.ActualQuantity ?? detail.ActualQuantity;
                    detail.ReFee = detailReq.ReFee;
                    detail.UnitPrice = detailReq.UnitPrice;
                    detail.Discount = detailReq.Discount;
                    detail.Note = detailReq.Note;
                }
            }
            purchaseOrder.SubTotal = request.SubTotal;
            purchaseOrder.PayMethod = request.PayMethod;
            purchaseOrder.DiscountAmount = request.DiscountAmount;
            purchaseOrder.Paid = request.Paid;
            purchaseOrder.Status = request.Status.ToString();
            purchaseOrder.UpdatedAt = DateTime.Now;


                _unitOfWork.PurchaseOrderRepository.Update(purchaseOrder);

                if (request.Status == OrderStatus.Received)
                {
                    foreach (var detail in purchaseOrder.PurchaseOrderDetails)
                    {
                        if (detail.ActualQuantity.HasValue && detail.ActualQuantity.Value > 0)
                        {
                            var result = await _unitOfWork.BranchStockRepository.UpdateBranchStockAsync(
                                warehouseId: (Guid)purchaseOrder.WarehouseId,
                                productId: detail.ProductId,
                                deltaAvailableQty: detail.ActualQuantity.Value
                            );

                            if (result.StatusCode != Const.SUCCESS)
                            {
                                return new BusinessResult(Const.ERROR_EXCEPTION,
                                    $"Lỗi cập nhật tồn kho cho sản phẩm {detail.ProductId}: {result.Message}");
                            }
                        }
                    }
                    decimal subtotal = purchaseOrder.SubTotal;
                    decimal paid = purchaseOrder.Paid;
                    decimal debtAmount = subtotal - paid;
                    if (debtAmount != 0 && purchaseOrder.SupplierId != null)
                    {
                        var debtResult = await _unitOfWork.SupplierDebtRepository.AddSupplierTransactionAsync(
                            supplierId: (Guid)purchaseOrder.SupplierId,
                            transactionType: "Purchase",
                            amount: debtAmount,
                            note: $"Công nợ từ phiếu nhập #{purchaseOrder.InvoiceCode}"
                        );

                        if (debtResult.StatusCode != Const.HTTP_STATUS_OK)
                        {
                            return new BusinessResult(Const.ERROR_EXCEPTION,
                                "Lỗi xử lý công nợ nhà cung cấp");
                        }
                    }
                }
                var detailsToUpdate = purchaseOrder.PurchaseOrderDetails.ToList();
                _unitOfWork.PurchaseOrderDetailRepository.UpdateRange(detailsToUpdate);

            await _unitOfWork.SaveChangesAsync();
            return new BusinessResult(Const.SUCCESS, "Cập nhật phiếu nhập hàng và công nợ thành công");
            
        }

        public async Task<IBusinessResult> DeletePurchaseOrder(Guid id)
        {
            var purchaseOrder = await _unitOfWork.PurchaseOrderRepository
                .GetByWhere(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (purchaseOrder == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu nhập hàng");

            if (purchaseOrder.Status == OrderStatus.Received.ToString())
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không thể xóa phiếu nhập hàng đã nhận");

            try
            {
                _unitOfWork.PurchaseOrderRepository.Delete(purchaseOrder);
                await _unitOfWork.SaveChangesAsync();

                return new BusinessResult(Const.HTTP_STATUS_OK, "Xóa phiếu nhập hàng thành công");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Lỗi khi xóa phiếu nhập hàng", ex.Message);
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

        public async Task<IBusinessResult> ReadPurchaseOrderFromExcel(IFormFile file)
        {
            ExcelPackage.License.SetNonCommercialOrganization("Chibest");
            var result = new List<PurchaseOrderDetailResponse>();
            var errorRows = new List<string>();

            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ hoặc trống.");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var sheet = package.Workbook.Worksheets["PurchaseOrderTemplate"];
                    if (sheet == null)
                        throw new Exception("Không tìm thấy sheet 'PurchaseOrderTemplate' trong file Excel.");

                    int startRow = 2;
                    int row = startRow;

                    while (true)
                    {
                        var productCode = sheet.Cells[row, 1].Text?.Trim();
                        if (string.IsNullOrEmpty(productCode))
                            break;

                        
                            var unitPrice = ParseDecimal(sheet.Cells[row, 2].Text);
                            var discount = ParseDecimal(sheet.Cells[row, 3].Text);
                            var reFee = ParseDecimal(sheet.Cells[row, 4].Text);
                            var quantity = ParseInt(sheet.Cells[row, 5].Text);

                            var product = await  _unitOfWork.ProductRepository.GetByWhere(x => x.Sku == productCode).FirstOrDefaultAsync();
                            if (product == null)
                            {
                                errorRows.Add($"Dòng {row}: Không tìm thấy sản phẩm có mã '{productCode}'");
                                row++;
                                continue; 
                            }

                            result.Add(new PurchaseOrderDetailResponse
                            {
                                Id = product.Id,
                                ProductName = product.Name,
                                Sku = product.Sku,
                                UnitPrice = unitPrice,
                                Discount = discount,
                                ReFee = reFee,
                                Quantity = quantity
                            });
                        

                        row++;
                    }
                }
            }

            var message = Const.SUCCESS_READ_MSG;
            if (errorRows.Any())
            {
                message += $" (Có {errorRows.Count} dòng bị bỏ qua)";
                foreach (var err in errorRows)
                    Console.WriteLine(err);
            }

            return new BusinessResult(Const.HTTP_STATUS_OK, message, result);
        }


        private decimal ParseDecimal(string input)
            => decimal.TryParse(input, out var value) ? value : 0;

        private int ParseInt(string input)
            => int.TryParse(input, out var value) ? value : 0;

    }
}
