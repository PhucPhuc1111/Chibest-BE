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
    public interface ISupplierDebtRepository : IGenericRepository<SupplierDebt>
    {
        Task<IBusinessResult> AddSupplierTransactionAsync(
            Guid supplierId,
            string transactionType,
            decimal amount, string? note = null);
    }
}
