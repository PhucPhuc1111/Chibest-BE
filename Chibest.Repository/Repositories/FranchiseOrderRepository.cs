using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;

namespace Chibest.Repository.Repositories;

public class FranchiseOrderRepository : GenericRepository<FranchiseOrder>, IFranchiseOrderRepository
{
    public FranchiseOrderRepository(ChiBestDbContext context) : base(context)
    {
    }
}

