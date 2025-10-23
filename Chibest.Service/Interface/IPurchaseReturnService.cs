﻿using Chibest.Common.BusinessResult;
using Chibest.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chibest.Service.ModelDTOs.Stock.PurchaseReturn.create;

namespace Chibest.Service.Interface
{
    public interface IPurchaseReturnService
    {
        Task<IBusinessResult> AddPurchaseReturn(PurchaseReturnCreate request);
        Task<IBusinessResult> GetPurchaseReturnById(Guid id);
        Task<IBusinessResult> GetPurchaseReturnList(
            int pageIndex,
            int pageSize,
            string search,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string status = null);
        Task<IBusinessResult> UpdatePurchaseReturnAsync(Guid id, OrderStatus status);
    }
}
