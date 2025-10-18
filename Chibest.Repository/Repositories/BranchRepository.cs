using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;

namespace Chibest.Repository.Repositories
{
    public class BranchRepository : GenericRepository<Branch>, IBranchRepository
    {
        public BranchRepository(ChiBestDbContext context) : base(context) { }
    }
}
