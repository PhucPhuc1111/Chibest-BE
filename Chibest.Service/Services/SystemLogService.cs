using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Response;
using Chibest.Service.Utilities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chibest.Service.Services;

public class SystemLogService : ISystemLogService
{
    private readonly IUnitOfWork _unitOfWork;

    public SystemLogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var log = await _unitOfWork.SystemLogRepository.GetByIdAsync(id);
        if (log == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = log.Adapt<SystemLogResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }
    public async Task<IBusinessResult> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        string? logLevel = null,
        string? module = null)
    {
        try
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            Expression<Func<SystemLog, bool>> predicate = x => true;

            if (!string.IsNullOrWhiteSpace(search))
            {
                predicate = x => x.Action.Contains(search) ||
                                x.EntityType.Contains(search) ||
                                x.Description.Contains(search) ||
                                x.AccountName.Contains(search);
            }

            if (!string.IsNullOrWhiteSpace(logLevel))
            {
                predicate = predicate.And(x => x.LogLevel == logLevel);
            }

            if (!string.IsNullOrWhiteSpace(module))
            {
                predicate = predicate.And(x => x.Module == module);
            }

            Func<IQueryable<SystemLog>, IOrderedQueryable<SystemLog>> orderBy =
                q => q.OrderByDescending(x => x.CreatedAt);

            var logs = await _unitOfWork.SystemLogRepository.GetPagedAsync(
                pageNumber, pageSize, predicate, orderBy);

            var totalCount = await _unitOfWork.SystemLogRepository.CountAsync();

            var response = logs.Adapt<List<SystemLogResponse>>();
            var pagedResult = new PagedResult<SystemLogResponse>
            {
                DataList = response,
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


    public async Task<IBusinessResult> CreateAsync(SystemLogRequest request)
    {
        try
        {
            if (request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

            var log = request.Adapt<SystemLog>();
            log.Id = Guid.NewGuid();
            log.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.SystemLogRepository.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();

            var response = log.Adapt<SystemLogResponse>();
            return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG, response);
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

            var log = await _unitOfWork.SystemLogRepository.GetByIdAsync(id);
            if (log == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            _unitOfWork.SystemLogRepository.Delete(log);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> DeleteOldLogsAsync(DateTime olderThan)
    {
        try
        {
            var oldLogs = await _unitOfWork.SystemLogRepository
                .GetByWhere(x => x.CreatedAt < olderThan)
                .ToListAsync();

            if (oldLogs.Any())
            {
                _unitOfWork.SystemLogRepository.RemoveRange(oldLogs);
                await _unitOfWork.SaveChangesAsync();
            }

            return new BusinessResult(Const.HTTP_STATUS_OK, $"Deleted {oldLogs.Count} old logs");
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }
}