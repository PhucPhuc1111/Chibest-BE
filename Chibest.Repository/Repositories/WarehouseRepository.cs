using Chibest.Repository.Interface;
using Chibest.Repository.Models;
using NutriDiet.Repository.Repositories;
namespace Chibest.Repository.Repositories
{
    public class WarehouseRepository : GenericRepository<Warehouse>, IWarehouseRepository
    {
        public WarehouseRepository(ChiBestDbContext context) : base(context) { }
    }
}
