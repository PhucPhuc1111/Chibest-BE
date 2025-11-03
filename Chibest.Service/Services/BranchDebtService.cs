using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Response;
using Microsoft.EntityFrameworkCore;

namespace Chibest.Service.Services
{
    public class BranchDebtService : IBranchDebtService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BranchDebtService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IBusinessResult> AddBranchTransactionAsync(Guid branchId, List<BranchDebtHistoryRequest> transactions)
        {
            if (transactions == null || !transactions.Any())
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "No transaction data provided");


            var branchDebt = await _unitOfWork.BranchDebtRepository
                .GetByWhere(x => x.BranchId == branchId)
                .FirstOrDefaultAsync();

            if (branchDebt == null)
            {
                branchDebt = new BranchDebt
                {
                    Id = Guid.NewGuid(),
                    BranchId = branchId,
                    TotalDebt = 0,
                    PaidAmount = 0,
                    LastTransactionDate = DateTime.Now,
                    LastUpdated = DateTime.Now
                };

                await _unitOfWork.BranchDebtRepository.AddAsync(branchDebt);
                await _unitOfWork.SaveChangesAsync();
            }

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
                        balanceAfter = balanceBefore + t.Amount;
                        break;

                    case "TransferOut":
                        branchDebt.PaidAmount += t.Amount;
                        balanceAfter = balanceBefore - t.Amount;
                        break;

                    case "Return":
                        branchDebt.TotalDebt = Math.Max(0, branchDebt.TotalDebt - t.Amount);
                        balanceAfter = balanceBefore - t.Amount;
                        break;

                    case "Custom":
                        balanceAfter = t.Amount;
                        branchDebt.RemainingDebt = t.Amount;

                        branchDebt.TotalDebt = Math.Max(t.Amount, 0);
                        branchDebt.PaidAmount = branchDebt.TotalDebt - t.Amount;
                        break;

                    default:
                        throw new Exception($"Invalid TransactionType: {t.TransactionType}");
                }

                branchDebt.RemainingDebt = balanceAfter;
                currentBalance = balanceAfter;

                historyEntities.Add(new BranchDebtHistory
                {
                    Id = Guid.NewGuid(),
                    BranchDebtId = branchDebt.Id,
                    TransactionType = t.TransactionType,
                    TransactionDate = t.TransactionDate,
                    Amount = t.Amount,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    Note = t.Note,
                    CreatedAt = DateTime.Now
                });
            }

            branchDebt.LastTransactionDate = DateTime.Now;
            branchDebt.LastUpdated = DateTime.Now;
            _unitOfWork.BranchDebtRepository.Update(branchDebt);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.BranchDebtHistoryRepository.AddRangeAsync(historyEntities);


            return new BusinessResult(Const.HTTP_STATUS_OK, "Branch transactions created successfully", new
            {
                BranchId = branchId,
                branchDebt.TotalDebt,
                branchDebt.PaidAmount,
                branchDebt.RemainingDebt
            });
        }
        public async Task<IBusinessResult> GetBranchDebtAsync(Guid id)
        {
            try
            {
                var branchDebt = await _unitOfWork.BranchDebtRepository
                    .GetByWhere(x => x.Id == id)
                    .Include(x => x.Branch)
                    .Include(x => x.BranchDebtHistories)
                    .FirstOrDefaultAsync();

                if (branchDebt == null)
                    return new BusinessResult(Const.HTTP_STATUS_OK, "No branch debt record found");

                var response = new BranchDebtResponse
                {
                    Id = branchDebt.Id,
                    BranchName = branchDebt.Branch?.Name ?? "Unknown",
                    TotalDebt = branchDebt.TotalDebt,
                    PaidAmount = branchDebt.PaidAmount,
                    RemainingDebt = branchDebt.RemainingDebt,
                    LastTransactionDate = branchDebt.LastTransactionDate,
                    LastUpdated = branchDebt.LastUpdated,
                    BranchDebtHistories = branchDebt.BranchDebtHistories
                        .OrderByDescending(h => h.TransactionDate)
                        .Select(h => new BranchDebtHistoryResponse
                        {
                            Id = h.Id,
                            TransactionType = h.TransactionType,
                            TransactionDate = h.TransactionDate,
                            Amount = h.Amount,
                            BalanceBefore = h.BalanceBefore,
                            BalanceAfter = h.BalanceAfter,
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
        public async Task<IBusinessResult> GetBranchDebtList(int pageIndex, int pageSize, string search)
        {
            string searchTerm = search?.ToLower() ?? string.Empty;

            var branchDebts = await _unitOfWork.BranchDebtRepository.GetPagedAsync(
                pageIndex,
                pageSize,
                x => string.IsNullOrEmpty(searchTerm)
                    || x.Branch.Name.ToLower().Contains(searchTerm)
                    || x.Branch.PhoneNumber.ToLower().Contains(searchTerm)
                    || x.Branch.PhoneNumber != null && x.Branch.PhoneNumber.ToLower().Contains(searchTerm),
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
                RemainingDebt = x.RemainingDebt,
                LastTransactionDate = x.LastTransactionDate
            }).ToList();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, responseList);
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
                        case "TransferIn":
                            totalDebt += h.Amount;
                            currentBalance += h.Amount;
                            break;

                        case "TransferOut":
                            paidAmount += h.Amount;
                            currentBalance -= h.Amount;
                            break;

                        case "Return":
                            totalDebt = Math.Max(0, totalDebt - h.Amount);
                            currentBalance -= h.Amount;
                            break;

                        case "Custom":
                            currentBalance = h.Amount;
                            totalDebt = Math.Max(h.Amount, 0);
                            paidAmount = totalDebt - h.Amount;
                            break;

                        default:
                            throw new Exception($"Invalid TransactionType: {h.TransactionType}");
                    }

                    if (currentBalance < 0)
                        currentBalance = 0;

                    if (h.TransactionDate > (lastTransactionDate ?? DateTime.MinValue))
                        lastTransactionDate = h.TransactionDate;
                }

                branchDebt.TotalDebt = totalDebt;
                branchDebt.PaidAmount = paidAmount;
                branchDebt.RemainingDebt = currentBalance;
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

