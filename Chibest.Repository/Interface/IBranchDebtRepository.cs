using Chibest.Common.BusinessResult;
using Chibest.Repository.Base;
using Chibest.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Repository.Interface
{
    public interface IBranchDebtRepository : IGenericRepository<BranchDebt>
    {
        Task<IBusinessResult> AddBranchTransactionAsync(
    Guid branchId,
    string transactionType,
    decimal amount,
    string? note = null);
    }
}
