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
    public class StockAdjusmentRepository : GenericRepository<StockAdjustment>, IStockAdjusmentRepository
    {
        public StockAdjusmentRepository(ChiBestDbContext context) : base(context) { }
    }
}
