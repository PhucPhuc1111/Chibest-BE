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
        Task<IBusinessResult> AddBranchTransactionAsync(Guid branchId, List<BranchDebtHistoryRequest> transactions);
        Task<IBusinessResult> GetBranchDebtAsync(Guid id);
        Task<IBusinessResult> GetBranchDebtList(int pageIndex, int pageSize, string search);
    }
}
