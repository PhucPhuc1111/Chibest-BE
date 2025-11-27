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

public class ColorService : IColorService
{
    private readonly IUnitOfWork _unitOfWork;

    public ColorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> GetAllAsync()
    {
        var colors = await _unitOfWork.ColorRepository
            .GetAllAsync(orderBy: query => query.OrderBy(c => c.Code));

        var response = colors.Adapt<IEnumerable<ColorResponse>>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id)
    {
        var color = await _unitOfWork.ColorRepository.GetByIdAsync(id);
        if (color == null)
        {
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
        }

        var response = color.Adapt<ColorResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> CreateAsync(ColorRequest request, Guid accountId)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Code))
        {
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);
        }

        var normalizedCode = request.Code.Trim().ToLower();

        var existingColor = await _unitOfWork.ColorRepository
            .GetByWhere(c =>
                c.Code.ToLower() == normalizedCode)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (existingColor != null)
        {
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Màu sắc đã tồn tại");
        }

        var color = new Color
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim()
        };

        await _unitOfWork.ColorRepository.AddAsync(color);
        await _unitOfWork.SaveChangesAsync();

        var response = color.Adapt<ColorResponse>();
        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG, response);
    }

    public async Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId)
    {
        var color = await _unitOfWork.ColorRepository.GetByIdAsync(id);
        if (color == null)
        {
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
        }

        _unitOfWork.ColorRepository.Delete(color);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }
}

