using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;

namespace Chibest.Repository.Repositories;

public class FranchiseOrderDetailRepository : GenericRepository<FranchiseOrderDetail>, IFranchiseOrderDetailRepository
{
    public FranchiseOrderDetailRepository(ChiBestDbContext context) : base(context)
    {
    }
}

