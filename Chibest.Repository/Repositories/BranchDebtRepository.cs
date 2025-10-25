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
    public class BranchDebtRepository : GenericRepository<BranchDebt>, IBranchDebtRepository
    {
        public BranchDebtRepository(ChiBestDbContext context) : base(context) { }
    }
}
