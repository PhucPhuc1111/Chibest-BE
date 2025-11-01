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
    public class BranchDebtHistoryRepository : GenericRepository<BranchDebtHistory>, IBranchDebtHistoryRepository
    {
        public BranchDebtHistoryRepository(ChiBestDbContext context) : base(context) { }
    }
}
