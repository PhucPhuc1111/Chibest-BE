using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        var role = await _unitOfWork.RoleRepository.GetByIdAsync(id);
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
        try
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            Expression<Func<Role, bool>> predicate = x => true;

            if (!string.IsNullOrWhiteSpace(search))
            {
                predicate = x => x.Name.Contains(search) ||
                                x.Description.Contains(search);
            }

            Func<IQueryable<Role>, IOrderedQueryable<Role>> orderBy =
                q => q.OrderBy(x => x.Name);

            var roles = await _unitOfWork.RoleRepository.GetPagedAsync(
                pageNumber, pageSize, predicate, orderBy);

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
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> CreateAsync(RoleRequest request)
    {
        try
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
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> UpdateAsync(RoleRequest request)
    {
        try
        {
            if (request.Id == Guid.Empty || request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

            var role = await _unitOfWork.RoleRepository.GetByIdAsync(request.Id);
            if (role == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            // Check for duplicate name
            if (!string.IsNullOrEmpty(request.Name) && request.Name != role.Name)
            {
                var duplicate = await _unitOfWork.RoleRepository
                    .GetByWhere(x => x.Name == request.Name && x.Id != request.Id)
                    .FirstOrDefaultAsync();

                if (duplicate != null)
                    return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Role name already exists");
            }

            // Update properties
            if (!string.IsNullOrEmpty(request.Name))
                role.Name = request.Name;

            if (request.Description != null)
                role.Description = request.Description;

            _unitOfWork.RoleRepository.Update(role);
            await _unitOfWork.SaveChangesAsync();

            var response = role.Adapt<RoleResponse>();

            // Get account count
            var accountCount = await _unitOfWork.AccountRoleRepository
                .GetByWhere(x => x.RoleId == request.Id)
                .CountAsync();

            response.AccountCount = accountCount;

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG, response);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

            var role = await _unitOfWork.RoleRepository.GetByIdAsync(id);
            if (role == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            // Check if role has accounts assigned
            var hasAccounts = await _unitOfWork.AccountRoleRepository
                .GetByWhere(x => x.RoleId == id)
                .AnyAsync();

            if (hasAccounts)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Cannot delete role that has accounts assigned");

            _unitOfWork.RoleRepository.Delete(role);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> GetRoleWithAccountsAsync(Guid id)
    {
        try
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
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> GetAllRolesAsync()
    {
        try
        {
            var roles = await _unitOfWork.RoleRepository
                .GetAll()
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
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> ChangeRoleAccountAsync(Guid accountId, Guid roleId)
    {
        var nowTime = DateTime.Now;
        if (accountId == Guid.Empty || roleId == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Validate Account, Role
        var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
        if (account == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
        var role = await _unitOfWork.RoleRepository.GetByIdAsync(roleId);
        if (role == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        // Find current active AccountRole
        var accountRole = await _unitOfWork.AccountRoleRepository.GetByWhere(
            ar => ar.AccountId == accountId &&
            (ar.EndDate == null || ar.EndDate > nowTime))
            .FirstOrDefaultAsync();
        if (accountRole == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        // End current role
        accountRole.EndDate = nowTime;
        _unitOfWork.AccountRoleRepository.Update(accountRole);

        // Then create new role with old branch
        var newAccountRole = new AccountRole
        {
            AccountId = accountId,
            RoleId = roleId,
            BranchId = accountRole.BranchId,
            StartDate = nowTime,
            EndDate = null
        };
        await _unitOfWork.AccountRoleRepository.AddAsync(newAccountRole);

        // Update account's UpdatedAt
        account.UpdatedAt = nowTime;
        _unitOfWork.AccountRepository.Update(account);

        // Save all changes
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
    }
}