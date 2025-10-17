using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;

namespace Chibest.Repository.Repositories;

public class AccountRoleRepository : GenericRepository<AccountRole>, IAccountRoleRepository
{
    public AccountRoleRepository(ChiBestDbContext context) : base(context){}
}