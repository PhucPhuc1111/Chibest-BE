using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository.Models;
using Chibest.Service.ModelDTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.Interface
{
    public interface ISupplierDebtService
    {
        Task<IBusinessResult> AddSupplierTransactionAsync(
    Guid supplierDebtId,
    List<SupplierDebtHistoryRequest> transactions);
        Task<IBusinessResult> GetSupplierDebtList(int pageIndex,
        int pageSize,
        string? search = null,
        decimal? totalFrom = null,
        decimal? totalTo = null,
        string? datePreset = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        decimal? debtFrom = null,
        decimal? debtTo = null);
        Task<IBusinessResult> GetSupplierDebtAsync(Guid id, string transactionType = null);
        Task<IBusinessResult> UpdateSupplierDebtHistoryAsync(Guid supplierDebtId, Guid historyId, SupplierDebtHistoryUpdateRequest request);
        Task<IBusinessResult> DeleteSupplierDebtHistoryAsync(Guid supplierDebtId, Guid historyId);
        Task<byte[]> ExportSupplierDebtToExcelAsync();

    }
}