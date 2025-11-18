using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Common.Enums;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;

namespace Chibest.Service.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var role = await _unitOfWork.RoleRepository
            .GetByWhere(x => x.Id == id)
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync();
        
        if (role == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = role.Adapt<RoleResponse>();

        // Get account count for this role
        var accountCount = await _unitOfWork.AccountRoleRepository
            .GetByWhere(x => x.RoleId == id)
            .CountAsync();

        response.AccountCount = accountCount;

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetPagedAsync(int pageNumber, int pageSize, string? search = null)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        Expression<Func<Role, bool>> predicate = x => true;

        if (!string.IsNullOrWhiteSpace(search))
        {
            predicate = x => x.Name.Contains(search);
        }

        Func<IQueryable<Role>, IOrderedQueryable<Role>> orderBy =
            q => q.OrderBy(x => x.Name);

        Func<IQueryable<Role>, IQueryable<Role>> include =
            q => q.Include(x => x.Permissions);

        var roles = await _unitOfWork.RoleRepository.GetPagedAsync(
            pageNumber, pageSize, predicate, orderBy, include);

        var totalCount = await _unitOfWork.RoleRepository.CountAsync();

        // Get account counts for each role
        var roleResponses = new List<RoleResponse>();
        foreach (var role in roles)
        {
            var response = role.Adapt<RoleResponse>();
            var accountCount = await _unitOfWork.AccountRoleRepository
                .GetByWhere(x => x.RoleId == role.Id)
                .CountAsync();
            response.AccountCount = accountCount;
            roleResponses.Add(response);
        }

        var pagedResult = new PagedResult<RoleResponse>
        {
            DataList = roleResponses,
            TotalCount = totalCount,
            PageIndex = pageNumber,
            PageSize = pageSize
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, pagedResult);
    }

    public async Task<IBusinessResult> GetRoleWithAccountsAsync(Guid id)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var role = await _unitOfWork.RoleRepository.GetByIdAsync(id);
        if (role == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var accountRoles = await _unitOfWork.AccountRoleRepository
            .GetByWhere(x => x.RoleId == id)
            .Include(x => x.Account)
            .ToListAsync();

        var response = role.Adapt<RoleWithAccountsResponse>();
        response.AccountRoles = accountRoles.Select(ar => new AccountRoleResponse
        {
            AccountId = ar.AccountId,
            AccountName = ar.Account?.Name ?? "Unknown",
            AccountEmail = ar.Account?.Email ?? "Unknown"
        }).ToList();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetAllRolesAsync()
    {
        var roles = await _unitOfWork.RoleRepository
            .GetAll()
            .Include(x => x.Permissions)
            .OrderBy(x => x.Name)
            .ToListAsync();

        var roleResponses = new List<RoleResponse>();
        foreach (var role in roles)
        {
            var response = role.Adapt<RoleResponse>();
            var accountCount = await _unitOfWork.AccountRoleRepository
                .GetByWhere(x => x.RoleId == role.Id)
                .CountAsync();
            response.AccountCount = accountCount;
            roleResponses.Add(response);
        }

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, roleResponses);
    }

    public async Task<IBusinessResult> CreateAsync(RoleRequest request, Guid accountId)
    {
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Check if role name already exists
        var existingRole = await _unitOfWork.RoleRepository
            .GetByWhere(x => x.Name == request.Name)
            .FirstOrDefaultAsync();

        if (existingRole != null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Role name already exists");

        var role = request.Adapt<Role>();
        role.Id = Guid.NewGuid();

        await _unitOfWork.RoleRepository.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();


        var response = role.Adapt<RoleResponse>();
        response.AccountCount = 0;

        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG, response);
    }

    public async Task<IBusinessResult> CreateAccountRoleAsync(AccountRoleRequest request, Guid makerId)
    {
        bool validBranch = true;

        // Validate request
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Validate Account, Role
        var account = await _unitOfWork.AccountRepository.GetByIdAsync(request.AccountId);
        if (account == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG + " Account");
        var role = await _unitOfWork.RoleRepository.GetByIdAsync(request.RoleId);
        if (role == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG + " Role");
        if (request.BranchId.HasValue)
        {
            var branch = await _unitOfWork.BranchRepository.GetByIdAsync(request.BranchId);
            if (branch == null)
                validBranch = false;
        }

        // Validate if AccountRole already exists
        var existingAccRole = await _unitOfWork.AccountRoleRepository.GetByWhere(
            ar => ar.AccountId == request.AccountId &&
            ar.RoleId == request.RoleId &&
            ar.BranchId == request.BranchId)
            .FirstOrDefaultAsync();
        if (existingAccRole != null)
            return new BusinessResult(Const.HTTP_STATUS_CONFLICT, "Account already has this role assigned");

        var accRole = new AccountRole
        {
            AccountId = request.AccountId,
            RoleId = request.RoleId,
            BranchId = validBranch ? request.BranchId : null
        };

        await _unitOfWork.AccountRoleRepository.AddAsync(accRole);
        await _unitOfWork.SaveChangesAsync();


        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG);
    }

    public async Task<IBusinessResult> UpdateAsync(RoleRequest request, Guid accountId)
    {
        if (request.Id == Guid.Empty || request.Id.HasValue == false)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var existingRole = await _unitOfWork.RoleRepository.GetByIdAsync(request.Id);
        if (existingRole == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
        var oldValue = JsonSerializer.Serialize(existingRole);
        var oldName = existingRole.Name;

        // Check for duplicate name
        if (!string.IsNullOrEmpty(request.Name) && request.Name != existingRole.Name)
        {
            var duplicate = await _unitOfWork.RoleRepository
                .GetByWhere(x => x.Name == request.Name && x.Id != request.Id)
                .FirstOrDefaultAsync();

            if (duplicate != null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Role name already exists");
        }

        // Update properties
        if (!string.IsNullOrEmpty(request.Name))
            existingRole.Name = request.Name;


        _unitOfWork.RoleRepository.Update(existingRole);
        await _unitOfWork.SaveChangesAsync();


        var response = existingRole.Adapt<RoleResponse>();

        // Get account count
        var accountCount = await _unitOfWork.AccountRoleRepository
            .GetByWhere(x => x.RoleId == request.Id)
            .CountAsync();

        response.AccountCount = accountCount;

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG, response);
    }

    public async Task<IBusinessResult> ChangeRoleAccountAsync(AccountRoleRequest request, Guid whoMakeId)
    {
        var nowTime = DateTime.Now;

        if (request.AccountId == Guid.Empty || request.RoleId == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Validate Account, Role
        var account = await _unitOfWork.AccountRepository.GetByIdAsync(request.AccountId);
        if (account == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG + " AccountId");
        var newRole = await _unitOfWork.RoleRepository.GetByIdAsync(request.RoleId);
        if (newRole == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG + " RoleId");

        // Find current accountRole
        var currentAccountRoleQuery = _unitOfWork.AccountRoleRepository.GetByWhere(
            ar => ar.AccountId == request.AccountId);

        if (request.BranchId.HasValue)
            currentAccountRoleQuery = currentAccountRoleQuery.Where(ar => ar.BranchId == request.BranchId);

        var currentAccountRole = await currentAccountRoleQuery.FirstOrDefaultAsync();
        if (currentAccountRole == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
        var oldData = JsonSerializer.Serialize(currentAccountRole);
        var oldRoleId = currentAccountRole.RoleId;

        // Update Role of the account
        currentAccountRole.RoleId = request.RoleId;
        if (request.BranchId != currentAccountRole.BranchId)// if branch changed
            currentAccountRole.BranchId = request.BranchId;

        account.UpdatedAt = nowTime;

        // Save all changes
        await _unitOfWork.SaveChangesAsync();


        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
    }

    public async Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var role = await _unitOfWork.RoleRepository.GetByIdAsync(id);
        if (role == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
        var oldValue = JsonSerializer.Serialize(role);

        _unitOfWork.RoleRepository.Delete(role);
        await _unitOfWork.SaveChangesAsync();


        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }

    public async Task<IBusinessResult> DeleteAccountRoleAsync(Guid accountId, Guid roleId, Guid makerId)
    {
        // Validate input
        if (accountId == Guid.Empty || roleId == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Find Account Role
        var accRole = await _unitOfWork.AccountRoleRepository.GetByWhere(
            ar => ar.AccountId == accountId &&
            ar.RoleId == roleId)
            .FirstOrDefaultAsync();

        if (accRole == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
        var oldValue = JsonSerializer.Serialize(accRole);

        _unitOfWork.AccountRoleRepository.Delete(accRole);
        await _unitOfWork.SaveChangesAsync();


        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }

    public async Task<IBusinessResult> GetRolePermissionsAsync(Guid roleId)
    {
        if (roleId == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var role = await _unitOfWork.RoleRepository
            .GetByWhere(x => x.Id == roleId)
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync();

        if (role == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG + " Role");

        var permissions = role.Permissions.Adapt<List<PermissionResponse>>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, permissions);
    }

    public async Task<IBusinessResult> AssignPermissionsToRoleAsync(RolePermissionRequest request, Guid accountId)
    {
        if (request == null || request.RoleId == Guid.Empty || request.PermissionIds == null || !request.PermissionIds.Any())
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Get role with permissions
        var role = await _unitOfWork.RoleRepository
            .GetByWhere(x => x.Id == request.RoleId)
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync();

        if (role == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG + " Role");

        // Get permissions to assign
        var permissions = await _unitOfWork.PermissionRepository
            .GetByWhere(x => request.PermissionIds.Contains(x.Id))
            .ToListAsync();

        if (permissions.Count != request.PermissionIds.Count)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "One or more permissions not found");

        // Add only new permissions (avoid duplicates)
        foreach (var permission in permissions)
        {
            if (!role.Permissions.Any(p => p.Id == permission.Id))
            {
                role.Permissions.Add(permission);
            }
        }

        _unitOfWork.RoleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
    }

    public async Task<IBusinessResult> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, Guid accountId)
    {
        if (roleId == Guid.Empty || permissionId == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Get role with permissions
        var role = await _unitOfWork.RoleRepository
            .GetByWhere(x => x.Id == roleId)
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync();

        if (role == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG + " Role");

        var permission = role.Permissions.FirstOrDefault(p => p.Id == permissionId);
        if (permission == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Permission not found in this role");

        role.Permissions.Remove(permission);
        _unitOfWork.RoleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }

    public async Task<IBusinessResult> UpdateRolePermissionsAsync(RolePermissionRequest request, Guid accountId)
    {
        if (request == null || request.RoleId == Guid.Empty || request.PermissionIds == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Get role with current permissions
        var role = await _unitOfWork.RoleRepository
            .GetByWhere(x => x.Id == request.RoleId)
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync();

        if (role == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG + " Role");

        // Get new permissions
        var newPermissions = new List<Permission>();
        if (request.PermissionIds.Any())
        {
            newPermissions = await _unitOfWork.PermissionRepository
                .GetByWhere(x => request.PermissionIds.Contains(x.Id))
                .ToListAsync();

            if (newPermissions.Count != request.PermissionIds.Count)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "One or more permissions not found");
        }

        // Clear existing permissions and add new ones
        role.Permissions.Clear();
        foreach (var permission in newPermissions)
        {
            role.Permissions.Add(permission);
        }

        _unitOfWork.RoleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
    }
}