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
    public class BranchDebtService : IBranchDebtService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BranchDebtService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IBusinessResult> AddBranchTransactionAsync(Guid branchDebtId, List<BranchDebtHistoryRequest> transactions)
        {
            if (transactions == null || !transactions.Any())
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "No transaction data provided");


            var branchDebt = await _unitOfWork.BranchDebtRepository
                .GetByWhere(x => x.Id == branchDebtId)
                .FirstOrDefaultAsync();

            decimal currentBalance = branchDebt.RemainingDebt ?? 0;
            var historyEntities = new List<BranchDebtHistory>();

            foreach (var t in transactions)
            {
                decimal balanceBefore = currentBalance;
                decimal balanceAfter = balanceBefore;

                switch (t.TransactionType)
                {
                    case "TransferIn":
                        branchDebt.TotalDebt += t.Amount;
                        balanceAfter = branchDebt.TotalDebt - branchDebt.PaidAmount - branchDebt.ReturnAmount;
                        break;

                    case "TransferOut":
                        branchDebt.PaidAmount += t.Amount;
                        balanceAfter = branchDebt.TotalDebt - branchDebt.PaidAmount - branchDebt.ReturnAmount;
                        break;

                    case "Return":
                        branchDebt.ReturnAmount += t.Amount;
                        balanceAfter = branchDebt.TotalDebt - branchDebt.PaidAmount - branchDebt.ReturnAmount;
                        break;

                    case "Custom":
                        balanceAfter = t.Amount;
                        branchDebt.RemainingDebt = t.Amount;

                        branchDebt.TotalDebt = Math.Max(t.Amount, 0);
                        branchDebt.PaidAmount = branchDebt.TotalDebt - t.Amount;
                        branchDebt.ReturnAmount = 0;
                        break;

                    default:
                        throw new Exception($"Invalid TransactionType: {t.TransactionType}");
                }

                branchDebt.RemainingDebt = Math.Max(0, balanceAfter);
                currentBalance = branchDebt.RemainingDebt ?? 0;

                historyEntities.Add(new BranchDebtHistory
                {
                    Id = Guid.NewGuid(),
                    BranchDebtId = branchDebt.Id,
                    TransactionType = t.TransactionType,
                    TransactionDate = t.TransactionDate,
                    Amount = t.Amount,
                    Note = t.Note,
                    CreatedAt = DateTime.Now
                });
            }

            branchDebt.LastTransactionDate = DateTime.Now;
            branchDebt.LastUpdated = DateTime.Now;
            _unitOfWork.BranchDebtRepository.Update(branchDebt);

            await _unitOfWork.BranchDebtHistoryRepository.AddRangeAsync(historyEntities);
            await _unitOfWork.SaveChangesAsync();


            return new BusinessResult(Const.HTTP_STATUS_OK, "Branch transactions created successfully", new
            {
                branchDebt.TotalDebt,
                branchDebt.PaidAmount,
                branchDebt.ReturnAmount,
                branchDebt.RemainingDebt
            });
        }
        public async Task<IBusinessResult> GetBranchDebtAsync(Guid id, string transactionType)
        {
            try
            {
                var branchDebt = await _unitOfWork.BranchDebtRepository
                    .GetByWhere(x => x.Id == id)
                    .Include(x => x.Branch)
                    .Include(x => x.BranchDebtHistories)
                    .OrderByDescending(x => x.TotalDebt)
                    .FirstOrDefaultAsync();

                if (branchDebt == null)
                    return new BusinessResult(Const.HTTP_STATUS_OK, "No branch debt record found");
                var historiesQuery = branchDebt.BranchDebtHistories.AsQueryable();
                if (!string.IsNullOrEmpty(transactionType) && transactionType != "all")
                {
                    historiesQuery = historiesQuery.Where(h => h.TransactionType == transactionType);
                }
                var response = new BranchDebtResponse
                {
                    Id = branchDebt.Id,
                    BranchName = branchDebt.Branch?.Name ?? "Unknown",
                    TotalDebt = branchDebt.TotalDebt,
                    PaidAmount = branchDebt.PaidAmount,
                    ReturnAmount = branchDebt.ReturnAmount,
                    RemainingDebt = branchDebt.RemainingDebt,
                    LastTransactionDate = branchDebt.LastTransactionDate,
                    LastUpdated = branchDebt.LastUpdated,
                    BranchDebtHistories = historiesQuery
                        .OrderByDescending(h => h.TransactionDate)
                        .Select(h => new BranchDebtHistoryResponse
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

                return new BusinessResult(Const.HTTP_STATUS_OK, "Branch debt retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Error retrieving branch debt", ex.Message);
            }
        }

        public async Task<IBusinessResult> GetBranchDebtList(
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

                var branchDebts = await _unitOfWork.BranchDebtRepository.GetPagedAsync(
                    pageIndex,
                    pageSize,
                    x =>
                        // Search
                        (string.IsNullOrEmpty(searchTerm)
                            || (x.Branch != null && x.Branch.Name.ToLower().Contains(searchTerm))
                            || (x.Branch != null && x.Branch.PhoneNumber != null && x.Branch.PhoneNumber.ToLower().Contains(searchTerm)))

                        // Total filter
                        && (!totalFrom.HasValue || x.TotalDebt >= totalFrom.Value)
                        && (!totalTo.HasValue || x.TotalDebt <= totalTo.Value)

                        // Debt filter
                        && (!debtFrom.HasValue || x.RemainingDebt >= debtFrom.Value)
                        && (!debtTo.HasValue || x.RemainingDebt <= debtTo.Value)

                        // Date filter
                        && (!startDate.HasValue || x.LastTransactionDate >= startDate.Value)
                        && (!endDate.HasValue || x.LastTransactionDate < endDate.Value),

                    include: q => q.Include(x => x.Branch)
                );

                if (branchDebts == null || !branchDebts.Any())
                    return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

                var responseList = branchDebts.Select(x => new BranchDebtResponse
                {
                    Id = x.Id,
                    BranchName = x.Branch?.Name ?? "Unknown",
                    TotalDebt = x.TotalDebt,
                    PaidAmount = x.PaidAmount,
                    ReturnAmount = x.ReturnAmount,
                    RemainingDebt = x.RemainingDebt,
                    LastTransactionDate = x.LastTransactionDate
                }).ToList();

                return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, responseList);
            }


        public async Task<byte[]> ExportBranchDebtToExcelAsync()
        {
            var branchDebts = await _unitOfWork.BranchDebtRepository
                .GetAll()
                .Include(x => x.Branch)
                .Include(x => x.BranchDebtHistories)
                .OrderByDescending(x => x.TotalDebt)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("BranchDebts");
                var headers = new[] { "STT", "Đại Lý", "Nợ Trong Kỳ", "Có", "Hàng Lỗi/Trả", "Nợ Phải Thu" };

                for (int i = 0; i < headers.Length; i++)
                {
                    var headerCell = worksheet.Cell(1, i + 1);
                    headerCell.Value = headers[i];
                    headerCell.Style.Font.Bold = true;
                    headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#A9D08E");
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

                for (int index = 0; index < branchDebts.Count; index++)
                {
                    var rowNumber = index + 2;
                    var branchDebt = branchDebts[index];

                    var branchName = branchDebt.Branch?.Name ?? "Unknown";
                    var debt = branchDebt.TotalDebt;
                    var paid = branchDebt.PaidAmount;
                    var returnAmount = branchDebt.ReturnAmount;
                    var remaining = branchDebt.RemainingDebt ?? 0m;

                    worksheet.Cell(rowNumber, 1).Value = index + 1;
                    worksheet.Cell(rowNumber, 2).Value = branchName;
                    worksheet.Cell(rowNumber, 3).Value = debt;
                    worksheet.Cell(rowNumber, 4).Value = paid;
                    worksheet.Cell(rowNumber, 5).Value = returnAmount;
                    worksheet.Cell(rowNumber, 6).Value = remaining;

                    HighlightBranchDebtCell(worksheet.Cell(rowNumber, 3), debt);
                    HighlightBranchDebtCell(worksheet.Cell(rowNumber, 4), paid);
                    HighlightBranchDebtCell(worksheet.Cell(rowNumber, 5), returnAmount);
                    HighlightBranchDebtCell(worksheet.Cell(rowNumber, 6), remaining, isRemainingColumn: true);

                    totalDebt += debt;
                    totalPaid += paid;
                    totalReturn += returnAmount;
                    totalRemaining += remaining;
                }

                var totalRow = branchDebts.Count + 2;
                worksheet.Range(totalRow, 1, totalRow, 2).Merge();

                var totalTitleCell = worksheet.Cell(totalRow, 1);
                totalTitleCell.Value = "Tổng cộng";
                totalTitleCell.Style.Font.Bold = true;
                totalTitleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                totalTitleCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                totalTitleCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E2EFDA");

                worksheet.Cell(totalRow, 3).Value = totalDebt;
                worksheet.Cell(totalRow, 4).Value = totalPaid;
                worksheet.Cell(totalRow, 5).Value = totalReturn;
                worksheet.Cell(totalRow, 6).Value = totalRemaining;

                var totalRange = worksheet.Range(totalRow, 3, totalRow, 6);
                totalRange.Style.Font.Bold = true;
                totalRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#E2EFDA");
                totalRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                totalRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                HighlightBranchDebtCell(worksheet.Cell(totalRow, 3), totalDebt);
                HighlightBranchDebtCell(worksheet.Cell(totalRow, 4), totalPaid);
                HighlightBranchDebtCell(worksheet.Cell(totalRow, 5), totalReturn);
                HighlightBranchDebtCell(worksheet.Cell(totalRow, 6), totalRemaining, isRemainingColumn: true);

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

                var branchDebtHistories = branchDebts
                    .SelectMany(b => (b.BranchDebtHistories ?? Enumerable.Empty<BranchDebtHistory>())
                        .Select(h => new
                        {
                            BranchName = b.Branch?.Name ?? "Unknown",
                            History = h
                        }))
                    .OrderBy(h => h.History.TransactionDate)
                    .ThenBy(h => h.History.CreatedAt)
                    .ToList();

                var historySheet = workbook.Worksheets.Add("BranchDebtHistories");
                var historyHeaders = new[] { "Ngày", "Chi Nhánh", "Số Tiền", "Ghi Chú" };

                for (int i = 0; i < historyHeaders.Length; i++)
                {
                    var headerCell = historySheet.Cell(1, i + 1);
                    headerCell.Value = historyHeaders[i];
                    headerCell.Style.Font.Bold = true;
                    headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#A9D08E");
                }

                historySheet.Row(1).Height = 25;
                historySheet.SheetView.FreezeRows(1);

                for (int index = 0; index < branchDebtHistories.Count; index++)
                {
                    var rowNumber = index + 2;
                    var entry = branchDebtHistories[index];
                    var history = entry.History;

                    var dateCell = historySheet.Cell(rowNumber, 1);
                    dateCell.Value = history.TransactionDate;
                    dateCell.Style.DateFormat.Format = "dd/MM/yyyy";

                    historySheet.Cell(rowNumber, 2).Value = entry.BranchName;

                    var amountCell = historySheet.Cell(rowNumber, 3);
                    amountCell.Value = history.Amount;
                    HighlightBranchDebtCell(amountCell, history.Amount);

                    historySheet.Cell(rowNumber, 4).Value = history.Note ?? string.Empty;
                }

                historySheet.Column(1).Width = 15;
                historySheet.Column(2).AdjustToContents();
                historySheet.Column(3).Style.NumberFormat.Format = "\"₫\" #,##0;\"₫\" -#,##0;\"₫\" 0";
                historySheet.Column(3).AdjustToContents();
                historySheet.Column(4).AdjustToContents();

                var historyLastRow = Math.Max(branchDebtHistories.Count + 1, 2);
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

        private static void HighlightBranchDebtCell(IXLCell cell, decimal value, bool isRemainingColumn = false)
        {
            if (value > 0)
            {
                cell.Style.Font.FontColor = isRemainingColumn
                    ? XLColor.FromHtml("#1F4E78")
                    : XLColor.FromHtml("#C00000");
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


        public async Task<IBusinessResult> DeleteBranchDebtHistoryAsync(Guid branchDebtId, Guid historyId)
        {
            try
            {
                var branchDebt = await _unitOfWork.BranchDebtRepository
                    .GetByWhere(x => x.Id == branchDebtId)
                    .Include(x => x.BranchDebtHistories)
                    .FirstOrDefaultAsync();

                if (branchDebt == null)
                    return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy công nợ chi nhánh");

                var history = branchDebt.BranchDebtHistories?
                    .FirstOrDefault(x => x.Id == historyId);

                if (history == null)
                    return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy lịch sử công nợ cần xoá");

                if (branchDebt.BranchDebtHistories == null)
                {
                    return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy lịch sử công nợ");
                }

                branchDebt.BranchDebtHistories.Remove(history);

                decimal totalDebt = 0;
                decimal paidAmount = 0;
                decimal returnAmount = 0;
                decimal currentBalance = 0;
                DateTime? lastTransactionDate = null;

                var remainingHistories = branchDebt.BranchDebtHistories
                    .OrderBy(h => h.TransactionDate)
                    .ThenBy(h => h.CreatedAt)
                    .ToList();

                foreach (var h in remainingHistories)
                {
                    switch (h.TransactionType)
                    {
                        case "Transfer":
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

                branchDebt.TotalDebt = totalDebt;
                branchDebt.PaidAmount = paidAmount;
                branchDebt.ReturnAmount = returnAmount;
                branchDebt.RemainingDebt = Math.Max(0, currentBalance);
                branchDebt.LastTransactionDate = lastTransactionDate;
                branchDebt.LastUpdated = DateTime.Now;

                _unitOfWork.BranchDebtRepository.Update(branchDebt);
                await _unitOfWork.SaveChangesAsync();

                return new BusinessResult(Const.HTTP_STATUS_OK,
                    "Đã xoá lịch sử công nợ và cập nhật lại công nợ chi nhánh",
                    new
                    {
                        branchDebt.Id,
                        branchDebt.TotalDebt,
                        branchDebt.PaidAmount,
                        branchDebt.ReturnAmount,
                        branchDebt.RemainingDebt
                    });
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Error deleting branch debt history", ex.Message);
            }
        }

    }
}

