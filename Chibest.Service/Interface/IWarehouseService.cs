using Chibest.Common.BusinessResult;
using Chibest.Service.ModelDTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.Interface
{
    public interface IWarehouseService
    {
        Task<IBusinessResult> GetWarehouseById(Guid id);
        Task<IBusinessResult> CreateWarehouse(WarehouseRequest request);
        Task<IBusinessResult> GetWarehouseList(int pageIndex, int pageSize, string allergyName);
        Task<IBusinessResult> UpdateWarehouse(Guid id, WarehouseRequest request);
        Task<IBusinessResult> DeleteWarehouse(Guid id);
    }
}
