using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Chibest.Repository.Repositories
{
    public class SupplierDebtRepository : GenericRepository<SupplierDebt>, ISupplierDebtRepository
    {
        public SupplierDebtRepository(ChiBestDbContext context) : base(context){}

        public async Task<IBusinessResult> AddSupplierTransactionAsync(
            Guid supplierId,
            string transactionType,
            decimal amount,
            string? note = null)
        {

            try
            {
                var supplierDebt = await _context.SupplierDebts
                    .FirstOrDefaultAsync(x => x.SupplierId == supplierId);

                if (supplierDebt == null)
                {
                    supplierDebt = new SupplierDebt
                    {
                        Id = Guid.NewGuid(),
                        SupplierId = supplierId,
                        TotalDebt = 0,
                        PaidAmount = 0,
                        RemainingDebt = 0,
                        LastTransactionDate = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    };
                    await _context.SupplierDebts.AddAsync(supplierDebt);
                    await _context.SaveChangesAsync();
                }

                decimal balanceBefore = supplierDebt.RemainingDebt ?? 0;
                decimal balanceAfter = balanceBefore;

                switch (transactionType)
                {
                    case "Purchase":
                        supplierDebt.TotalDebt += amount;
                        balanceAfter = balanceBefore + amount;
                        break;

                    case "Payment":
                        supplierDebt.PaidAmount += amount;
                        balanceAfter = balanceBefore - amount;
                        break;

                    case "Return":
                        balanceAfter = balanceBefore - amount;
                        break;

                    case "Custom":
                        balanceAfter = amount;
                        supplierDebt.RemainingDebt = amount;
                        supplierDebt.TotalDebt = Math.Max(amount, 0);
                        supplierDebt.PaidAmount = supplierDebt.TotalDebt - amount;
                        break;

                    default:
                        throw new Exception($"Invalid TransactionType: {transactionType}");
                }

                supplierDebt.RemainingDebt = balanceAfter;
                supplierDebt.LastTransactionDate = DateTime.UtcNow;
                supplierDebt.LastUpdated = DateTime.UtcNow;

                _context.SupplierDebts.Update(supplierDebt);
                await _context.SaveChangesAsync();

                var history = new SupplierDebtHistory
                {
                    Id = Guid.NewGuid(),
                    SupplierDebtId = supplierDebt.Id,
                    TransactionType = transactionType,
                    TransactionDate = DateTime.UtcNow,
                    Amount = amount,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = balanceAfter,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.SupplierDebtHistories.AddAsync(history);
                await _context.SaveChangesAsync();

                return new BusinessResult(Const.HTTP_STATUS_OK, "Supplier transaction recorded successfully");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Error adding supplier transaction", ex.Message);
            }
        }
    }
}
