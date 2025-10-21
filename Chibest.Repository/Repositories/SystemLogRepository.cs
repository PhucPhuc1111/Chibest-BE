using Chibest.Repository.Base;
using Chibest.Repository.Interface;
using Chibest.Repository.Models;

namespace Chibest.Repository.Repositories;

public class SystemLogRepository : GenericRepository<SystemLog>, ISystemLogRepository
{
    public SystemLogRepository(ChiBestDbContext context) : base(context) { }
}