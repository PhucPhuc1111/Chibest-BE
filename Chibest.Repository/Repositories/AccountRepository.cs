using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;

namespace Chibest.Repository.Repositories;

public class AccountRepository : GenericRepository<Account>, IAccountRepository
{
    public AccountRepository(ChiBestDbContext context) : base(context) { }
}