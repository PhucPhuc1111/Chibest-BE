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
    public class SupplierDebtService : ISupplierDebtService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SupplierDebtService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IBusinessResult> AddSupplierTransactionAsync(
    Guid supplierId,
    List<SupplierDebtHistoryRequest> transactions)
        {
            if (transactions == null || !transactions.Any())
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "No transaction data provided");

                var supplierDebt = await _unitOfWork.SupplierDebtRepository
                    .GetByWhere(x => x.SupplierId == supplierId)
                    .FirstOrDefaultAsync();

                if (supplierDebt == null)
                {
                    supplierDebt = new SupplierDebt
                    {
                        Id = Guid.NewGuid(),
                        SupplierId = supplierId,
                        TotalDebt = 0,
                        PaidAmount = 0,
                        LastTransactionDate = DateTime.Now,
                        LastUpdated = DateTime.Now
                    };

                    await _unitOfWork.SupplierDebtRepository.AddAsync(supplierDebt);
                    await _unitOfWork.SaveChangesAsync();
                }

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
                            balanceAfter = balanceBefore + t.Amount;
                            break;

                        case "Payment":
                            supplierDebt.PaidAmount += t.Amount;
                            balanceAfter = balanceBefore - t.Amount;
                            break;

                        case "Return":
                            supplierDebt.TotalDebt = Math.Max(0, supplierDebt.TotalDebt - t.Amount);
                            balanceAfter = balanceBefore - t.Amount;
                            break;
                        case "Custom":
                            balanceAfter = t.Amount;
                            supplierDebt.RemainingDebt = t.Amount;

                            supplierDebt.TotalDebt = Math.Max(t.Amount, 0);
                            supplierDebt.PaidAmount = supplierDebt.TotalDebt - t.Amount;
                            break;

                        default:
                            throw new Exception($"Invalid TransactionType: {t.TransactionType}");
                    }

                    supplierDebt.RemainingDebt = balanceAfter;
                    currentBalance = balanceAfter;

                    historyEntities.Add(new SupplierDebtHistory
                    {
                        Id = Guid.NewGuid(),
                        SupplierDebtId = supplierDebt.Id,
                        TransactionType = t.TransactionType,
                        TransactionDate = t.TransactionDate,
                        Amount = t.Amount,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        Note = t.Note,
                        CreatedAt = DateTime.Now
                    });
                }

                supplierDebt.LastTransactionDate = DateTime.Now;
                supplierDebt.LastUpdated = DateTime.Now;
                _unitOfWork.SupplierDebtRepository.Update(supplierDebt);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.SupplierDebtHistoryRepository.AddRangeAsync(historyEntities);


                return new BusinessResult(Const.HTTP_STATUS_OK, "Supplier transactions created successfully", new
                {
                    SupplierId = supplierId,
                    supplierDebt.TotalDebt,
                    supplierDebt.PaidAmount,
                    supplierDebt.RemainingDebt,
                });
            
        }


        public async Task<IBusinessResult> GetSupplierDebtAsync(Guid id)
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

                var response = new SupplierDebtResponse
                {
                    Id = supplierDebt.Id,
                    SupplierName = supplierDebt.Supplier?.Name ?? "Unknown",
                    TotalDebt = supplierDebt.TotalDebt,
                    PaidAmount = supplierDebt.PaidAmount,
                    RemainingDebt = supplierDebt.RemainingDebt,
                    LastTransactionDate = supplierDebt.LastTransactionDate,
                    LastUpdated = supplierDebt.LastUpdated,
                    SupplierDebtHistories = supplierDebt.SupplierDebtHistories
                        .OrderByDescending(h => h.TransactionDate)
                        .Select(h => new SupplierDebtHistoryResponse
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

                return new BusinessResult(Const.HTTP_STATUS_OK, "Supplier debt retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Error retrieving supplier debt", ex.Message);
            }
        }

        public async Task<IBusinessResult> GetSupplierDebtList(int pageIndex, int pageSize, string search)
        {
            string searchTerm = search?.ToLower() ?? string.Empty;

            var supplierDebts = await _unitOfWork.SupplierDebtRepository.GetPagedAsync(
                pageIndex,
                pageSize,
                x => string.IsNullOrEmpty(searchTerm)
                    || (x.Supplier != null && !string.IsNullOrEmpty(x.Supplier.Name) && x.Supplier.Name.ToLower().Contains(searchTerm))
                    || (x.Supplier != null && !string.IsNullOrEmpty(x.Supplier.PhoneNumber) && x.Supplier.PhoneNumber.ToLower().Contains(searchTerm))
                    || (x.Supplier != null && !string.IsNullOrEmpty(x.Supplier.Email) && x.Supplier.Email.ToLower().Contains(searchTerm)),
                include: q => q.Include(x => x.Supplier).OrderByDescending(x=> x.TotalDebt)
            );

            if (supplierDebts == null || !supplierDebts.Any())
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }

            var responseList = supplierDebts.Select(x => new SupplierDebtResponse
            {
                Id = x.Id,
                SupplierName = x.Supplier?.Name ?? "Unknown",
                SupplierPhone = x.Supplier?.PhoneNumber ?? string.Empty,
                TotalDebt = x.TotalDebt,
                PaidAmount = x.PaidAmount,
                RemainingDebt = x.RemainingDebt,
                LastTransactionDate = x.LastTransactionDate,
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
                            currentBalance += h.Amount;
                            break;

                        case "Payment":
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

                supplierDebt.TotalDebt = totalDebt;
                supplierDebt.PaidAmount = paidAmount;
                supplierDebt.RemainingDebt = currentBalance;
                supplierDebt.LastTransactionDate = lastTransactionDate;
                supplierDebt.LastUpdated = DateTime.Now;

                _unitOfWork.SupplierDebtRepository.Update(supplierDebt);
                await _unitOfWork.SaveChangesAsync();

                return new BusinessResult(Const.HTTP_STATUS_OK,
                    "Đã xoá lịch sử công nợ và cập nhật lại công nợ nhà cung cấp",
                    new
                    {
                        supplierDebt.Id,
                        supplierDebt.TotalDebt,
                        supplierDebt.PaidAmount,
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
