using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Repository.Repositories
{
    public class StockAdjustmentDetailRepository : GenericRepository<StockAdjustmentDetail>, IStockAdjustmentDetailRepository
    {
        public StockAdjustmentDetailRepository(ChiBestDbContext context) : base(context) { }
    }
}
