using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Response;
using Chibest.Service.Utilities;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.IO;
using System.Linq;

namespace Chibest.Service.Services
{
    public class SupplierDebtService : ISupplierDebtService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SupplierDebtService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IBusinessResult> AddSupplierTransactionAsync(
    Guid supplierDebtId,
    List<SupplierDebtHistoryRequest> transactions)
        {
            if (transactions == null || !transactions.Any())
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "No transaction data provided");

                var supplierDebt = await _unitOfWork.SupplierDebtRepository
                    .GetByWhere(x => x.Id == supplierDebtId)
                    .FirstOrDefaultAsync();


                decimal currentBalance = supplierDebt.RemainingDebt ?? 0;
                var historyEntities = new List<SupplierDebtHistory>();

                foreach (var t in transactions)
                {
                    decimal balanceBefore = currentBalance;
                    decimal balanceAfter = balanceBefore;

                    switch (t.TransactionType)
                    {
                        case "Purchase":
                            supplierDebt.TotalDebt += t.Amount;
                            balanceAfter = supplierDebt.TotalDebt - supplierDebt.PaidAmount - supplierDebt.ReturnAmount;
                            break;

                        case "Payment":
                            supplierDebt.PaidAmount += t.Amount;
                            balanceAfter = supplierDebt.TotalDebt - supplierDebt.PaidAmount - supplierDebt.ReturnAmount;
                            break;

                        case "Return":
                            supplierDebt.ReturnAmount += t.Amount;
                            balanceAfter = supplierDebt.TotalDebt - supplierDebt.PaidAmount - supplierDebt.ReturnAmount;
                            break;
                        case "Custom":
                            balanceAfter = t.Amount;
                            supplierDebt.RemainingDebt = t.Amount;

                            supplierDebt.TotalDebt = Math.Max(t.Amount, 0);
                            supplierDebt.PaidAmount = supplierDebt.TotalDebt - t.Amount;
                            supplierDebt.ReturnAmount = 0;
                            break;

                        default:
                            throw new Exception($"Invalid TransactionType: {t.TransactionType}");
                    }

                    supplierDebt.RemainingDebt = Math.Max(0, balanceAfter);
                    currentBalance = supplierDebt.RemainingDebt ?? 0;

                    historyEntities.Add(new SupplierDebtHistory
                    {
                        Id = Guid.NewGuid(),
                        SupplierDebtId = supplierDebt.Id,
                        TransactionType = t.TransactionType,
                        TransactionDate = t.TransactionDate,
                        Amount = t.Amount,
                        Note = t.Note,
                        CreatedAt = DateTime.Now
                    });
                }

                _unitOfWork.SupplierDebtRepository.Update(supplierDebt);

                await _unitOfWork.SupplierDebtHistoryRepository.AddRangeAsync(historyEntities);

             await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Supplier transactions created successfully", new
                {
                    supplierDebt.TotalDebt,
                    supplierDebt.PaidAmount,
                    supplierDebt.ReturnAmount,
                    supplierDebt.RemainingDebt,
                });
            
        }


        public async Task<IBusinessResult> GetSupplierDebtAsync(Guid id, string transactionType)
        {
            try
            {
                var supplierDebt = await _unitOfWork.SupplierDebtRepository
                    .GetByWhere(x => x.Id == id)
                    .Include(x => x.Supplier)
                    .Include(x => x.SupplierDebtHistories)
                    .OrderByDescending(x => x.TotalDebt)
                    .FirstOrDefaultAsync();

                if (supplierDebt == null)
                {
                    return new BusinessResult(Const.HTTP_STATUS_OK, "No debt record found");
                }

                // Filter histories by transaction type if provided
                var historiesQuery = supplierDebt.SupplierDebtHistories.AsQueryable();

                if (!string.IsNullOrEmpty(transactionType) && transactionType != "all")
                {
                    historiesQuery = historiesQuery.Where(h => h.TransactionType == transactionType);
                }

                var response = new SupplierDebtResponse
                {
                    Id = supplierDebt.Id,
                    SupplierName = supplierDebt.Supplier?.Name ?? "Unknown",
                    TotalDebt = supplierDebt.TotalDebt,
                    PaidAmount = supplierDebt.PaidAmount,
                    ReturnAmount = supplierDebt.ReturnAmount,
                    RemainingDebt = supplierDebt.RemainingDebt,
                    SupplierDebtHistories = historiesQuery
                        .OrderByDescending(h => h.TransactionDate)
                        .Select(h => new SupplierDebtHistoryResponse
                        {
                            Id = h.Id,
                            TransactionType = h.TransactionType,
                            TransactionDate = h.TransactionDate,
                            Amount = h.Amount,
                            Note = h.Note,
                            CreatedAt = h.CreatedAt
                        })
                        .ToList()
                };

                return new BusinessResult(Const.HTTP_STATUS_OK, "Supplier debt retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Error retrieving supplier debt", ex.Message);
            }
        }


        public async Task<byte[]> ExportSupplierDebtToExcelAsync()
        {
            var supplierDebts = await _unitOfWork.SupplierDebtRepository
                .GetAll()
                .Include(x => x.Supplier)
                .Include(x => x.SupplierDebtHistories)
                .OrderByDescending(x => x.TotalDebt)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SupplierDebts");
                var headers = new[] { "STT", "Tên NCC", "Nợ Trong Kỳ", "Có", "Hàng Lỗi/Trả", "Nợ Phải Trả" };

                for (int i = 0; i < headers.Length; i++)
                {
                    var headerCell = worksheet.Cell(1, i + 1);
                    headerCell.Value = headers[i];
                    headerCell.Style.Font.Bold = true;
                    headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F4B084");
                }

                worksheet.Row(1).Height = 25;
                worksheet.SheetView.FreezeRows(1);

                worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Columns(3, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Columns(3, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                decimal totalDebt = 0m;
                decimal totalPaid = 0m;
                decimal totalReturn = 0m;
                decimal totalRemaining = 0m;

                for (int index = 0; index < supplierDebts.Count; index++)
                {
                    var rowNumber = index + 2;
                    var supplierDebt = supplierDebts[index];

                    var supplierName = supplierDebt.Supplier?.Name ?? "Unknown";
                    var debt = supplierDebt.TotalDebt;
                    var paid = supplierDebt.PaidAmount;
                    var returnAmount = supplierDebt.ReturnAmount;
                    var remaining = supplierDebt.RemainingDebt ?? 0m;

                    worksheet.Cell(rowNumber, 1).Value = index + 1;
                    worksheet.Cell(rowNumber, 2).Value = supplierName;
                    worksheet.Cell(rowNumber, 3).Value = debt;
                    worksheet.Cell(rowNumber, 4).Value = paid;
                    worksheet.Cell(rowNumber, 5).Value = returnAmount;
                    worksheet.Cell(rowNumber, 6).Value = remaining;

                    HighlightDebtCell(worksheet.Cell(rowNumber, 3), debt);
                    HighlightDebtCell(worksheet.Cell(rowNumber, 4), paid);
                    HighlightDebtCell(worksheet.Cell(rowNumber, 5), returnAmount);
                    HighlightDebtCell(worksheet.Cell(rowNumber, 6), remaining);

                    totalDebt += debt;
                    totalPaid += paid;
                    totalReturn += returnAmount;
                    totalRemaining += remaining;
                }

                var totalRow = supplierDebts.Count + 2;
                worksheet.Range(totalRow, 1, totalRow, 2).Merge();

                var totalTitleCell = worksheet.Cell(totalRow, 1);
                totalTitleCell.Value = "TỔNG CỘNG";
                totalTitleCell.Style.Font.Bold = true;
                totalTitleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                totalTitleCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                totalTitleCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");

                worksheet.Cell(totalRow, 3).Value = totalDebt;
                worksheet.Cell(totalRow, 4).Value = totalPaid;
                worksheet.Cell(totalRow, 5).Value = totalReturn;
                worksheet.Cell(totalRow, 6).Value = totalRemaining;

                var totalRange = worksheet.Range(totalRow, 3, totalRow, 6);
                totalRange.Style.Font.Bold = true;
                totalRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                totalRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                totalRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                HighlightDebtCell(worksheet.Cell(totalRow, 3), totalDebt);
                HighlightDebtCell(worksheet.Cell(totalRow, 4), totalPaid);
                HighlightDebtCell(worksheet.Cell(totalRow, 5), totalReturn);
                HighlightDebtCell(worksheet.Cell(totalRow, 6), totalRemaining);

                var lastRow = Math.Max(totalRow, 2);
                var tableRange = worksheet.Range(1, 1, lastRow, headers.Length);
                tableRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                worksheet.Columns(3, 6).Style.NumberFormat.Format = "\"₫\" #,##0;\"₫\" -#,##0;\"₫\" 0";
                worksheet.Column(1).AdjustToContents();
                worksheet.Column(2).AdjustToContents();
                worksheet.Columns(3, 6).AdjustToContents();

                var supplierDebtHistories = supplierDebts
                    .SelectMany(d => (d.SupplierDebtHistories ?? Enumerable.Empty<SupplierDebtHistory>())
                        .Select(h => new
                        {
                            SupplierName = d.Supplier?.Name ?? "Unknown",
                            History = h
                        }))
                    .OrderBy(h => h.History.TransactionDate)
                    .ThenBy(h => h.History.CreatedAt)
                    .ToList();

                var historySheet = workbook.Worksheets.Add("SupplierDebtHistories");
                var historyHeaders = new[] { "Ngày", "NCC", "Số Tiền", "Ghi Chú" };

                for (int i = 0; i < historyHeaders.Length; i++)
                {
                    var headerCell = historySheet.Cell(1, i + 1);
                    headerCell.Value = historyHeaders[i];
                    headerCell.Style.Font.Bold = true;
                    headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F4B084");
                }

                historySheet.Row(1).Height = 25;
                historySheet.SheetView.FreezeRows(1);

                for (int index = 0; index < supplierDebtHistories.Count; index++)
                {
                    var rowNumber = index + 2;
                    var entry = supplierDebtHistories[index];
                    var history = entry.History;

                    var dateCell = historySheet.Cell(rowNumber, 1);
                    dateCell.Value = history.TransactionDate;
                    dateCell.Style.DateFormat.Format = "dd/MM/yyyy";

                    historySheet.Cell(rowNumber, 2).Value = entry.SupplierName;

                    var amountCell = historySheet.Cell(rowNumber, 3);
                    amountCell.Value = history.Amount;
                    HighlightDebtCell(amountCell, history.Amount);

                    historySheet.Cell(rowNumber, 4).Value = history.Note ?? string.Empty;
                }

                historySheet.Column(1).Width = 15;
                historySheet.Column(2).AdjustToContents();
                historySheet.Column(3).Style.NumberFormat.Format = "\"₫\" #,##0;\"₫\" -#,##0;\"₫\" 0";
                historySheet.Column(3).AdjustToContents();
                historySheet.Column(4).AdjustToContents();

                var historyLastRow = Math.Max(supplierDebtHistories.Count + 1, 2);
                var historyRange = historySheet.Range(1, 1, historyLastRow, historyHeaders.Length);
                historyRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                historyRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                historyRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                historyRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        private static void HighlightDebtCell(IXLCell cell, decimal value)
        {
            if (value > 0)
            {
                cell.Style.Font.FontColor = XLColor.FromHtml("#C00000");
            }
            else if (value < 0)
            {
                cell.Style.Font.FontColor = XLColor.FromHtml("#548235");
            }
            else
            {
                cell.Style.Font.FontColor = XLColor.Black;
            }
        }


        public async Task<IBusinessResult> GetSupplierDebtList(
        int pageIndex,
        int pageSize,
        string? search = null,
        decimal? totalFrom = null,
        decimal? totalTo = null,
        string? datePreset = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        decimal? debtFrom = null,
        decimal? debtTo = null)
        {
            string searchTerm = search?.ToLower() ?? string.Empty;

            // Dùng helper để suy ra khoảng ngày
            var (startDate, endDate) = DateRangeHelper.GetDateRange(datePreset, fromDate, toDate);

            var supplierDebts = await _unitOfWork.SupplierDebtRepository.GetPagedAsync(
                pageIndex,
                pageSize,
                x =>
                    // Search theo tên/điện thoại/email NCC
                    (string.IsNullOrEmpty(searchTerm)
                        || (x.Supplier != null && !string.IsNullOrEmpty(x.Supplier.Name) && x.Supplier.Name.ToLower().Contains(searchTerm))
                        || (x.Supplier != null && !string.IsNullOrEmpty(x.Supplier.PhoneNumber) && x.Supplier.PhoneNumber.ToLower().Contains(searchTerm))
                        || (x.Supplier != null && !string.IsNullOrEmpty(x.Supplier.Email) && x.Supplier.Email.ToLower().Contains(searchTerm)))

                    // Total filter
                    && (!totalFrom.HasValue || x.TotalDebt >= totalFrom.Value)
                    && (!totalTo.HasValue || x.TotalDebt <= totalTo.Value)

                    // Debt filter
                    && (!debtFrom.HasValue || x.RemainingDebt >= debtFrom.Value)
                    && (!debtTo.HasValue || x.RemainingDebt <= debtTo.Value),

                include: q => q.Include(x => x.Supplier).OrderByDescending(x => x.TotalDebt)
            );

            if (supplierDebts == null || !supplierDebts.Any())
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            var responseList = supplierDebts.Select(x => new SupplierDebtResponse
            {
                Id = x.Id,
                SupplierName = x.Supplier?.Name ?? "Unknown",
                SupplierPhone = x.Supplier?.PhoneNumber ?? string.Empty,
                TotalDebt = x.TotalDebt,
                PaidAmount = x.PaidAmount,
                ReturnAmount = x.ReturnAmount,
                RemainingDebt = x.RemainingDebt,
            }).ToList();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, responseList);
        }


    #region  Delete Supplier Debt History
    public async Task<IBusinessResult> DeleteSupplierDebtHistoryAsync(Guid supplierDebtId, Guid historyId)
        {
            try
            {
                var supplierDebt = await _unitOfWork.SupplierDebtRepository
                    .GetByWhere(x => x.Id == supplierDebtId)
                    .Include(x => x.SupplierDebtHistories)
                    .FirstOrDefaultAsync();

                if (supplierDebt == null)
                    return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy công nợ nhà cung cấp");

                var history = supplierDebt.SupplierDebtHistories?
                    .FirstOrDefault(x => x.Id == historyId);

                if (history == null)
                    return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy lịch sử công nợ cần xoá");

                if (supplierDebt.SupplierDebtHistories == null)
                {
                    return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy lịch sử công nợ");
                }

                supplierDebt.SupplierDebtHistories.Remove(history);

                decimal totalDebt = 0;
                decimal paidAmount = 0;
                decimal returnAmount = 0;
                decimal currentBalance = 0;
                DateTime? lastTransactionDate = null;

                var remainingHistories = supplierDebt.SupplierDebtHistories
                    .OrderBy(h => h.TransactionDate)
                    .ThenBy(h => h.CreatedAt)
                    .ToList();

                foreach (var h in remainingHistories)
                {
                    switch (h.TransactionType)
                    {
                        case "Purchase":
                            totalDebt += h.Amount;
                            break;

                        case "Payment":
                            paidAmount += h.Amount;
                            break;

                        case "Return":
                            returnAmount += h.Amount;
                            break;

                        case "Custom":
                            currentBalance = h.Amount;
                            totalDebt = Math.Max(h.Amount, 0);
                            paidAmount = totalDebt - h.Amount;
                            returnAmount = 0;
                            break;

                        default:
                            throw new Exception($"Invalid TransactionType: {h.TransactionType}");
                    }

                    // Calculate RemainingDebt = TotalDebt - PaidAmount - ReturnAmount
                    if (h.TransactionType != "Custom")
                    {
                        currentBalance = totalDebt - paidAmount - returnAmount;
                    }

                    if (currentBalance < 0)
                        currentBalance = 0;

                    if (h.TransactionDate > (lastTransactionDate ?? DateTime.MinValue))
                        lastTransactionDate = h.TransactionDate;
                }

                supplierDebt.TotalDebt = totalDebt;
                supplierDebt.PaidAmount = paidAmount;
                supplierDebt.ReturnAmount = returnAmount;
                supplierDebt.RemainingDebt = Math.Max(0, currentBalance);

                _unitOfWork.SupplierDebtRepository.Update(supplierDebt);
                await _unitOfWork.SaveChangesAsync();

                return new BusinessResult(Const.HTTP_STATUS_OK,
                    "Đã xoá lịch sử công nợ và cập nhật lại công nợ nhà cung cấp",
                    new
                    {
                        supplierDebt.Id,
                        supplierDebt.TotalDebt,
                        supplierDebt.PaidAmount,
                        supplierDebt.ReturnAmount,
                        supplierDebt.RemainingDebt
                    });
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Error deleting supplier debt history", ex.Message);
            }
        }
        #endregion
    }
}
