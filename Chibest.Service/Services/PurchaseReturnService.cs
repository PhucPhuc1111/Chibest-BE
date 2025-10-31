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
using static Chibest.Service.ModelDTOs.Stock.PurchaseReturn.create;
using static Chibest.Service.ModelDTOs.Stock.PurchaseReturn.id;
using static Chibest.Service.ModelDTOs.Stock.PurchaseReturn.list;

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
                SupplierId = request.SupplierId,
                SubTotal = request.SubTotal,
                Note = request.Note,
                WarehouseId = request.WarehouseId,
                EmployeeId = request.EmployeeId,
                Status = OrderStatus.Draft.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var returnDetails = request.PurchaseReturnDetails.Select(detailReq => new PurchaseReturnDetail
            {
                Id = Guid.NewGuid(),
                PurchaseReturnId = purchaseReturn.Id,
                ProductId = detailReq.ProductId,
                Quantity = detailReq.Quantity,
                UnitPrice = detailReq.UnitPrice,
                ReturnPrice = detailReq.ReturnPrice,
                Note = detailReq.Note
            }).ToList();

                await _unitOfWork.PurchaseReturnRepository.AddAsync(purchaseReturn);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.BulkInsertAsync(returnDetails);


                return new BusinessResult(Const.HTTP_STATUS_OK, "Tạo phiếu trả hàng thành công", new { purchaseReturn.InvoiceCode });
            
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
                    Note = x.Note,
                    Status = x.Status,

                    FromWarehouseName = x.Warehouse != null ? x.Warehouse.Name : null,
                    ToWarehouseName = x.Supplier != null ? x.Supplier.Name : null, 

                    PurchaseReturnDetails = x.PurchaseReturnDetails.Select(d => new PurchaseReturnDetailResponse
                    {
                        Id = d.Id,
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
                .Include(pr => pr.PurchaseReturnDetails)
                .FirstOrDefaultAsync();

            if (purchaseReturn == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu trả hàng");

            var oldStatus = purchaseReturn.Status;
            purchaseReturn.Status = status.ToString();
            purchaseReturn.UpdatedAt = DateTime.Now;


                _unitOfWork.PurchaseReturnRepository.Update(purchaseReturn);
                await _unitOfWork.SaveChangesAsync();

                if (status == OrderStatus.Received && oldStatus != OrderStatus.Received.ToString())
                {
                    foreach (var detail in purchaseReturn.PurchaseReturnDetails)
                    {
                        if (detail.Quantity > 0)
                        {
                            var decreaseResult = await _unitOfWork.BranchStockRepository.UpdateBranchStockAsync(
                                warehouseId: (Guid)purchaseReturn.WarehouseId,
                                productId: detail.ProductId,
                                deltaAvailableQty: -detail.Quantity
                            );

                            if (decreaseResult.StatusCode != Const.SUCCESS)
                            {
                                return new BusinessResult(Const.ERROR_EXCEPTION,
                                    $"Lỗi khi giảm tồn kho cho sản phẩm {detail.ProductId}: {decreaseResult.Message}");
                            }
                        }
                    }
                    if (purchaseReturn.SupplierId != null)
                    {
                        decimal subtotal = purchaseReturn.SubTotal;

                        if (subtotal != 0)
                        {
                            var debtResult = await _unitOfWork.SupplierDebtRepository.AddSupplierTransactionAsync(
                                supplierId: (Guid)purchaseReturn.SupplierId,
                                transactionType: "Return",
                                amount: subtotal,
                                note: $"Công nợ từ phiếu trả hàng #{purchaseReturn.InvoiceCode}"
                            );

                            if (debtResult.StatusCode != Const.HTTP_STATUS_OK)
                            {
                                return new BusinessResult(Const.ERROR_EXCEPTION,
                                    "Lỗi xử lý công nợ nhà cung cấp khi trả hàng");
                            }
                        }
                    }

                    if (purchaseReturn.WarehouseId != null)
                    {
                        var warehouse = await _unitOfWork.WarehouseRepository.GetByIdAsync((Guid)purchaseReturn.WarehouseId);
                        if (warehouse?.BranchId != null)
                        {
                            var branch = await _unitOfWork.BranchRepository.GetByIdAsync((Guid)warehouse.BranchId);
                            if (branch != null && branch.IsFranchise == false)
                            {
                                decimal subtotal = purchaseReturn.SubTotal;

                                if (subtotal != 0)
                                {
                                    var branchDebtResult = await _unitOfWork.BranchDebtRepository.AddBranchTransactionAsync(
                                        branchId: (Guid)warehouse.BranchId,
                                        transactionType: "Return",
                                        amount: subtotal,
                                        note: $"Công nợ chi nhánh từ phiếu trả hàng #{purchaseReturn.InvoiceCode}"
                                    );

                                    if (branchDebtResult.StatusCode != Const.HTTP_STATUS_OK)
                                    {
                                        return new BusinessResult(Const.ERROR_EXCEPTION,
                                            "Lỗi xử lý công nợ chi nhánh khi trả hàng");
                                    }
                                }
                            }
                        }
                    }
                }

                return new BusinessResult(Const.SUCCESS, "Cập nhật phiếu trả hàng và công nợ thành công");
            
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
            string randomPart = new Random().Next(100, 999).ToString();
            return $"{prefix}{nextNumber:D4}{randomPart}";
        }

        public async Task<IBusinessResult> ReadPurchaseReturnFromExcel(IFormFile file)
        {
            ExcelPackage.License.SetNonCommercialOrganization("Chibest");
            var result = new List<PurchaseReturnDetailResponse>();
            var errorRows = new List<string>();

            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ hoặc trống.");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var sheet = package.Workbook.Worksheets["PurchaseReturnTemplate"];
                    if (sheet == null)
                        throw new Exception("Không tìm thấy sheet 'PurchaseReturnTemplate' trong file Excel.");

                    int startRow = 2;
                    int row = startRow;

                    while (true)
                    {
                        var productCode = sheet.Cells[row, 1].Text?.Trim();
                        if (string.IsNullOrEmpty(productCode))
                            break;

                        try
                        {
                            var quantity = ParseInt(sheet.Cells[row, 3].Text);
                            var unitPrice = ParseDecimal(sheet.Cells[row, 4].Text);
                            var returnPrice = ParseDecimal(sheet.Cells[row, 5].Text);
                            var discount = ParseDecimal(sheet.Cells[row, 6].Text);

                            var product = await _unitOfWork.ProductRepository.GetByWhere(x => x.Sku == productCode).FirstOrDefaultAsync();
                            if (product == null)
                            {
                                errorRows.Add($"Dòng {row}: Không tìm thấy sản phẩm có mã '{productCode}'");
                                row++;
                                continue;
                            }

                            result.Add(new PurchaseReturnDetailResponse
                            {
                                Id = Guid.NewGuid(),
                                Sku = product.Sku,
                                ProductName = product.Name,
                                Quantity = quantity,
                                UnitPrice = unitPrice,
                                ReturnPrice = returnPrice,
                                Note = $"Giảm giá trả lại: {discount}"
                            });
                        }
                        catch (Exception ex)
                        {
                            errorRows.Add($"Dòng {row}: Lỗi xử lý dữ liệu ({ex.Message})");
                        }

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

        private decimal ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            value = value.Replace(",", "").Trim();
            return decimal.TryParse(value, out decimal result) ? result : 0;
        }

        private int ParseInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            value = value.Replace(",", "").Trim();
            return int.TryParse(value, out int result) ? result : 0; 
        }

        public async Task<IBusinessResult> DeletePurchaseReturn(Guid id)
        {
            var purchaseReturn = await _unitOfWork.PurchaseReturnRepository
                .GetByWhere(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (purchaseReturn == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu trả hàng");

            if (purchaseReturn.Status == OrderStatus.Received.ToString())
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không thể xóa phiếu trả hàng đã nhận");

            try
            {
                // Cascade delete will remove PurchaseReturnDetail records
                _unitOfWork.PurchaseReturnRepository.Delete(purchaseReturn);
                await _unitOfWork.SaveChangesAsync();

                return new BusinessResult(Const.HTTP_STATUS_OK, "Xóa phiếu trả hàng thành công");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Lỗi khi xóa phiếu trả hàng", ex.Message);
            }
        }
    }
}
