using Chibest.Common.BusinessResult;
using Chibest.Common.Enums;
using Chibest.Service.ModelDTOs.Stock.FranchiseOrder;

namespace Chibest.Service.Interface;

public interface IFranchiseInvoiceService
{
    Task<IBusinessResult> CreateInvoiceAsync(FranchiseInvoiceCreate request);

    Task<IBusinessResult> UpdateInvoiceStatusAsync(Guid invoiceId, OrderStatus status);

    Task<IBusinessResult> GetInvoiceListAsync(int pageIndex, int pageSize, string? search = null, DateTime? fromDate = null, DateTime? toDate = null, string? status = null, Guid? branchId = null);

    Task<IBusinessResult> GetInvoiceByIdAsync(Guid id);

    Task<IBusinessResult> DeleteInvoiceAsync(Guid id);
}

