using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;

namespace Chibest.Repository.Repositories;

public class ProductPlanRepository : GenericRepository<ProductPlan>, IProductPlanRepository
{
    public ProductPlanRepository(ChiBestDbContext context) : base(context)
    {
    }
}

