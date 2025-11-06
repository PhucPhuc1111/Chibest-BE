using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.Interface
{
    public interface IBranchDebtService
    {
        Task<IBusinessResult> AddBranchTransactionAsync(Guid branchDebtId, List<BranchDebtHistoryRequest> transactions);
        Task<IBusinessResult> GetBranchDebtAsync(Guid id, string transactionType = null);
        Task<IBusinessResult> GetBranchDebtList(int pageIndex,
    int pageSize,
    string? search = null,
    decimal? totalFrom = null,
    decimal? totalTo = null,
    string? datePreset = null,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    decimal? debtFrom = null,
    decimal? debtTo = null);
        Task<IBusinessResult> DeleteBranchDebtHistoryAsync(Guid branchDebtId, Guid historyId);
    }
}
