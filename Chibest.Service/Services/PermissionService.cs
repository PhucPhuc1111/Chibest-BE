using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Chibest.Service.Services;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HasPermissionAsync(Guid accountId, params string[] permissionCodes)
    {
        if (accountId == Guid.Empty)
        {
            return false;
        }

        var normalized = permissionCodes?
            .Where(code => string.IsNullOrWhiteSpace(code) == false)
            .Select(code => code.Trim().ToUpperInvariant())
            .Distinct()
            .ToArray() ?? Array.Empty<string>();

        if (normalized.Length == 0)
        {
            return false;
        }

        var activeRoles = await _unitOfWork.AccountRoleRepository
            .GetByWhere(ar => ar.AccountId == accountId &&
                (ar.EndDate == null || ar.EndDate > DateTime.Now))
            .Include(ar => ar.Role)
                .ThenInclude(role => role.Permissions)
            .ToListAsync();

        if (activeRoles.Count == 0)
        {
            return false;
        }

        if (activeRoles.Any(ar =>
                string.Equals(ar.Role.Name, Const.Roles.Admin, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        var grantedPermissions = activeRoles
            .SelectMany(ar => ar.Role.Permissions)
            .Select(permission => permission.Code)
            .Where(code => string.IsNullOrWhiteSpace(code) == false)
            .Select(code => code.Trim().ToUpperInvariant())
            .ToHashSet();

        return normalized.Any(grantedPermissions.Contains);
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var permission = await _unitOfWork.PermissionRepository.GetByIdAsync(id);
        if (permission == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = permission.Adapt<PermissionResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetAllAsync()
    {
        var permissions = await _unitOfWork.PermissionRepository
            .GetAll()
            .OrderBy(x => x.Code)
            .ToListAsync();

        var response = permissions.Adapt<List<PermissionResponse>>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetPagedAsync(int pageNumber, int pageSize, string? search = null)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        Expression<Func<Permission, bool>> predicate = x => true;

        if (!string.IsNullOrWhiteSpace(search))
        {
            predicate = x => x.Code.Contains(search);
        }

        Func<IQueryable<Permission>, IOrderedQueryable<Permission>> orderBy =
            q => q.OrderBy(x => x.Code);

        var permissions = await _unitOfWork.PermissionRepository.GetPagedAsync(
            pageNumber, pageSize, predicate, orderBy);

        var totalCount = await _unitOfWork.PermissionRepository.CountAsync();

        var permissionResponses = permissions.Adapt<List<PermissionResponse>>();

        var pagedResult = new PagedResult<PermissionResponse>
        {
            DataList = permissionResponses,
            TotalCount = totalCount,
            PageIndex = pageNumber,
            PageSize = pageSize
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, pagedResult);
    }

    public async Task<IBusinessResult> CreateAsync(PermissionRequest request, Guid accountId)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Code))
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Check if permission code already exists
        var existingPermission = await _unitOfWork.PermissionRepository
            .GetByWhere(x => x.Code == request.Code.Trim().ToUpperInvariant())
            .FirstOrDefaultAsync();

        if (existingPermission != null)
            return new BusinessResult(Const.HTTP_STATUS_CONFLICT, "Permission code already exists");

        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim().ToUpperInvariant()
        };

        await _unitOfWork.PermissionRepository.AddAsync(permission);
        await _unitOfWork.SaveChangesAsync();

        var response = permission.Adapt<PermissionResponse>();
        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG, response);
    }

    public async Task<IBusinessResult> UpdateAsync(PermissionRequest request, Guid accountId)
    {
        if (request.Id == Guid.Empty || request.Id.HasValue == false)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var existingPermission = await _unitOfWork.PermissionRepository.GetByIdAsync(request.Id);
        if (existingPermission == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        // Check for duplicate code
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var normalizedCode = request.Code.Trim().ToUpperInvariant();
            if (normalizedCode != existingPermission.Code)
            {
                var duplicate = await _unitOfWork.PermissionRepository
                    .GetByWhere(x => x.Code == normalizedCode && x.Id != request.Id)
                    .FirstOrDefaultAsync();

                if (duplicate != null)
                    return new BusinessResult(Const.HTTP_STATUS_CONFLICT, "Permission code already exists");

                existingPermission.Code = normalizedCode;
            }
        }

        _unitOfWork.PermissionRepository.Update(existingPermission);
        await _unitOfWork.SaveChangesAsync();

        var response = existingPermission.Adapt<PermissionResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG, response);
    }

    public async Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var permission = await _unitOfWork.PermissionRepository
            .GetByWhere(x => x.Id == id)
            .Include(x => x.Roles)
            .FirstOrDefaultAsync();

        if (permission == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        // Check if permission is assigned to any roles
        if (permission.Roles != null && permission.Roles.Any())
            return new BusinessResult(Const.HTTP_STATUS_CONFLICT, 
                "Cannot delete permission that is assigned to roles");

        _unitOfWork.PermissionRepository.Delete(permission);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }
}

