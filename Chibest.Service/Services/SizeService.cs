using System;
using System.Collections.Generic;
using System.Linq;
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

public class SizeService : ISizeService
{
    private readonly IUnitOfWork _unitOfWork;

    public SizeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> GetAllAsync()
    {
        var sizes = await _unitOfWork.SizeRepository
            .GetAllAsync(orderBy: query => query.OrderBy(s => s.Code));

        var response = sizes.Adapt<IEnumerable<SizeResponse>>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id)
    {
        var size = await _unitOfWork.SizeRepository.GetByIdAsync(id);
        if (size == null)
        {
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
        }

        var response = size.Adapt<SizeResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> CreateAsync(SizeRequest request, Guid accountId)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
        {
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);
        }

        var normalizedCode = request.Code.Trim().ToLower();

        var existingSize = await _unitOfWork.SizeRepository
            .GetByWhere(s => s.Code.ToLower() == normalizedCode)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (existingSize != null)
        {
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Size đã tồn tại");
        }

        var size = new Size
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim()
        };

        await _unitOfWork.SizeRepository.AddAsync(size);
        await _unitOfWork.SaveChangesAsync();

        var response = size.Adapt<SizeResponse>();
        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG, response);
    }

    public async Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId)
    {
        var size = await _unitOfWork.SizeRepository.GetByIdAsync(id);
        if (size == null)
        {
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
        }

        _unitOfWork.SizeRepository.Delete(size);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }
}

