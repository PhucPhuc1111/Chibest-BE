using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Chibest.Repository.Repositories
{
    public class BranchDebtRepository : GenericRepository<BranchDebt>, IBranchDebtRepository
    {
        public BranchDebtRepository(ChiBestDbContext context) : base(context){ }
        public async Task<IBusinessResult> AddBranchTransactionAsync(
    Guid branchId,
    string transactionType,
    decimal amount,
    string? note = null)
        {
            try
            {
                var branchDebt = await _context.BranchDebts
                    .FirstOrDefaultAsync(x => x.BranchId == branchId);

                if (branchDebt == null)
                {
                    branchDebt = new BranchDebt
                    {
                        Id = Guid.NewGuid(),
                        BranchId = branchId,
                        TotalDebt = 0,
                        PaidAmount = 0,
                        RemainingDebt = 0,
                        LastTransactionDate = DateTime.Now,
                        LastUpdated = DateTime.Now
                    };

                    await _context.BranchDebts.AddAsync(branchDebt);
                    await _context.SaveChangesAsync();
                }
                decimal balanceBefore = branchDebt.RemainingDebt ?? 0;
                decimal balanceAfter = balanceBefore;

                switch (transactionType)
                {
                    case "TransferIn":
                        branchDebt.TotalDebt += amount;
                        balanceAfter = branchDebt.TotalDebt - branchDebt.PaidAmount;
                        break;

                    case "TransferOut":
                        branchDebt.PaidAmount += amount;
                        balanceAfter = branchDebt.TotalDebt - branchDebt.PaidAmount;
                        break;

                    case "Custom":
                        balanceAfter = amount;
                        branchDebt.RemainingDebt = amount;
                        branchDebt.TotalDebt = Math.Max(amount, 0);
                        branchDebt.PaidAmount = branchDebt.TotalDebt - amount;
                        break;

                    default:
                        throw new Exception($"Invalid TransactionType: {transactionType}");
                }
                branchDebt.RemainingDebt = Math.Max(0, balanceAfter);
                branchDebt.LastTransactionDate = DateTime.Now;
                branchDebt.LastUpdated = DateTime.Now;

                _context.BranchDebts.Update(branchDebt);
                await _context.SaveChangesAsync();

                var history = new BranchDebtHistory
                {
                    Id = Guid.NewGuid(),
                    BranchDebtId = branchDebt.Id,
                    TransactionType = transactionType,
                    TransactionDate = DateTime.Now,
                    Amount = amount,
                    Note = note,
                    CreatedAt = DateTime.Now
                };
                await _context.BranchDebtHistories.AddAsync(history);
                await _context.SaveChangesAsync();
                return new BusinessResult(Const.HTTP_STATUS_OK, "Branch transaction recorded successfully", new
                {
                    BranchId = branchId,
                    branchDebt.TotalDebt,
                    branchDebt.PaidAmount,
                    branchDebt.RemainingDebt
                });
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Error adding branch transaction", ex.Message);
            }
        }
    }
}
