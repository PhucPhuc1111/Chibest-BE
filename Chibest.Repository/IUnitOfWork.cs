using Chibest.Repository.Interface;
using System.Threading.Tasks;

namespace Chibest.Repository
{
    public interface IUnitOfWork
    {
        Task BeginTransaction();
        Task CommitTransaction();
        Task RollbackTransaction();
        Task SaveChangesAsync();

        IWarehouseRepository WarehouseRepository { get; }
        IAccountRepository AccountRepository { get; }
        IAccountRoleRepository AccountRoleRepository { get; }
        IRoleRepository RoleRepository { get; }
        IBranchRepository BranchRepository { get; }
    }
}