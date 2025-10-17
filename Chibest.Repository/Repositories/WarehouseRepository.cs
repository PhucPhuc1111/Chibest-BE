using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;
namespace Chibest.Repository.Repositories
{
    public class WarehouseRepository : GenericRepository<Warehouse>, IWarehouseRepository
    {
        public WarehouseRepository(ChiBestDbContext context) : base(context) { }
    }
}
