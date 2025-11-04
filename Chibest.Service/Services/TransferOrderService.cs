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
        public async Task<IBusinessResult> AddMultiTransferOrder(TransferMultiOrderCreate request)
        {
            if (request == null || request.Destinations == null || !request.Destinations.Any())
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Invalid request");

            var createdOrders = new List<object>();

            try
            {
                foreach (var dest in request.Destinations)
                {
                    var singleRequest = new TransferOrderCreate
                    {
                        FromWarehouseId = request.FromWarehouseId,
                        ToWarehouseId = dest.ToWarehouseId,
                        EmployeeId = request.EmployeeId,
                        OrderDate = request.OrderDate,
                        DiscountAmount = request.DiscountAmount,
                        SubTotal = request.SubTotal,
                        Paid = request.Paid,
                        Note = request.Note,
                        PayMethod = request.PayMethod,
                        TransferOrderDetails = dest.Products.Select(p => new TransferOrderDetailCreate
                        {
                            ProductId = p.ProductId,
                            Quantity = p.Quantity,
                            UnitPrice = p.UnitPrice,
                            ExtraFee = p.ExtraFee,
                            CommissionFee = p.CommissionFee,
                            Discount = p.Discount,
                            Note = p.Note
                        }).ToList()
                    };

                    var result = await AddTransferOrder(singleRequest);

                    if (result.StatusCode != Const.HTTP_STATUS_OK)
                    {
                        return new BusinessResult(Const.ERROR_EXCEPTION,
                            $"Error creating order for warehouse {dest.ToWarehouseId}", result.Message);
                    }

                    createdOrders.Add(result.Data);
                }

                return new BusinessResult(Const.HTTP_STATUS_OK, "All transfer orders created successfully", createdOrders);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Error creating multi transfer orders", ex.Message);
            }
        }

        public async Task<IBusinessResult> UpdateTransferOrderAsync(Guid id, TransferOrderUpdate request)
        {
            var transferOrder = await _unitOfWork.TransferOrderRepository
                .GetByWhere(x => x.Id == id)
                .Include(x => x.FromWarehouse)
                    .ThenInclude(w => w.Branch)
                .Include(x => x.ToWarehouse)
                    .ThenInclude(w => w.Branch)
                .Include(x => x.TransferOrderDetails)
                .FirstOrDefaultAsync();

            if (transferOrder == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu chuyển kho");

            var fromBranch = new { Id = transferOrder.FromWarehouse.Branch.Id, IsFranchise = transferOrder.FromWarehouse.Branch.IsFranchise };
            var toBranch = new { Id = transferOrder.ToWarehouse.Branch.Id, IsFranchise = transferOrder.ToWarehouse.Branch.IsFranchise };
            var oldStatus = transferOrder.Status;
            foreach (var detailReq in request.TransferOrderDetails)
            {
                var detail = transferOrder.TransferOrderDetails
                    .FirstOrDefault(x => x.Id == detailReq.Id);

                if (detail != null)
                {
                    detail.ActualQuantity = detailReq.ActualQuantity ?? detail.ActualQuantity;
                    detail.UnitPrice = detailReq.UnitPrice;
                    detail.CommissionFee = detailReq.CommissionFee;
                    detail.ExtraFee = detailReq.ExtraFee;
                    detail.Note = detailReq.Note;
                }
            }
            transferOrder.PayMethod = request.PayMethod;
            transferOrder.Status = request.Status.ToString();
            transferOrder.SubTotal = request.SubTotal;
            transferOrder.DiscountAmount = request.DiscountAmount;
            transferOrder.Paid = request.Paid;
            transferOrder.UpdatedAt = DateTime.Now;

                _unitOfWork.TransferOrderRepository.Update(transferOrder);
                await _unitOfWork.SaveChangesAsync();

                if (request.Status == OrderStatus.Received && oldStatus != OrderStatus.Received.ToString())
                {
                    var debt = transferOrder.SubTotal - transferOrder.Paid;
                    var note = $"Công nợ từ phiếu chuyển kho #{transferOrder.InvoiceCode}";

                    if (fromBranch.IsFranchise && !toBranch.IsFranchise)
                    {
                        var debtResult = await _unitOfWork.BranchDebtRepository.AddBranchTransactionAsync(
                            fromBranch.Id, "Return", debt, note
                        );

                        if (debtResult.StatusCode != Const.HTTP_STATUS_OK)
                        {
                            return new BusinessResult(Const.ERROR_EXCEPTION,
                                "Lỗi xử lý công nợ chi nhánh nhận hàng");
                        }
                    }
                    if (!fromBranch.IsFranchise && toBranch.IsFranchise)
                    {
                        var debtOutResult = await _unitOfWork.BranchDebtRepository.AddBranchTransactionAsync(
                            toBranch.Id, "Transfer", debt, note
                        );

                        if (debtOutResult.StatusCode != Const.HTTP_STATUS_OK)
                        {
                            return new BusinessResult(Const.ERROR_EXCEPTION,
                                "Lỗi xử lý công nợ chi nhánh chuyển hàng");
                        }
                    }

                    foreach (var detail in transferOrder.TransferOrderDetails)
                    {
                        if (detail.ActualQuantity.HasValue && detail.ActualQuantity.Value > 0)
                        {
                            int qty = detail.ActualQuantity.Value;

                            var increaseResult = await _unitOfWork.BranchStockRepository.UpdateBranchStockAsync(
                                warehouseId: (Guid)transferOrder.ToWarehouseId,
                                productId: detail.ProductId,
                                deltaAvailableQty: qty
                            );

                            if (increaseResult.StatusCode != Const.HTTP_STATUS_OK)
                            {
                                return new BusinessResult(Const.ERROR_EXCEPTION,
                                    $"Lỗi khi tăng tồn kho tại kho đích cho sản phẩm {detail.ProductId}: {increaseResult.Message}");
                            }
                        }
                    }
                }

                _unitOfWork.TransferOrderDetailRepository.UpdateRange(transferOrder.TransferOrderDetails.ToList());
                 await _unitOfWork.SaveChangesAsync();
                return new BusinessResult(Const.HTTP_STATUS_OK, "Cập nhật phiếu chuyển kho thành công");
            
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

                await _unitOfWork.TransferOrderRepository.AddAsync(transferOrder);
                await _unitOfWork.TransferOrderDetailRepository.AddRangeAsync(transferDetails);
            await _unitOfWork.SaveChangesAsync();

            foreach (var detail in transferDetails)
                {
                    var decreaseResult = await _unitOfWork.BranchStockRepository.UpdateBranchStockAsync(
                        warehouseId: (Guid)transferOrder.FromWarehouseId,
                        productId: detail.ProductId,
                        deltaAvailableQty: -detail.Quantity
                    );

                    if (decreaseResult.StatusCode != Const.HTTP_STATUS_OK)
                    {
                        return new BusinessResult(Const.ERROR_EXCEPTION,
                            $"Lỗi khi trừ tồn kho tại kho nguồn cho sản phẩm {detail.ProductId}: {decreaseResult.Message}");
                    }
                }

                return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_CREATE_MSG, new { transferOrder.InvoiceCode });
            
        }

        public async Task<IBusinessResult> GetTransferOrderList(
    int pageIndex,
    int pageSize,
    string search,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    string status = null,
    Guid? fromWarehouseId = null,    
    Guid? toWarehouseId = null)     
        {
            Expression<Func<TransferOrder, bool>> filter = x => true;

            if (!string.IsNullOrEmpty(search))
            {
                string searchTerm = search.ToLower();
                Expression<Func<TransferOrder, bool>> searchFilter =
                    x => x.InvoiceCode.ToLower().Contains(searchTerm)
                      || (x.FromWarehouse != null && x.FromWarehouse.Name.ToLower().Contains(searchTerm))
                      || (x.ToWarehouse != null && x.ToWarehouse.Name.ToLower().Contains(searchTerm));
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

            if (fromWarehouseId.HasValue && fromWarehouseId.Value != Guid.Empty)
            {
                Expression<Func<TransferOrder, bool>> fromWarehouseFilter =
                    x => x.FromWarehouse != null && x.FromWarehouse.Id == fromWarehouseId.Value;
                filter = filter.And(fromWarehouseFilter);
            }

            if (toWarehouseId.HasValue && toWarehouseId.Value != Guid.Empty)
            {
                Expression<Func<TransferOrder, bool>> toWarehouseFilter =
                    x => x.ToWarehouse != null && x.ToWarehouse.Id == toWarehouseId.Value;
                filter = filter.And(toWarehouseFilter);
            }

            var transferOrders = await _unitOfWork.TransferOrderRepository
                .GetByWhere(filter)
                .Select(to => new TransferOrderList
                {
                    Id = to.Id,
                    InvoiceCode = to.InvoiceCode,
                    OrderDate = to.OrderDate,
                    SubTotal = to.SubTotal,
                    Status = to.Status,
                    FromWarehouseName = to.FromWarehouse != null ? to.FromWarehouse.Name : null,
                    ToWarehouseName = to.ToWarehouse != null ? to.ToWarehouse.Name : null
                })
                .OrderByDescending(x => x.OrderDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (transferOrders == null || !transferOrders.Any())
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, transferOrders);
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
                            Id = product.Id,
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

        public async Task<IBusinessResult> DeleteTransferOrder(Guid id)
        {
            var transferOrder = await _unitOfWork.TransferOrderRepository
                .GetByWhere(x => x.Id == id)
                .Include(x => x.TransferOrderDetails)
                .FirstOrDefaultAsync();

            if (transferOrder == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy phiếu chuyển kho");

            if (transferOrder.Status == OrderStatus.Received.ToString())
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không thể xóa phiếu chuyển kho đã nhận");


                foreach (var detail in transferOrder.TransferOrderDetails)
                {
                    var restoreResult = await _unitOfWork.BranchStockRepository.UpdateBranchStockAsync(
                        warehouseId: (Guid)transferOrder.FromWarehouseId,
                        productId: detail.ProductId,
                        deltaAvailableQty: detail.Quantity
                    );

                    if (restoreResult.StatusCode != Const.HTTP_STATUS_OK)
                    {
                        return new BusinessResult(Const.ERROR_EXCEPTION,
                            $"Lỗi khi khôi phục tồn kho tại kho nguồn cho sản phẩm {detail.ProductId}");
                    }
                }

                // Delete the transfer order - related details will be deleted automatically due to cascade delete
                _unitOfWork.TransferOrderRepository.Delete(transferOrder);
                await _unitOfWork.SaveChangesAsync();

                return new BusinessResult(Const.HTTP_STATUS_OK, "Xóa phiếu chuyển kho thành công");
            
        }
    }
}


