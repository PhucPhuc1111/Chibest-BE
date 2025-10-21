using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;

namespace Chibest.Repository.Repositories;

public class BranchStockRepository : GenericRepository<BranchStock>, IBranchStockRepository
{
    public BranchStockRepository(ChiBestDbContext context) : base(context) { }
}