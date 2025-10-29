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
    Guid supplierId,
    List<SupplierDebtHistoryRequest> transactions);
        Task<IBusinessResult> GetSupplierDebtList(int pageIndex, int pageSize, string search);
        Task<IBusinessResult> GetSupplierDebtAsync(Guid id);
        Task<IBusinessResult> DeleteSupplierDebtHistoryAsync(Guid supplierDebtId, Guid historyId);

    }
}