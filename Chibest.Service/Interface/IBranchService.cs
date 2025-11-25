using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.Interface
{
    public interface IBranchService
    {
        Task<IBusinessResult> GetBranchById(Guid id);
        Task<IBusinessResult> CreateBranch(BranchRequest request);
        Task<IBusinessResult> GetBranchList(int pageIndex, int pageSize, string? search = null, bool? isFranchise = null);
        Task<IBusinessResult> UpdateBranch(Guid id, BranchRequest request);
        Task<IBusinessResult> DeleteBranch(Guid id);
    }
}
