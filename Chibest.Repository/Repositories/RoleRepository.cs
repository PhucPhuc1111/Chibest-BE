using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;

namespace Chibest.Repository.Repositories;
public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(ChiBestDbContext context) : base(context){ }
}
