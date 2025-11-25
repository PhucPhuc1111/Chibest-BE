using Chibest.Common.BusinessResult;
using Chibest.Common.Enums;
using Chibest.Service.ModelDTOs.Stock.FranchiseOrder;
using System;

namespace Chibest.Service.Interface;

public interface IFranchiseService
{
    Task<IBusinessResult> CreateFranchiseOrderAsync(FranchiseOrderCreate request);

    Task<IBusinessResult> UpdateFranchiseOrderAsync(Guid id, FranchiseOrderUpdate request);

    Task<IBusinessResult> DeleteFranchiseOrderAsync(Guid id);

    Task<IBusinessResult> GetFranchiseOrderByIdAsync(Guid id);

    Task<IBusinessResult> GetFranchiseOrderListAsync(int pageIndex, int pageSize, string? search = null, DateTime? fromDate = null, DateTime? toDate = null, string? status = null, Guid? branchId = null);
}

